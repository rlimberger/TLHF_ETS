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
    public class MacdSar : Script
    {
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");
        public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");


        public ParamOptimization Multiplicity = new ParamOptimization(2, 1, 500, 1, "Multiplicity", "Кратность размера тейка относительно стопа");


        List<double> _parabolic;
        List<double> _prevParabolic;
        List<double> _macdB;
        List<double> _macdG;

        IPosition _long;
        IPosition _short;

        double _longStopLoss;
        double _shortStopLoss;

        bool _macdLong = false;
        bool _macdShort = false;
        bool _sarLong = false;
        bool _sarShort = false;

        public override void Execute()
        {
            if (CandleCount <= 2)
                return;


            _parabolic = Sar.param.LinesIndicators[0].PriceSeries;
            _prevParabolic = Sar.param.LinesIndicators[1].PriceSeries;
            _macdB = Macd.param.LinesIndicators[0].PriceSeries;
            _macdG = Macd.param.LinesIndicators[1].PriceSeries;


            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                MacdSignal(bar);
                SarSignal(bar);
                 
                Entry(bar);
                Exit(bar);

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

        private void MacdSignal(int bar)
        {
            if (LongPos.Count != 0 || ShortPos.Count != 0)
                return;

            if (_macdB[bar - 1] < _macdG[bar - 1] && _macdB[bar] > _macdG[bar])
            {
                _macdLong = true;
                _macdShort = false;
            }
            if (_macdB[bar - 1] > _macdG[bar - 1] && _macdB[bar] < _macdG[bar])
            {
                _macdShort = true;
                _macdLong = false;
            }
        }

        private void SarSignal(int bar)
        {
            if (LongPos.Count != 0 || ShortPos.Count != 0)
                return;

            if (_parabolic[bar - 1] > Candles.HighSeries[bar - 1] && _parabolic[bar] < Candles.LowSeries[bar])
            {
                _sarLong = true;
                _sarShort = false;
            }
            if (_parabolic[bar - 1] < Candles.LowSeries[bar - 1] && _parabolic[bar] > Candles.HighSeries[bar])
            {
                _sarShort = true;
                _sarLong = false;
            }

        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _sarLong && _macdLong && _parabolic[bar + 1] < Candles.OpenSeries[bar + 1])
            {
                if (_long != null && _long.ExitBar == bar + 1)
                    return;

                _long = BuyAtMarket(bar + 1, 1, "Long");
                _longStopLoss = _parabolic[bar];
                _sarLong = false;
                _macdLong = false;
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _sarShort && _macdShort && _parabolic[bar + 1] > Candles.OpenSeries[bar + 1])
            {
                if (_short != null && _short.ExitBar == bar + 1)
                    return;

                _short = ShortAtMarket(bar + 1, 1, "Short");
                _shortStopLoss = _parabolic[bar];
                _sarShort = false;
                _macdShort = false;
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                double longProfitTarget = _long.EntryPrice + (_long.EntryPrice - _longStopLoss) * Multiplicity.Value;
                SellAtProfit(bar + 1, _long, longProfitTarget, "long take");
                SellAtStop(bar + 1, _long, _longStopLoss, "long stop");
            }
            if (ShortPos.Count > 0)
            {
                double shortProfitTarget = _short.EntryPrice - (_shortStopLoss - _short.EntryPrice) * Multiplicity.Value;
                CoverAtProfit(bar + 1, _short, shortProfitTarget, "short take");
                CoverAtStop(bar + 1, _short, _shortStopLoss, "short stop");
            }
        }



        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 06, 23);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 23);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357338438";
            DesParamStratetgy.NameStrategy = "MacdSar";
        }
    }
}
