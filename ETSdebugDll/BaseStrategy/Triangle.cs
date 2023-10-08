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
using SourceEts;
using SourceEts.Config;
using SourceEts.Models;



namespace ETSBots
{
    public class Triangle : Script
    {
        public ParamOptimization Sec = new ParamOptimization(EnumTypeGetIEnumerable.GetSeccodeList, "Второй");
        public ParamOptimization Thrd = new ParamOptimization(EnumTypeGetIEnumerable.GetSeccodeList, "Третий");
        public ParamOptimization Period = new ParamOptimization(50, 1, 50000, 1, "Period", "");


        IPosition _long;
        IPosition _short;
        IPosition _long2;
        IPosition _short2;
        IPosition _long3;
        IPosition _short3;


        List<double> _sprd = new List<double>();
        DateTime _time = DateTime.MinValue;
        double _sprdNew;

        string signal = "";


        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                CalcSprd(bar);

                ParamDebug("Spread", _sprdNew);

                Entry(bar + 1);
                Exit(bar);


                Drawing();
            }
        }

        private void CalcSprd(int bar)
        {
            if (Candles.DateTimeCandle[bar] != _time)
            {
                _sprd.Add(Math.Log(Candles.CloseSeries[bar]) - Math.Log(Sec.FinInfo.Candles.CloseSeries[bar]) - Math.Log(Thrd.FinInfo.Candles.CloseSeries[bar]));
                _time = Candles.DateTimeCandle[bar];
            }
            else
            {
                _sprdNew = Math.Log(Candles.CloseSeries[bar]) - Math.Log(Sec.FinInfo.Candles.CloseSeries[bar]) - Math.Log(Thrd.FinInfo.Candles.CloseSeries[bar]);
            }
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _sprdNew >= 0.0005)
            {
                _short = ShortAtMarket(bar, 2, "Short Instr1");
                _long2 = BuyAtMarket(Sec.FinInfo, bar, 1, "Long Instr2");
                _long3 = BuyAtMarket(Thrd.FinInfo, bar, 1, "Long Instr3");
                signal = "sell";
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _sprdNew <= -0.0005)
            {
                _long = BuyAtMarket(bar, 2, "Long Instr1");
                _short2 = ShortAtMarket(Sec.FinInfo, bar, 1, "Short Instr2");
                _short3 = ShortAtMarket(Thrd.FinInfo, bar, 1, "Short Instr3");
                signal = "buy";
            }
        }

        private void Exit(int bar)
        {
            if (ShortPos.Count > 0 && signal == "sell" && _sprdNew <= 0)
            {
                CoverAtClose(bar + 1, _short, "Close Instr1");
                SellAtClose(bar + 1, _long2, "Close Instr2");
                SellAtClose(bar + 1, _long3, "Close Instr3");
                signal = "";
            }
            if (LongPos.Count > 0 && signal == "buy" && _sprdNew >= 0)
            {
                SellAtClose(bar + 1, _long, "Close Instr1");
                CoverAtClose(bar + 1, _short2, "Close Instr2");
                CoverAtClose(bar + 1, _short3, "Close Instr3");
                signal = "";
            }
        }

        private void Drawing()
        {
            PlotCanldes("SecondInstr", Sec.FinInfo, new UserChartPropModel()
            {
                NumberPanel = 1,
                Transparency = 100
            });
            PlotCanldes("ThirdInstr", Thrd.FinInfo, new UserChartPropModel()
            {
                NumberPanel = 2,
                Transparency = 100
            });
            Plotseries("Sprd", _sprd, new UserChartPropModel()
            {
                ColorLine = Color.OrangeRed,
                Width = 1,
                NumberPanel = 3,
                TypeLine =EnumTypeLine.Line,
                Transparency = 100
            });
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 27);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 27);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Arbitrage;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/2nrszfejv1-robot-triangle";
            DesParamStratetgy.NameStrategy = "Triangle";
        }
    }
}
