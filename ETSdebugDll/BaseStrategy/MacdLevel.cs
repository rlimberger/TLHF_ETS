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
    /// <summary>
    /// Стратегия по индикатору макд
    /// </summary>

    public class MacdLevel : Script
    {
        public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");
        public ParamOptimization Otstup = new ParamOptimization(0, 1, 10, 5, "Отступ", "Превышение МАКД относительно нулевой линии на заданную величину");


        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 5)
                    continue;

                if (Macd.param.LinesIndicators[0].PriceSeries[bar - 1] < Math.Abs(Otstup.Value) &&
                    Math.Abs(Otstup.Value) <= Macd.param.LinesIndicators[0].PriceSeries[bar])
                {
                    CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                    BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                }

                if (Macd.param.LinesIndicators[0].PriceSeries[bar - 1] > -Math.Abs(Otstup.Value) &&
                    -Math.Abs(Otstup.Value) >= Macd.param.LinesIndicators[0].PriceSeries[bar])
                {
                    SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
                    ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                }

                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                }
                SendClosePosOnRiskFromForm(bar + 1, "");

            }
        }



        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 29);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 29);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357322661";
            DesParamStratetgy.NameStrategy = "MacdHistLevel";

        }

    }
}
