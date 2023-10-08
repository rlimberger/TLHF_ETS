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

             /// <summary>
    /// Стратегия скользящие средние
    /// </summary>

    public class MA2 : Script
    {
        public CreateIndicator FastMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Быстрая. Открытие позиции");
        public CreateIndicator SlowMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Медленная. Открытие позиции");

        public CreateIndicator FastMaClose = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Быстрая. Закрытие позиции");
        public CreateIndicator SlowMaClose = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Медленная. Закрытие позиции");

        private bool _closeLong = false;
        private bool _closeShort = false;
        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                #region Открытие позиции и переворт позиции


                if (ShortPos.Count != 0)
                    if (CrossOver(FastMaClose.param.LinesIndicators[0].PriceSeries,
                         SlowMaClose.param.LinesIndicators[0].PriceSeries, bar))
                    {
                        CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                        _closeShort = true;
                    }

                if (LongPos.Count != 0)
                    if (CrossUnder(FastMaClose.param.LinesIndicators[0].PriceSeries,
                        SlowMaClose.param.LinesIndicators[0].PriceSeries, bar))
                    {
                        SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
                        _closeLong = true;
                    }

                if (ShortPos.Count == 0 && LongPos.Count == 0 ||
                    ShortPos.Count == 0 && LongPos.Count != 0 && _closeLong)
                    if (CrossUnder(FastMa.param.LinesIndicators[0].PriceSeries,
                    SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                    {
                        _closeLong = false;

                        ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                    }

                if (ShortPos.Count == 0 && LongPos.Count == 0 ||
                    ShortPos.Count != 0 && LongPos.Count == 0 && _closeShort)
                    if (CrossOver(FastMa.param.LinesIndicators[0].PriceSeries,
                        SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                    {
                        BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                        _closeShort = false;
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
            FastMa.param.LinesIndicators[0].LineParam[0].Value = 28;
            SlowMa.param.LinesIndicators[0].ArgbLine = Color.Red.ToArgb();
            SlowMa.param.LinesIndicators[0].LineParam[0].Value = 56;
            FastMaClose.param.LinesIndicators[0].ArgbLine = Color.Blue.ToArgb();
            FastMaClose.param.LinesIndicators[0].LineParam[0].Value = 7;
            SlowMaClose.param.LinesIndicators[0].ArgbLine = Color.Orange.ToArgb();
            SlowMaClose.param.LinesIndicators[0].LineParam[0].Value = 14;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2016, 03, 14);
            DesParamStratetgy.DateChange = new DateTime(2016, 03, 14);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "";
            DesParamStratetgy.NameStrategy = "MA2";

        }
    }

    
}
