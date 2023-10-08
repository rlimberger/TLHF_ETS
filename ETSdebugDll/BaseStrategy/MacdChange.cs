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
    public class MacdChange : Script
    {
       public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");

        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 5)
                    continue;
                if (ShortPos.Count != 0)
                    if (Macd.param.LinesIndicators[2].PriceSeries[bar - 1] <
                        Macd.param.LinesIndicators[2].PriceSeries[bar])
                        CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");

                if (Macd.param.LinesIndicators[2].PriceSeries[bar - 1] < 0
                    && Macd.param.LinesIndicators[2].PriceSeries[bar] >= 0)
                {
                    BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                }

                if (LongPos.Count != 0)
                    if (Macd.param.LinesIndicators[2].PriceSeries[bar - 1] >
                        Macd.param.LinesIndicators[2].PriceSeries[bar])
                        SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");

                if (Macd.param.LinesIndicators[2].PriceSeries[bar - 1] > 0
                    && Macd.param.LinesIndicators[2].PriceSeries[bar] <= 0)
                {
                    ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
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
            DesParamStratetgy.DateRelease = new DateTime(2017, 03, 02);
            DesParamStratetgy.DateChange = new DateTime(2017, 03, 02);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357281972";
            DesParamStratetgy.NameStrategy = "MacdChange";
        }

    }
}
