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

    public class SarRobot : Script
    {
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");

        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 1)
                    continue;

                if (Sar.param.LinesIndicators[0].PriceSeries[bar - 1] > Candles.OpenSeries[bar - 1] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar] < Candles.OpenSeries[bar])
                {
                    if (ShortPos.Count > 0)
                        CoverAtMarket(bar + 1, ShortPos[0], "SarClose");
                    BuyAtMarket(bar + 1, 1, "SarOpen");
                }


                if (Sar.param.LinesIndicators[0].PriceSeries[bar - 1] < Candles.OpenSeries[bar - 1] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar] > Candles.OpenSeries[bar])
                {
                    if (LongPos.Count > 0)
                        SellAtMarket(bar + 1, LongPos[0], "SarClose");
                    ShortAtMarket(bar + 1, 1, "SarOpen");
                }


                if (MarketPosition != 0)
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
            DesParamStratetgy.DateRelease = new DateTime(2016, 09, 01);
            DesParamStratetgy.DateChange = new DateTime(2016, 09, 01);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/ho5rxi0x01-robot-parabolic-sar";
            DesParamStratetgy.NameStrategy = "SarRobot";

        }

    }
}
