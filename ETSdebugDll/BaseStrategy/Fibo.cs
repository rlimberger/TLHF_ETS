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
    public class Fibo : Script
    {
        public class FiboLevels
        {
            public double _level_0236;
            public double _level_0382;
            public double _level_0500;
            public double _level_0618;
            public double _level_0764;
            public int _swingSize;
            public string _direction;

            public FiboLevels(double level_0236, double level_0382, double level_0500, double level_0618, double level_0764, int swingSize, string direction)
            {
                _level_0236 = level_0236;
                _level_0382 = level_0382;
                _level_0500 = level_0500;
                _level_0618 = level_0618;
                _level_0764 = level_0764;
                _swingSize = swingSize;
                _direction = direction;
            }
        }


        public ParamOptimization SwingSize = new ParamOptimization(130, 1, 50000, 1, "SwingSize", "");
        public ParamOptimization Diff = new ParamOptimization(2, 1, 50000, 1, "Diff", "");
        public ParamOptimization EntrLevel = new ParamOptimization(1, 1, 4, 1, "EntrLevel", "");


        public CreateIndicator Channel = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");



        IPosition _long_long_long;
        IPosition _short;

        List<double> _channelUp;
        List<double> _channelDown;
        List<double> _parabolic;


        double _max;
        double _min;
        double _diff;


        DateTime _maxDate = DateTime.MinValue;
        DateTime _minDate = DateTime.MinValue;


        FiboLevels _levels;


       

        public override void Execute()
        {
            _channelUp = Channel.param.LinesIndicators[0].PriceSeries;
            _channelDown = Channel.param.LinesIndicators[1].PriceSeries;
            _parabolic = Sar.param.LinesIndicators[0].PriceSeries;

            

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (LongPos.Count == 0 && ShortPos.Count == 0)
                {
                    DetectExtremums(bar);
                    _levels = CalcFiboLevels();
                }
                
                CalcDiff(bar);

                Entry(bar);
                Exit(bar);


                ParamDebug("Max", _max);
                ParamDebug("Min", _min);
                ParamDebug("SwingSize", _levels._swingSize);
                ParamDebug("Diff", _diff);



                Draw();

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

        private void DetectExtremums(int bar)
        {
            for (int i = bar; i > 0; i--)
            {
                if (Candles.HighSeries[i] >= _channelUp[i])
                {
                    _max = Candles.HighSeries[i];
                    _maxDate = Candles.DateTimeCandle[i];
                    //PlotArea("MaxValue", i, new UserChartPropModel() { ColorLine = Color.Green });
                    break;
                }
            }

            for (int i = bar; i > 0; i--)
            {
                if (Candles.LowSeries[i] <= _channelDown[i])
                {
                    _min = Candles.LowSeries[i];
                    _minDate = Candles.DateTimeCandle[i];
                    //PlotArea("MinValue", i, new UserChartPropModel() { ColorLine = Color.Red });
                    break;
                }
            }
        }

        private FiboLevels CalcFiboLevels()
        {
            if (_max == 0 || _min == 0 || _maxDate == DateTime.MinValue || _minDate == DateTime.MinValue)
                return new FiboLevels(0, 0, 0, 0, 0, 0, ""); 

            if (_max > _min && _maxDate > _minDate)
            {
                double size = _max - _min;

                double a = _max - (size * 23.6 / 100);
                double b = _max - (size * 38.2 / 100);
                double c = _max - (size * 50 / 100);
                double d = _max - (size * 61.8 / 100);
                double e = _max - (size * 76.4 / 100);

                return new FiboLevels(a, b, c, d, e, (int)(size / FinInfo.Security.MinStep), "up");
            }
            else if (_max > _min && _maxDate < _minDate)
            {
                double size = _max - _min;

                double a = _min + (size * 23.6 / 100);
                double b = _min + (size * 38.2 / 100);
                double c = _min + (size * 50 / 100);
                double d = _min + (size * 61.8 / 100);
                double e = _min + (size * 76.4 / 100);

                return new FiboLevels(a, b, c, d, e, (int)(size / FinInfo.Security.MinStep), "down");
            }
            else
            {
                return new FiboLevels(0, 0, 0, 0, 0, 0, "");
            }
        }

        private void CalcDiff(int bar)
        {
            if (Candles.LowSeries[bar] >= _parabolic[bar])
                _diff = Candles.LowSeries[bar] - _parabolic[bar];
            else if (Candles.HighSeries[bar] < _parabolic[bar])
                _diff = _parabolic[bar] - Candles.HighSeries[bar];
            else
                _diff = 0;

            if (Candles.LowSeries[bar] > _parabolic[bar] && _diff <= Diff.Value)
                PlotArea("StopLong", bar, new UserChartPropModel() { ColorLine = Color.Green });
            else if (Candles.HighSeries[bar] < _parabolic[bar] && _diff <= Diff.Value)
                PlotArea("StopShort", bar, new UserChartPropModel() { ColorLine = Color.Red });
        }

        private void Draw()
        {
            if (_levels != null && _levels._level_0236 > 0 && _levels._direction == "up")
            {
                PlotLine("23.6 UP", _levels._level_0236, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("38.2 UP", _levels._level_0382, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("50.0 UP", _levels._level_0500, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("61.8 UP", _levels._level_0618, new UserChartPropModel() { ColorLine = Color.GreenYellow });

                PlotLine("23.6 DOWN", 0, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("38.2 DOWN", 0, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("50.0 DOWN", 0, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("61.8 DOWN", 0, new UserChartPropModel() { ColorLine = Color.OrangeRed });
            }
            else if (_levels != null && _levels._level_0236 > 0 && _levels._direction == "down")
            {
                PlotLine("23.6 DOWN", _levels._level_0236, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("38.2 DOWN", _levels._level_0382, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("50.0 DOWN", _levels._level_0500, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("61.8 DOWN", _levels._level_0618, new UserChartPropModel() { ColorLine = Color.OrangeRed });

                PlotLine("23.6 UP", 0, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("38.2 UP", 0, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("50.0 UP", 0, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("61.8 UP", 0, new UserChartPropModel() { ColorLine = Color.GreenYellow });
            }
            else
            {
                PlotLine("23.6 UP", 0, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("38.2 UP", 0, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("50.0 UP", 0, new UserChartPropModel() { ColorLine = Color.GreenYellow });
                PlotLine("61.8 UP", 0, new UserChartPropModel() { ColorLine = Color.GreenYellow });

                PlotLine("23.6 DOWN", 0, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("38.2 DOWN", 0, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("50.0 DOWN", 0, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                PlotLine("61.8 DOWN", 0, new UserChartPropModel() { ColorLine = Color.OrangeRed });
            }
        }

        private void Entry(int bar)
        {
            if (_levels == null || _levels._level_0236 == 0)
                return;

            if (LongPos.Count == 0 && ShortPos.Count == 0 && _levels._direction == "up" && Candles.LowSeries[bar] > _parabolic[bar] && _diff > Diff.Value)
            {
                switch (EntrLevel.ValueInt)
                {
                    case 1:
                        _long = BuyAtLimit(bar + 1, _levels._level_0236, 1, "Long");
                        break;
                    case 2:
                        _long = BuyAtLimit(bar + 1, _levels._level_0382, 1, "Long");
                        break;
                    case 3:
                        _long = BuyAtLimit(bar + 1, _levels._level_0500, 1, "Long");
                        break;
                    case 4:
                        _long = BuyAtLimit(bar + 1, _levels._level_0618, 1, "Long");
                        break;
                }
            }

            if (LongPos.Count == 0 && ShortPos.Count == 0 && _levels._direction == "down" && Candles.HighSeries[bar] < _parabolic[bar] && _diff > Diff.Value)
            {
                switch (EntrLevel.ValueInt)
                {
                    case 1:
                        _short = ShortAtLimit(bar + 1, _levels._level_0236, 1, "Short");
                        break;
                    case 2:
                        _short = ShortAtLimit(bar + 1, _levels._level_0382, 1, "Short");
                        break;
                    case 3:
                        _short = ShortAtLimit(bar + 1, _levels._level_0500, 1, "Short");
                        break;
                    case 4:
                        _short = ShortAtLimit(bar + 1, _levels._level_0618, 1, "Short");
                        break;
                }
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                SellAtProfit(bar + 1, _long, _max,"long take " + _long.EntryPrice.ToString());
                SellAtStop(bar + 1, _long, _levels._level_0764, "long stop " + _long.EntryPrice.ToString());
            }
            if (ShortPos.Count > 0)
            {
                CoverAtProfit(bar + 1, _short, _min, "short take " + _short.EntryPrice.ToString());
                CoverAtStop(bar + 1, _short, _levels._level_0764, "short stop " + _short.EntryPrice.ToString());
            }
        }

        public override void SetSettingDefault()
        {
            Channel.param.LinesIndicators[0].LineParam[0].Value = 20;
            Channel.param.LinesIndicators[1].LineParam[0].Value = 20;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 27);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 27);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/ao32olc501-robot-fibo";
            DesParamStratetgy.NameStrategy = "Fibo";
        }
    }
}
