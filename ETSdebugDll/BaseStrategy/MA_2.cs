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

    public class MA_2 : Script
    {
        public CreateIndicator FastMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Быстрая");
        public CreateIndicator SlowMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "");
        public ParamOptimization TradeInstr = new ParamOptimization(EnumTypeGetIEnumerable.GetSeccodeList, "Торг. инструмент", "Тогруемый инструмент");

        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                #region Открытие позиции и переворт позиции

                if (CrossOver(FastMa.param.LinesIndicators[0].PriceSeries,
            SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                {
                    OpenPos(bar, true);
                }
                if (CrossUnder(FastMa.param.LinesIndicators[0].PriceSeries,
                    SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                {
                    OpenPos(bar, false);
                }

                #endregion


                #region Закрытие позиции по времени, встроенным стопам и рискам
                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                }
                SendClosePosOnRiskFromForm(bar + 1, "");

                #endregion

            }

        }

        private void OpenPos(int bar, bool isLong)
        {
            if (isLong)
            {
                var pos = GetShortPositions(TradeInstr.FinInfo);
                if (pos.Count > 0)
                    CoverAtMarket(bar + 1, pos[0], "Закрытие короткой позиции");
                BuyAtMarket(TradeInstr.FinInfo, bar + 1, 1, "Открытие длинной позиции");
            }
            else
            {
                var pos = GetLongPosition(TradeInstr.FinInfo);
                if (pos.Count > 0)
                    SellAtMarket(bar + 1, pos[0], "Закрытие длинной позиции");
                ShortAtMarket(TradeInstr.FinInfo, bar + 1, 1, "Открытие короткой позиции");
            }
        }


        public override void SetSettingDefault()
        {

            FastMa.param.LinesIndicators[0].ArgbLine = Color.Green.ToArgb();
            FastMa.param.LinesIndicators[0].LineParam[0].Value = 50;
            SlowMa.param.LinesIndicators[0].ArgbLine = Color.Red.ToArgb();
            SlowMa.param.LinesIndicators[0].LineParam[0].Value = 100;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 09, 13);
            DesParamStratetgy.DateChange = new DateTime(2021, 09, 13);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec356965955";
            DesParamStratetgy.NameStrategy = "MA_2";

        }
    }


}
