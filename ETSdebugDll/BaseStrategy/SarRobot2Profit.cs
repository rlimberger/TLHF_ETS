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

    public class SarRobot2Profit : Script
    {
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");
        public ParamOptimization Profit = new ParamOptimization(200, 5, 120, 5, "Прибыль, шаг цены",
"Половина позиции закрывается при достижении заданной прибыли.  Данная величина умножается на шаг цены.");

        private double priceLong = 0;
        private double priceShort = 0;

        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 1)
                    continue;
                var longPos = GetLongPosition(FinInfo);
                var shortPos = GetShortPositions(FinInfo);
                if (Sar.param.LinesIndicators[0].PriceSeries[bar] > Candles.HighSeries[bar] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar + 1] < Candles.HighSeries[bar + 1] ||
                    Sar.param.LinesIndicators[0].PriceSeries[bar] <= Candles.HighSeries[bar] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar] >= Candles.LowSeries[bar] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar + 1] < Candles.LowSeries[bar + 1])
                {
                    priceLong = Sar.param.LinesIndicators[0].PriceSeries[bar];
                    priceShort = 0;
                }

                for (int i = shortPos.Count - 1; i >= 0; i--)
                {
                    var item = shortPos[i];
                    if (item.EntryNameSignal == "SarOpen2")
                        CoverAtProfit(bar + 1, item, item.EntryPrice - Profit.ValueInt * FinInfo.Security.MinStep, "Профит");

                }

                if (priceLong > 0)
                {
                    for (int i = shortPos.Count - 1; i >= 0; i--)
                    {
                        var item = shortPos[i];

                        CoverAtStop(bar + 1, item, priceLong, "SarClose");
                    }

                    BuyGreater(bar + 1, priceLong, 1, "SarOpen1");
                    BuyGreater(bar + 1, priceLong, 1, "SarOpen2");


                    if (shortPos.Count == 0 && longPos.Count > 1)
                        priceLong = 0;
                }


                if (Sar.param.LinesIndicators[0].PriceSeries[bar] < Candles.LowSeries[bar] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar + 1] > Candles.LowSeries[bar + 1] ||
                    Sar.param.LinesIndicators[0].PriceSeries[bar] <= Candles.HighSeries[bar] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar] >= Candles.LowSeries[bar] &&
                    Sar.param.LinesIndicators[0].PriceSeries[bar + 1] > Candles.HighSeries[bar + 1])
                //if (Sar.param.LinesIndicators[0].PriceSeries[bar-1] <= Sar.param.LinesIndicators[0].PriceSeries[bar] &&
                //    Sar.param.LinesIndicators[0].PriceSeries[bar] > Sar.param.LinesIndicators[0].PriceSeries[bar+1])
                {
                    priceShort = Sar.param.LinesIndicators[0].PriceSeries[bar];
                    priceLong = 0;
                }

                for (int i = longPos.Count - 1; i >= 0; i--)
                {
                    var item = longPos[i];
                    if (item.EntryNameSignal == "SarOpen2")
                        SellAtProfit(bar + 1, item, item.EntryPrice + Profit.ValueInt * FinInfo.Security.MinStep, "Профит");

                }

                if (priceShort > 0)
                {
                    for (int i = longPos.Count - 1; i >= 0; i--)
                    {
                        var item = longPos[i];
                        SellAtStop(bar + 1, item, priceShort, "SarClose");
                    }

                    ShortLess(bar + 1, priceShort, 1, "SarOpen1");
                    ShortLess(bar + 1, priceShort, 1, "SarOpen2");

                    if (longPos.Count == 0 && shortPos.Count > 1)
                        priceShort = 0;
                }

                if (longPos.Count != 0 || shortPos.Count!=0)
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
            DesParamStratetgy.DateRelease = new DateTime(2017, 05, 05);
            DesParamStratetgy.DateChange = new DateTime(2018, 02, 07);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec366198873";
            DesParamStratetgy.NameStrategy = "SarRobot2Profit";

        }

    }
}
