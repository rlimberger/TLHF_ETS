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
    /// Стратегия скользящие средние
    /// </summary>

    public class MaAndPriceRevers : Script
    {
        public CreateIndicator FastMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "");

        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                #region Открытие позиции и переворт позиции

                if (CrossOver(Candles.CloseSeries, FastMa.param.LinesIndicators[0].PriceSeries
                    , bar))
                {

                    SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
                    ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                }
                if (CrossUnder(Candles.CloseSeries, FastMa.param.LinesIndicators[0].PriceSeries,
                     bar))
                {
                    CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                    BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                }

                #endregion


                #region Закрытие позиции по времени, встроенным стопам и рискам
                if (MarketPosition != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                    SendClosePosOnRiskFromForm(bar + 1, "");
                }
                #endregion

            }
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 04, 25);
            DesParamStratetgy.DateChange = new DateTime(2021, 04, 25);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357026993";
            DesParamStratetgy.NameStrategy = "MaAndPriceRevers";

        }
    }

    
}
