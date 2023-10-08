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
    public class TrendMomentum : Script
    {
        public CreateIndicator Ma = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "");
        public CreateIndicator Momentum = new CreateIndicator(EnumIndicators.Momentum, 1, "");

        public ParamOptimization MomentumTrigger = new ParamOptimization(100, 1, 50000, 1, "MomentumTrigger", "Momentum entry trigger");

        IPosition _long;
        IPosition _short;

        List<double> _ma;
        List<double> _momentum;

        string _candleSignal;

        public override void Execute()
        {
            if (CandleCount <= 2)
                return;

            _ma = Ma.param.LinesIndicators[0].PriceSeries;
            _momentum = Momentum.param.LinesIndicators[0].PriceSeries;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                CandleCondition(bar + 1);
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

        private void CandleCondition(int bar)
        {
            double _candleSize = Math.Abs(Candles.CloseSeries[bar] - Candles.OpenSeries[bar]);

            if (Candles.OpenSeries[bar] < _ma[bar] && Candles.CloseSeries[bar] > _ma[bar] && Candles.OpenSeries[bar] < Candles.CloseSeries[bar])
            {
                double value = (_ma[bar] - Candles.OpenSeries[bar]) * 100 / _candleSize;

                if (value >= 80)
                    _candleSignal = "buy";
            }
            else if (Candles.OpenSeries[bar] < _ma[bar] && Candles.CloseSeries[bar] > _ma[bar] && Candles.OpenSeries[bar] > Candles.CloseSeries[bar])
            {
                double value = (_ma[bar] - Candles.CloseSeries[bar]) * 100 / _candleSize;

                if (value >= 80)
                    _candleSignal = "sell";
            }
            else if (Candles.OpenSeries[bar] < Candles.CloseSeries[bar] && Candles.OpenSeries[bar] > _ma[bar])
                _candleSignal = "buy";
            else if (Candles.OpenSeries[bar] > Candles.CloseSeries[bar] && Candles.OpenSeries[bar] < _ma[bar])
                _candleSignal = "sell";
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _candleSignal == "buy" && _momentum[bar] > MomentumTrigger.Value)
            {
                _long = BuyAtMarket(bar + 1, 1, "Long");
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _candleSignal == "sell" && _momentum[bar] < MomentumTrigger.Value)
            {
                _short = ShortAtMarket(bar + 1, 1, "Short");
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0 && Candles.DateTimeCandle[bar].Day != Candles.DateTimeCandle[bar + 1].Day && _long.EntryTime.Day != Candles.DateTimeCandle[bar + 1].Day)
            {
                SellAtMarket(bar + 1, _long, "long stop " + _long.EntryPrice.ToString());
            }
            if (ShortPos.Count > 0 && Candles.DateTimeCandle[bar].Day != Candles.DateTimeCandle[bar + 1].Day && _short.EntryTime.Day != Candles.DateTimeCandle[bar + 1].Day)
            {
                CoverAtMarket(bar + 1, _short, "short stop " + _short.EntryPrice.ToString());
            }
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 06, 22);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 22);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/k75k42fjs1-robot-trendmomentum";
            DesParamStratetgy.NameStrategy = "TrendMomentum";

        }
    }
}
