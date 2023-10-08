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

namespace TestETSApi
{
    public class PercentChange : Script
    {
        public ParamOptimization trigger = new ParamOptimization(1, 1, 1000, 1, "Процент", "Процентное изменение цены");

        double percent;
        DateTime signal = DateTime.MinValue;
        public override void Execute()
        {
            if (IndexBar < 1)
                return;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                percent = Math.Abs(Math.Round((Candles.HighSeries[bar + 1] - Candles.OpenSeries[bar + 1]) / Candles.OpenSeries[bar + 1] * 100, 2));

                ParamDebug("percent", percent);

                if (percent >= trigger.Value && signal != Candles.DateTimeCandle[bar + 1])
                {
                    signal = Candles.DateTimeCandle[bar + 1];
                    string message = FinInfo.Security.ShortName + "\n"
                                     + percent + "\n"
                                     + Candles.OpenSeries[bar + 1] + "\n"
                                     + Candles.HighSeries[bar + 1] + "\n";

                    PlotArea("signal", bar + 1, new UserChartPropModel() { ColorLine = Color.Green });

                    SendAlert(message);
                }
            }
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2015, 07, 27);
            DesParamStratetgy.DateChange = new DateTime(2020, 01, 29);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/imhkvd1mu1-robot-percentchange";
            DesParamStratetgy.NameStrategy = "PercentChange";
        }

    
    }
}
