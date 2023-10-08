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

    public class MacdRobot : Script
    {
        public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");
        public ParamOptimization Level = new ParamOptimization(0, 1, 10, 5, "Мин. уровень пересечения", "Уровень выше/ниже которого должно произойти пересечение линий индикатора");


        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 5)
                    continue;

                if (Math.Abs(Level.Value) <= Math.Abs(Macd.param.LinesIndicators[0].PriceSeries[bar]))
                    if (CrossOver(Macd.param.LinesIndicators[0].PriceSeries, Macd.param.LinesIndicators[1].PriceSeries, bar))
                    {
                        CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                        BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                    }

                if (Math.Abs(Level.Value) <= Math.Abs(Macd.param.LinesIndicators[0].PriceSeries[bar]))
                    if (CrossUnder(Macd.param.LinesIndicators[0].PriceSeries, Macd.param.LinesIndicators[1].PriceSeries, bar))
                    {
                        SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
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
            DesParamStratetgy.Version = "3";
            DesParamStratetgy.DateRelease = new DateTime(2015, 11, 02);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 13);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/50jtxihbh1-macdrobot-robot-po-indikatoru-macd";
            DesParamStratetgy.NameStrategy = "MacdRobot";

        }

    }
}
