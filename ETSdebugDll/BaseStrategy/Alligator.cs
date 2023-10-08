using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Indicators.Model;
using ScriptSolution.Model;
using ScriptSolution.Model.Interfaces;
using SourceEts.Models;

namespace ETSBots
{
    public class Alligator : Script
    {
        public CreateIndicator Alligators = new CreateIndicator(EnumIndicators.Alligator, 0, "");
        public CreateIndicator Fractals = new CreateIndicator(EnumIndicators.Fractals, 0, "");
        public CreateIndicator Ao = new CreateIndicator(EnumIndicators.Ao, 1, "");

        public ParamOptimization DiffTrendCancellation = new ParamOptimization(1, 1, 500, 1, "DiffTrendCancellation", "");

        IPosition _long;
        IPosition _short;

        bool _buySignal = false;
        bool _sellSignal = false;

        List<double> _alligatorsFast;
        List<double> _alligatorsMiddle;
        List<double> _alligatorsSlow;
        List<double> _fractalsUp;
        List<double> _fractalsDown;
        List<double> _ao;

        double _longEntryPrice;
        double _shortEntryPrice;
        double _longStopPrice;
        double _shortStopPrice;

        public override void Execute()
        {
            Indicators();

            if (!Readiness())
                return;
            
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                DetectNearestLevel(bar);
                Signals(bar);
                Entry(bar);
                ClosePosition(bar);

                #region Закрытие позиции по времени, встроенным стопам и рискам
                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                    SendClosePosOnRiskFromForm(bar + 1, "");
                }
                #endregion
            }
        }

        private bool Readiness()
        {
            if (_alligatorsFast.Count == 0 || _alligatorsMiddle.Count == 0 || _alligatorsSlow.Count == 0
                || _fractalsUp.Count == 0 || _fractalsDown.Count == 0 || _ao.Count == 0)
                return false;
            else
                return true;
        }

        private void Indicators()
        {
            _alligatorsFast = Alligators.param.LinesIndicators[2].PriceSeries;
            _alligatorsMiddle = Alligators.param.LinesIndicators[1].PriceSeries;
            _alligatorsSlow = Alligators.param.LinesIndicators[0].PriceSeries;
            _fractalsUp = Fractals.param.LinesIndicators[0].PriceSeries;
            _fractalsDown = Fractals.param.LinesIndicators[1].PriceSeries;
            _ao = Ao.param.LinesIndicators[0].PriceSeries;
        }

        private void Signals(int bar)
        {
            if (CandleCount <= 2)
                return;

            if (_ao[bar - 1] < 0 && _ao[bar] > 0)
            {
                _buySignal = true;
                _sellSignal = false;
            } 
            if (_buySignal && _ao[bar] >= DiffTrendCancellation.Value)
                _buySignal = false;

            if (_ao[bar - 1] > 0 && _ao[bar] < 0)
            {
                _sellSignal = true;
                _buySignal = false;
            }
            if (_sellSignal && _ao[bar] <= -1 * DiffTrendCancellation.Value)
                _sellSignal = false;
        }

        private void DetectNearestLevel(int bar)
        {
            if (_buySignal)
            {
                for (int i = CandleCount - 1; i > 0; i--)
                {
                    if (_fractalsUp[i] > _alligatorsFast[bar] && _fractalsUp[i] > Candles.HighSeries[bar])
                    {
                        _longEntryPrice = _fractalsUp[i];
                        break;
                    }
                }
            }
            if (_sellSignal)
            {
                for (int i = CandleCount - 1; i > 0; i--)
                {
                    if (_fractalsDown[i] < _alligatorsFast[bar] && _fractalsDown[i] < Candles.LowSeries[bar])
                    {
                        _shortEntryPrice = _fractalsDown[i];
                        break;
                    }
                }
            }
        }

        private void Entry(int bar)
        {
            if (_buySignal && _alligatorsFast[bar] > _alligatorsMiddle[bar] && _alligatorsMiddle[bar] > _alligatorsSlow[bar] 
                && LongPos.Count == 0 && ShortPos.Count == 0 && _longEntryPrice > 0)
            {
                _long = BuyGreater(bar + 1, _longEntryPrice, 1, "Long");
                _longStopPrice = _fractalsDown[bar];
            }
            if (_sellSignal && _alligatorsFast[bar] < _alligatorsMiddle[bar] && _alligatorsMiddle[bar] < _alligatorsSlow[bar]
                && LongPos.Count == 0 && ShortPos.Count == 0 && _shortEntryPrice > 0)
            {
                _short = ShortLess(bar + 1, _shortEntryPrice, 1, "Short");
                _shortStopPrice = _fractalsUp[bar];
            }
        }

        private void ClosePosition(int bar)
        {
            if (LongPos.Count > 0 && Candles.CloseSeries[bar] < _alligatorsMiddle[bar])
            {
                SellAtMarket(bar + 1, _long, "long stop alligator fast");
            }
            if (ShortPos.Count > 0 && Candles.CloseSeries[bar] > _alligatorsMiddle[bar])
            {
                CoverAtMarket(bar + 1, _short, "short stop alligator fast");
            }
            if (LongPos.Count > 0)
            {
                SellAtStop(bar + 1, _long, _longStopPrice, "long stoploss");
            }
            if (ShortPos.Count > 0)
            {
                CoverAtStop(bar + 1, _short, _shortStopPrice, "short stoploss");
            }
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 6, 21);
            DesParamStratetgy.DateChange = new DateTime(2021, 6, 21);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/gtsn51s0u1-robot-alligator";
            DesParamStratetgy.NameStrategy = "Alligator";
        }
    }
}
