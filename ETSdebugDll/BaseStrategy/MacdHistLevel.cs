using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Model;

namespace ModulSolution.Robots
{
    public class MacdHistLevel : Script
    {
        public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");
        public ParamOptimization Change = new ParamOptimization(1, 5, 120, 5, "Уровень", "При превышении заданного уровня гистограммой будет открытие по рынку");

        string _dir = "";
        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                PlotLine("Up", Change.Value, new SourceEts.Models.UserChartPropModel { ColorLine = Color.Black, NumberPanel = 1 });
                PlotLine("Down", -Change.Value, new SourceEts.Models.UserChartPropModel { ColorLine = Color.Black, NumberPanel = 1 });

                if (bar < 5)
                    continue;

                if (Macd.param.LinesIndicators[2].PriceSeries[bar - 1] < Change.Value &&
                    Macd.param.LinesIndicators[2].PriceSeries[bar] >= Change.Value)
                    _dir = "Long";

                if (Macd.param.LinesIndicators[2].PriceSeries[bar - 1] > -Change.Value &&
                    Macd.param.LinesIndicators[2].PriceSeries[bar] <= -Change.Value)
                    _dir = "Short";

                if (ShortPos.Count != 0)
                    if (_dir == "Long")
                        CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");

                if (_dir == "Long")
                {
                    BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                    _dir = "";
                }

                if (LongPos.Count != 0)
                    if (_dir == "Short")
                        SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");


                if (_dir == "Short")
                {
                    ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                    _dir = "";
                }


                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                    SendClosePosOnRiskFromForm(bar + 1, "");
                }
            }
        }


        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 11, 25);
            DesParamStratetgy.DateChange = new DateTime(2020, 11, 25);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357306060";
            DesParamStratetgy.NameStrategy = "MacdHistLevel";

        }

    }
}
