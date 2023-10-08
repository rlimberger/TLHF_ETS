using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using SourceEts.Config;
using SourceEts.Models;


namespace ETSBots
{
    public class PinBar : Script
    {
        public CreateIndicator Fractal = new CreateIndicator(EnumIndicators.Fractals, 0, "");

        public ParamOptimization Range = new ParamOptimization(5, 1, 500, 1, "Range", "");
        public ParamOptimization Stop = new ParamOptimization(5, 1, 500, 1, "Stop", "Stop ticks");
        public ParamOptimization Multiplicity = new ParamOptimization(2, 1, 500, 1, "Множитель", "Кратность размера тейка относительно стопа");


        IPosition _long;
        IPosition _short;

        List<double> _fractalUp;
        List<double> _fractalDown;

        double _levelLong;
        double _levelShort;

        string _signal;

        double _longStopLoss;
        double _shortStopLoss;


        public override void Execute()
        {
            _fractalUp = Fractal.param.LinesIndicators[0].PriceSeries;
            _fractalDown = Fractal.param.LinesIndicators[1].PriceSeries;


            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                Levels(bar);
                PinBarDetection(bar);
                Entry(bar);
                Exit(bar);


                ParamDebug("long", _levelLong);
                ParamDebug("short", _levelShort);




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

        private void Levels(int bar)
        {
            if (_fractalUp[bar] < Candles.CloseSeries[bar])
                _levelLong = _fractalUp[bar];
            else
                _levelLong = 0;

            if (_fractalDown[bar] > Candles.CloseSeries[bar])
                _levelShort = _fractalDown[bar];
            else
                _levelShort = 0;
        }

        private void PinBarDetection(int bar)
        {
            double size = Math.Abs(Candles.HighSeries[bar] - Candles.LowSeries[bar]);
            double tail = 0;

            if (_levelLong > 0)
                tail = Candles.CloseSeries[bar] > Candles.OpenSeries[bar] ? Candles.OpenSeries[bar] - Candles.LowSeries[bar] : Candles.CloseSeries[bar] - Candles.LowSeries[bar];
            else if (_levelShort > 0)
                tail = Candles.CloseSeries[bar] > Candles.OpenSeries[bar] ? Candles.HighSeries[bar] - Candles.CloseSeries[bar] : Candles.HighSeries[bar] - Candles.OpenSeries[bar];

            double percent = (tail * 100) / size;

            if (_levelLong > 0 && percent >= 60 && Math.Abs(_levelLong - Candles.LowSeries[bar]) <= Range.Value * FinInfo.Security.MinStep)
            {
                _signal = "buy";
                PlotArea("buy", bar, new UserChartPropModel() { ColorLine = Color.GreenYellow });
            }
            else if (_levelShort > 0 && percent >= 60 && Math.Abs(_levelShort - Candles.HighSeries[bar]) <= Range.Value * FinInfo.Security.MinStep)
            {
                _signal = "sell";
                PlotArea("sell", bar, new UserChartPropModel() { ColorLine = Color.OrangeRed });
            }
            else
            {
                _signal = "";
            }
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _signal == "buy")
            {
                _long = BuyAtMarket(bar + 1, 1, "Long");
                _longStopLoss = _levelLong - Stop.Value * FinInfo.Security.MinStep;
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _signal == "sell")
            {
                _short = ShortAtMarket(bar + 1, 1, "Short");
                _shortStopLoss = _levelShort + Stop.Value * FinInfo.Security.MinStep;
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                double longProfitTarget = _long.EntryPrice + (_long.EntryPrice - _longStopLoss) * Multiplicity.Value;
                SellAtProfit(bar + 1, _long, longProfitTarget, "Long take");
                SellAtStop(bar + 1, _long, _longStopLoss, "Long stop");
            }
            if (ShortPos.Count > 0)
            {
                double shortProfitTarget = _short.EntryPrice - (_shortStopLoss - _short.EntryPrice) * Multiplicity.Value;
                CoverAtProfit(bar + 1, _short, shortProfitTarget, "Short take");
                CoverAtStop(bar + 1, _short, _shortStopLoss, "Short stop");
            }
        }

        public override void SetSettingDefault()
        {
            Fractal.param.LinesIndicators[0].LineParam[0].Value = 50;
            Fractal.param.LinesIndicators[1].LineParam[0].Value = 50;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 20);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 20);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/frunlixd51-robot-pinbar";
            DesParamStratetgy.NameStrategy = "PinBar";
        }
    
    }
}
