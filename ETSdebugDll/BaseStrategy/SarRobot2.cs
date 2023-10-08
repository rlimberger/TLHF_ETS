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

    public class SarRobot2Atr : Script
    {
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");


        private double priceLong = 0;
        private double priceShort = 0;

        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 1)
                    continue;

                if (Sar.param.LinesIndicators[0].PriceSeries[bar] > Candles.OpenSeries[bar] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar + 1] < Candles.OpenSeries[bar + 1])
                {
                    priceLong = Sar.param.LinesIndicators[0].PriceSeries[bar];
                    priceShort = 0;
                }

                if (priceLong > 0)
                {
                    if (ShortPos.Count > 0)
                        CoverAtStop(bar + 1, ShortPos[0], priceLong, "SarClose");
                    BuyGreater(bar + 1, priceLong, 1, "SarOpen");

                    if (MarketPosition > 0)
                        priceLong = 0;
                }


                if (Sar.param.LinesIndicators[0].PriceSeries[bar] < Candles.OpenSeries[bar] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar + 1] > Candles.OpenSeries[bar + 1])
                {
                    priceShort = Sar.param.LinesIndicators[0].PriceSeries[bar];
                    priceLong = 0;
                }

                if (priceShort > 0)
                {
                    if (LongPos.Count > 0)
                        SellAtStop(bar + 1, LongPos[0], priceShort, "SarClose");

                    ShortLess(bar + 1, priceShort, 1, "SarOpen");
                    if (MarketPosition < 0)
                        priceShort = 0;
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
            DesParamStratetgy.DateRelease = new DateTime(2016, 09, 02);
            DesParamStratetgy.DateChange = new DateTime(2020, 05, 27);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec365827418";
            DesParamStratetgy.NameStrategy = "SarRobot2";

        }

    }
}
