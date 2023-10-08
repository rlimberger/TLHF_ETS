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
using SourceEts.Config;
using SourceEts.Models;



namespace ETSBots
{
    public class RenkoRC : Script
    {
        public class RenkoBar
        {
            public double open;
            public double close;
            public DateTime candleTime;
            public RenkoBar(double open, double close, DateTime candleTime)
            {
                this.open = open;
                this.close = close;
                this.candleTime = candleTime;
            }
        }

        List<RenkoBar> renkos = new List<RenkoBar>();

        bool _isFormed = true;

        double _start;

        DateTime time = DateTime.MinValue;

        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                CalcRenko(bar, 4);


                Drawing();
            }
        }

        private void CalcRenko(int bar, int size)
        {
            if (Candles.CloseSeries.Count == 0)
                return;

            if (_start == 0 && time != Candles.DateTimeCandle[bar])
            {
                _start = Candles.CloseSeries[bar];
                time = Candles.DateTimeCandle[bar];
                _isFormed = false;
            }
            else if (!_isFormed && _start > 0 && Candles.CloseSeries[bar] == _start + size * FinInfo.Security.MinStep && time != Candles.DateTimeCandle[bar])
            {
                renkos.Add(new RenkoBar(_start, Candles.CloseSeries[bar], Candles.DateTimeCandle[bar]));
                
                time = Candles.DateTimeCandle[bar];
                _isFormed = true;
                _start = 0;
            }
            else if (!_isFormed && _start > 0 && Candles.CloseSeries[bar] == _start - size * FinInfo.Security.MinStep && time != Candles.DateTimeCandle[bar])
            {
                renkos.Add(new RenkoBar(_start, Candles.CloseSeries[bar], Candles.DateTimeCandle[bar]));

                time = Candles.DateTimeCandle[bar];
                _isFormed = true;
                _start = 0;
            }
            else if (!_isFormed && _start > 0 && Candles.CloseSeries[bar] > _start + size * FinInfo.Security.MinStep && time != Candles.DateTimeCandle[bar])
            {
                double value = Math.Truncate((Candles.CloseSeries[bar] - _start) / (size * FinInfo.Security.MinStep));

                double start = _start;
                for (int i = 0; i <= 4; i++)
                {
                    renkos.Add(new RenkoBar(start, start + size * FinInfo.Security.MinStep, Candles.DateTimeCandle[bar]));
                    start += size * FinInfo.Security.MinStep;
                }
                _start = start;
                time = Candles.DateTimeCandle[bar];
            }
            else if (!_isFormed && _start > 0 && Candles.CloseSeries[bar] < _start + size * FinInfo.Security.MinStep && time != Candles.DateTimeCandle[bar])
            {
                double value = Math.Truncate((_start - Candles.CloseSeries[bar]) / (size * FinInfo.Security.MinStep));

                double start = _start;
                for (int i = 0; i <= 4; i++)
                {
                    renkos.Add(new RenkoBar(start, start - size * FinInfo.Security.MinStep, Candles.DateTimeCandle[bar]));
                    start -= size * FinInfo.Security.MinStep;
                }
                _start = start;
                time = Candles.DateTimeCandle[bar];
            }
        } 

        private List<RangeBarCol> ConvertData()
        {
            var list = new List<RangeBarCol>();

            if (renkos.Count == 0)
                return list;

            int count = 0;

            foreach (var item in renkos)
            {
                if (item.open < item.close)
                    list.Add(new RangeBarCol() { NumberBar = count++, ColorRangeBar = Color.Green, PricePoint1 = item.open, PricePoint2 = item.close });
                if (item.open > item.close)
                    list.Add(new RangeBarCol() { NumberBar = count++, ColorRangeBar = Color.Red, PricePoint1 = item.open, PricePoint2 = item.close });
            }
            return list;
        }

        private void ConvertToRenko(int bar)
        {



        }

        private void Drawing()
        {
            PlotRangeBar("SecondInstr", ConvertData(), new UserChartPropModel()
            {
                NumberPanel = 1,
                Transparency = 100
            });
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 29);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 29);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/s3e81oour1-robot-renko";
            DesParamStratetgy.NameStrategy = "Renko";
        }
    }
}
