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

    public class MAandMacd : Script
    {
        public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");
        public CreateIndicator FastMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Быстрая");
        public CreateIndicator SlowMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Медленная");

        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                #region Открытие позиции и переворт позиции

                if (CrossOver(FastMa.param.LinesIndicators[0].PriceSeries,
            SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                {
                    CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                    BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                }
                if (CrossUnder(FastMa.param.LinesIndicators[0].PriceSeries,
                    SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                {
                    SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
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
                    ParamDebug("Fast MA тек.", Math.Round(FastMa.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                    ParamDebug("Fast MA пред.", Math.Round(FastMa.param.LinesIndicators[0].PriceSeries[bar], 4));

                }

                if (bar > 2 && SlowMa.param.LinesIndicators[0].PriceSeries.Count > 2)
                {
                    ParamDebug("slow MA тек.", Math.Round(SlowMa.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                    ParamDebug("slow MA пред.", Math.Round(SlowMa.param.LinesIndicators[0].PriceSeries[bar], 4));

                } 
                #endregion
            }

        }


        public override void SetSettingDefault()
        {

            FastMa.param.LinesIndicators[0].ArgbLine = Color.Green.ToArgb();
            FastMa.param.LinesIndicators[0].LineParam[0].Value = 50;
            SlowMa.param.LinesIndicators[0].ArgbLine= Color.Red.ToArgb();
            SlowMa.param.LinesIndicators[0].LineParam[0].Value = 100;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 07, 24);
            DesParamStratetgy.DateChange = new DateTime(2020, 07, 24);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/99dmara8g1-robot-maandmacd";
            DesParamStratetgy.NameStrategy = "MA и MACD";
        }
    }

    
}
