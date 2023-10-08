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
    public class LindaRashke : Script
    {
        public ParamOptimization Percent = new ParamOptimization(100, 1, 500, 1, "Bar size", "");
        public ParamOptimization Stop = new ParamOptimization(50, 1, 500, 1, "Stop size", "");

        IPosition _long;
        IPosition _short;


        double _avgSize;
        double _longEntry;
        double _shortEntry;
        double _longStopLoss;
        double _shortStopLoss;
        double _longTarget;
        double _shortTarget;


        bool _signal;

        DateTime _time = DateTime.MinValue;


        public override void Execute()
        {



            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                CalcCandles(bar);
                Signal(bar + 1);
                Entry(bar);
                Exit(bar);

                ParamDebug("Signal", _signal);
                ParamDebug("Long", _longEntry);
                ParamDebug("Short", _shortEntry);
                ParamDebug("LongProfit", _longTarget);
                ParamDebug("ShortProfit", _shortTarget);
                ParamDebug("LongStop", _longStopLoss);
                ParamDebug("ShortStop", _shortStopLoss);


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

        private void CalcCandles(int bar)
        {
            if (bar <= 100)
                return;

            List<double> _candleSize = new List<double>();

            for (var i = bar; i > bar - 100; i--)
            {
                _candleSize.Add(Candles.HighSeries[i] - Candles.LowSeries[i]);
            }
                
            _avgSize = _candleSize.Sum() / _candleSize.Count;

            double trigger = _avgSize * (100 + Percent.ValueInt) / 100;

            double highTail = Candles.CloseSeries[bar] > Candles.OpenSeries[bar] ? Candles.HighSeries[bar] - Candles.CloseSeries[bar] : Candles.HighSeries[bar] - Candles.OpenSeries[bar];
            double lowTail = Candles.CloseSeries[bar] > Candles.OpenSeries[bar] ? Candles.OpenSeries[bar] - Candles.LowSeries[bar] : Candles.CloseSeries[bar] - Candles.LowSeries[bar];

            double highPercent = highTail * 100 / _candleSize[0];
            double lowPercent = lowTail * 100 / _candleSize[0];

            if (trigger <= _candleSize[0] && highPercent + lowPercent <= 20)
            {
                _signal = true;
                PlotArea("Signal", bar, new UserChartPropModel { ColorLine = Color.DarkViolet });
            }
        }

        private void Signal(int bar)
        {
            double size = 0;

            if (Candles.OpenSeries[bar] > Candles.CloseSeries[bar] && _signal && Candles.OpenSeries[bar] < Candles.HighSeries[bar - 1]
                && Candles.OpenSeries[bar] > Candles.LowSeries[bar - 1] && LongPos.Count == 0 && ShortPos.Count == 0)
            {
                size = (Candles.OpenSeries[bar] - Candles.CloseSeries[bar]) / FinInfo.Security.MinStep;

                if (size >= 15 && Candles.OpenSeries[bar - 1] > Candles.CloseSeries[bar - 1])
                {
                    PlotArea("SignalEntryLong", bar, new UserChartPropModel { ColorLine = Color.YellowGreen });
                    _longEntry = Candles.CloseSeries[bar - 1];
                    _longTarget = Candles.HighSeries[bar - 1];
                    _longStopLoss = Candles.CloseSeries[bar - 1] - Stop.Value * FinInfo.Security.MinStep;
                    _time = Candles.DateTimeCandle[bar];
                }
                else
                {
                    _shortEntry = 0;
                    _shortStopLoss = 0;
                    _shortTarget = 0;
                    _signal = false;
                }
            }
            else if (Candles.OpenSeries[bar] < Candles.CloseSeries[bar] && _signal && Candles.OpenSeries[bar] < Candles.HighSeries[bar - 1]
                && Candles.OpenSeries[bar] > Candles.LowSeries[bar - 1] && LongPos.Count == 0 && ShortPos.Count == 0)
            {
                size = (Candles.CloseSeries[bar] - Candles.OpenSeries[bar]) / FinInfo.Security.MinStep;

                if (size >= 15 && Candles.OpenSeries[bar - 1] < Candles.CloseSeries[bar - 1])
                {
                    PlotArea("SignalEntryShort", bar, new UserChartPropModel { ColorLine = Color.Orchid });
                    _shortEntry = Candles.CloseSeries[bar - 1];
                    _shortTarget = Candles.LowSeries[bar - 1];
                    _shortStopLoss = Candles.CloseSeries[bar - 1] + Stop.Value * FinInfo.Security.MinStep;
                    _time = Candles.DateTimeCandle[bar];
                }
                else
                {
                    _longEntry = 0;
                    _longStopLoss = 0;
                    _longTarget = 0;
                    _signal = false;
                }
            }
            else
            {
                _signal = false;
            }
                
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _longEntry > 0 && Candles.DateTimeCandle[bar + 1] == _time)
            {
                _long = BuyGreater(bar + 1, _longEntry, 1, "Long");

                if (_long != null && _long.ExitOrderStatus == EnumOrderStatus.Active)
                {
                    _longEntry = 0;
                    _signal = false;
                }
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _shortEntry > 0 && Candles.DateTimeCandle[bar + 1] == _time)
            {
                _short = ShortLess(bar + 1, _shortEntry, 1, "Short");

                if (_short != null && _short.ExitOrderStatus == EnumOrderStatus.Active)
                {
                    _shortEntry = 0;
                    _signal = false;
                }
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                SellAtProfit(bar + 1, _long, _longTarget, "Long take");
                SellAtStop(bar + 1, _long, _longStopLoss, "Long stop");
            }
            if (ShortPos.Count > 0)
            {
                CoverAtProfit(bar + 1, _short, _shortTarget, "Short take");
                CoverAtStop(bar + 1, _short, _shortStopLoss, "Short stop");
            }

            if (_long != null && _long.ExitOrderStatus == EnumOrderStatus.Performed)
            {
                _longEntry = 0;
                _longStopLoss = 0;
                _longTarget = 0;
            }
            if (_short != null && _short.ExitOrderStatus == EnumOrderStatus.Performed)
            {
                _shortEntry = 0;
                _shortStopLoss = 0;
                _shortTarget = 0;
            }
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 22);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 22);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/z8d0gunmx1-robot-lindarashke";
            DesParamStratetgy.NameStrategy = "LindaRashke";


        }
    }
}
