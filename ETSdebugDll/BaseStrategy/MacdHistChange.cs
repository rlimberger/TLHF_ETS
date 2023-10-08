using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Model;

namespace ModulSolution.Robots
{
    public class MacdHistChange : Script
    {
        public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");
        public ParamOptimization Change = new ParamOptimization(1, 5, 120, 5, "Изм. гистограммы", "");

        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 5)
                    continue;
                double change = Math.Abs(Macd.param.LinesIndicators[2].PriceSeries[bar] -
                        Macd.param.LinesIndicators[2].PriceSeries[bar - 1]);
                if (Macd.param.LinesIndicators[2].PriceSeries[bar] < Macd.param.LinesIndicators[2].PriceSeries[bar - 1])
                    change = -change;

                if (ShortPos.Count != 0)
                    if (change > Math.Abs(Change.Value))
                        CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");

                if (change > Math.Abs(Change.Value))
                {
                    BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                }

                if (LongPos.Count != 0)
                    if (change < -Math.Abs(Change.Value))
                        SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");

                if (change < -Math.Abs(Change.Value))
                {
                    ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                }

                ParamDebug("Изм. гистограммы", change);

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
            DesParamStratetgy.DateRelease = new DateTime(2017, 03, 02);
            DesParamStratetgy.DateChange = new DateTime(2017, 03, 02);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357300653";
            DesParamStratetgy.NameStrategy = "MacdHistChange";

        }

    }
}
