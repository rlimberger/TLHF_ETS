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

    public class MA4 : Script
    {
        public CreateIndicator FastMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Быстрая");
        public CreateIndicator SlowMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Медленная");
        public ParamOptimization Deep = new ParamOptimization(20, 1, 10, 1, "Глубина, свечи", "Величина учитывающая как давно свечи стали закрываться под медленной скользящей средней. " +
            "Если закрытия прошло в рамках укаанного значения, то робот откроет позицию");

        /// <summary>
        /// Бар когда цена закрылась ниже медленной МА
        /// </summary>
        int _barCrossDown = 0;
        /// <summary>
        /// Бар когда цена закрылась выше медленной МА
        /// </summary>
        int _barCrossUp = 0;
        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 5)
                    continue;

                #region Открытие позиции и переворт позиции
                if (Candles.CloseSeries[bar - 1] >= SlowMa.param.LinesIndicators[0].PriceSeries[bar - 1] &&
                    Candles.CloseSeries[bar] < SlowMa.param.LinesIndicators[0].PriceSeries[bar])
                    _barCrossDown = bar;
                if (Candles.CloseSeries[bar - 1] <= SlowMa.param.LinesIndicators[0].PriceSeries[bar - 1] &&
                    Candles.CloseSeries[bar] > SlowMa.param.LinesIndicators[0].PriceSeries[bar])
                    _barCrossUp = bar;

                if (CrossOver(FastMa.param.LinesIndicators[0].PriceSeries,
                    SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                {
                    CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                    if (_barCrossUp + Deep.ValueInt >= bar + 1)
                        BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                }
                if (CrossUnder(FastMa.param.LinesIndicators[0].PriceSeries,
                    SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                {
                    SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
                    if (_barCrossDown + Deep.ValueInt >= bar + 1)
                        ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                }

                #endregion


                #region Закрытие позиции по времени, встроенным стопам и рискам
                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                    SendClosePosOnRiskFromForm(bar + 1, "");
                }
                #endregion

                #region Вывод информации
                if (bar > 2)
                {
                    ParamDebug("Бар закрытие ниже SlowMa", _barCrossDown);
                    ParamDebug("Бар закрытие выше SlowMa", _barCrossUp);
                }

                #endregion
            }
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 02, 08);
            DesParamStratetgy.DateChange = new DateTime(2020, 02, 08);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec356971103";
            DesParamStratetgy.NameStrategy = "MA4";

        }
    }

    
}
