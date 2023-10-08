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
    /// <summary>
    /// Стратегия скользящие средние
    /// </summary>

    public class RsiSarRobot : Script
    {
        public CreateIndicator Rsi = new CreateIndicator(EnumIndicators.Rsi, 1, "");
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");
        public ParamOptimization LevelUp = new ParamOptimization(80, 5, 120, 5, "Верхний уровень RSI",
            "При пробое верхнего уровня сверху вниз, робот откроет короткую позицию");
        public ParamOptimization LevelDown = new ParamOptimization(20, 5, 120, 5, "Нижний уровень RSI",
            "При пробое уровня снизу вверх, робот откроет длинную позицию");



        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 5)
                    continue;

                if (Sar.param.LinesIndicators[0].PriceSeries[bar] < Candles.OpenSeries[bar])
                    if (Rsi.param.LinesIndicators[0].PriceSeries[bar - 1] < LevelDown.ValueInt &&
                        Rsi.param.LinesIndicators[0].PriceSeries[bar] >= LevelDown.ValueInt)
                    {
                        CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                        BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                    }
                if (Sar.param.LinesIndicators[0].PriceSeries[bar] > Candles.OpenSeries[bar])
                    if (Rsi.param.LinesIndicators[0].PriceSeries[bar - 1] > LevelUp.ValueInt &&
                      Rsi.param.LinesIndicators[0].PriceSeries[bar] <= LevelUp.ValueInt)
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
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 04, 13);
            DesParamStratetgy.DateChange = new DateTime(2020, 04, 13);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/33cce19oo1-robot-rsisarrobot-rsi-filtr-sar";
            DesParamStratetgy.NameStrategy = "RsiSarRobot";

        }

    }
}
