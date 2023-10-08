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

    public class WillamR1Corss : Script
    {
        public CreateIndicator WillamSmall = new CreateIndicator(EnumIndicators.WilliamsPercentsR, 1, "Быстрая");
        public CreateIndicator WillamBig = new CreateIndicator(EnumIndicators.WilliamsPercentsR, 1, "Медленная");

        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                #region Открытие позиции и переворт позиции

                if (CrossOver(WillamSmall.param.LinesIndicators[0].PriceSeries,
            WillamBig.param.LinesIndicators[0].PriceSeries, bar))
                {
                    CoverAtClose(bar, GetLastPosition(), "Закрытие короткой позиции");
                    BuyAtClose(bar, 1, "Открытие длинной позиции");
                }
                if (CrossUnder(WillamSmall.param.LinesIndicators[0].PriceSeries,
                    WillamBig.param.LinesIndicators[0].PriceSeries, bar))
                {
                    SellAtClose(bar, GetLastPosition(), "Закрытие длинной позиции");
                    ShortAtClose(bar, 1, "Открытие короткой позиции");
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


            }

        }


        public override void SetSettingDefault()
        {

            WillamSmall.param.LinesIndicators[0].ArgbLine = Color.Green.ToArgb();
            WillamSmall.param.LinesIndicators[0].LineParam[0].Value = 26;
            WillamBig.param.LinesIndicators[0].ArgbLine = Color.Red.ToArgb();
            WillamBig.param.LinesIndicators[0].LineParam[0].Value = 52;
        }



        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 10, 19);
            DesParamStratetgy.DateChange = new DateTime(2020, 10, 19);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec368397678";
            DesParamStratetgy.NameStrategy = "WillamR cross";

        }
    }

    
}
