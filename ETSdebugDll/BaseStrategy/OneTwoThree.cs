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
    public class OneTwoThree : Script
    {
        public class Values
        {
            public DateTime time;
            public bool isMax;

            public Values(DateTime time, bool isMax)
            {
                this.time = time;
                this.isMax = isMax;
            }
        }

        public CreateIndicator Channel = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");

        public ParamOptimization Multiplicity = new ParamOptimization(2, 1, 500, 1, "Multiplicity", "Кратность размера тейка относительно стопа");


        Dictionary<double, bool> keyValues = new Dictionary<double, bool>(3);

        List<double> _channelUp;
        List<double> _channelDown;


        IPosition _long;
        IPosition _short;

        double _longProfitTarget;
        double _shortProfitTarget;


        double _oldMax;
        double _oldMin;
        double _newMax;
        double _newMin;

        double _entryLongPoint;
        double _entryShortPoint;
        double _stopLongPoint;
        double _stopShortPoint;


        DateTime _oldTimeMax;
        DateTime _oldTimeMin;
        DateTime _newTimeMax;
        DateTime _newTimeMin;

        bool _signalBuy;
        bool _signalSell;

        

        public override void Execute()
        {
            if (CandleCount <= 2)
                return;

            _channelUp = Channel.param.LinesIndicators[0].PriceSeries;
            _channelDown = Channel.param.LinesIndicators[1].PriceSeries;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (_channelDown.Count == 0 || _channelUp.Count == 0)
                    return;

                
                FilterSignals(bar);

                Entry(bar);
                Exit(bar);
                

                if (keyValues.Keys.ToList().Count == 3)
                {
                    ParamDebug("_signalBuy", _signalBuy);
                    ParamDebug("_signalSell", _signalSell);
                    ParamDebug("value1", keyValues.Keys.ToList()[0]);
                    ParamDebug("value2", keyValues.Keys.ToList()[1]);
                    ParamDebug("value3", keyValues.Keys.ToList()[2]);
                }
                    


                

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

        private Dictionary<double, Values> DefiningVertices(int bar)
        {
            Dictionary<double, Values> keyValues = new Dictionary<double, Values>();
            int count = 0;

            for (int i = bar; i > 0; i--)
            {
                if (!keyValues.ContainsKey(Candles.LowSeries[i]) && _channelDown[i] >= Candles.LowSeries[i])
                {
                    keyValues.Add(Candles.LowSeries[i], new Values(Candles.DateTimeCandle[i], false));
                    count++;
                }
                if (count == 2)
                    break;
            }
            count = 0;

            for (int i = bar; i > 0; i--)
            {
                if (!keyValues.ContainsKey(Candles.HighSeries[i]) && _channelUp[i] <= Candles.HighSeries[i])
                {
                    keyValues.Add(Candles.HighSeries[i], new Values(Candles.DateTimeCandle[i], true));
                    count++;
                }
                if (count == 2)
                    break;
            }

            return keyValues;
        }

        private void Variables(int bar)
        {
            var sorted = DefiningVertices(bar).OrderByDescending(x => x.Value.time).ToList();
            if (sorted.Count < 4)
                return;

            keyValues.Clear();

            if (sorted[0].Value.isMax)
            {
                _newMax = sorted[0].Key;
                _newTimeMax = sorted[0].Value.time;
                keyValues.Add(_newMax, true);
            }
            else
            {
                _newMin = sorted[0].Key;
                _newTimeMin = sorted[0].Value.time;
                keyValues.Add(_newMin, false);
            }
            if (sorted[1].Value.isMax)
            {
                _newMax = sorted[1].Key;
                _newTimeMax = sorted[1].Value.time;
                keyValues.Add(_newMax, true);
            }
            else
            {
                _newMin = sorted[1].Key;
                _newTimeMin = sorted[1].Value.time;
                keyValues.Add(_newMin, false);
            }
            if (sorted[2].Value.isMax)
            {
                _oldMax = sorted[2].Key;
                _oldTimeMax = sorted[2].Value.time;
                keyValues.Add(_oldMax, true);
            }
            else
            {
                _oldMin = sorted[2].Key;
                _oldTimeMin = sorted[2].Value.time;
                keyValues.Add(_oldMin, false);
            }
        }

        private void Signal(int bar)
        {
            if (keyValues.Values.ToList().Count < 3)
                return;

            if (!keyValues.Values.ToList()[0] && keyValues.Values.ToList()[1] && !keyValues.Values.ToList()[2]
                && keyValues.Keys.ToList()[0] > keyValues.Keys.ToList()[2])
            {
                _signalBuy = true;
                _signalSell = false;
                _entryLongPoint = keyValues.Keys.ToList()[1];
                _stopLongPoint = keyValues.Keys.ToList()[2];
            }
            else if (keyValues.Values.ToList()[0] && !keyValues.Values.ToList()[1] && keyValues.Values.ToList()[2] 
                && keyValues.Keys.ToList()[0] < keyValues.Keys.ToList()[2])
            {
                _signalSell = true;
                _signalBuy = false;
                _entryShortPoint = keyValues.Keys.ToList()[1];
                _stopShortPoint = keyValues.Keys.ToList()[2];
            }
            else
            {
                _signalBuy = false;
                _signalSell = false;
            }
        }

        private void FilterSignals(int bar)
        {
            if (_signalBuy && Candles.LowSeries[bar] > _stopLongPoint)
                return;
            else if (_signalSell && Candles.HighSeries[bar] < _stopShortPoint)
                return;
            else
            {
                Variables(bar);
                Signal(bar);
            }
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _signalBuy && _entryLongPoint > 0)
            {
                ParamDebug("long entry", _entryLongPoint);
                _longProfitTarget = _entryLongPoint + (_entryLongPoint - _stopLongPoint) * Multiplicity.Value;
                _long = BuyGreater(bar + 1, _entryLongPoint, 1, "Long");
                if (_long != null && LongPos.Count > 0)
                {
                    _entryLongPoint = 0;
                }
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _signalSell && _entryShortPoint > 0)
            {
                ParamDebug("short entry", _entryShortPoint);
                _shortProfitTarget = _entryShortPoint - (_stopLongPoint - _entryShortPoint) * Multiplicity.Value;
                _short = ShortLess(bar + 1, _entryShortPoint, 1, "Short");
                if (_short != null && ShortPos.Count > 0)
                {
                    _entryShortPoint = 0;
                }
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                SellAtProfit(bar + 1, _long, _longProfitTarget, "long take");
                SellAtStop(bar + 1, _long, _stopLongPoint, "long stoploss");
            }
            if (ShortPos.Count > 0)
            {
                CoverAtProfit(bar + 1, _short, _shortProfitTarget, "short take");
                CoverAtStop(bar + 1, _short, _stopLongPoint, "short stoploss");
            }
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 08);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 08);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/ia781bxo21-robot-onetwothree-1-2-3";
            DesParamStratetgy.NameStrategy = "OneTwoThree";
        }
    }
}
