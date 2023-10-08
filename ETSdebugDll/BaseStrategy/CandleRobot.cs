using ScriptSolution;
using ScriptSolution.Indicators;
using SourceEts;
using System;

namespace ModulSolution.Robots
{
    /// <summary>
    /// 
    /// </summary>
    public class CandleRobot : Script
    {
        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 5)
                    continue;

                if (LongPos.Count == 0 && ShortPos.Count == 0)
                {
                    if (Candles.HighSeries[bar] >= Candles.HighSeries[bar - 1])
                    {
                        BuyAtMarket(bar + 1, 1, "");
                    }

                    if (Candles.LowSeries[bar] <= Candles.LowSeries[bar - 1])
                    {
                        ShortAtMarket(bar + 1, 1, "");
                    }
                }
                else
                {
                    //Close последней сформированной свечи меньше low предпоследней сформированной свечи и
                    //Open последней сформированной свечи выще close последней сформированной свечи
                    if (LongPos.Count != 0)
                        if (Candles.CloseSeries[bar] < Candles.LowSeries[bar - 1] &&
                            Candles.OpenSeries[bar] > Candles.CloseSeries[bar - 1])
                            SellAtMarket(bar + 1, GetLastPosition(), "Закрытие позиции");

                    if (ShortPos.Count != 0)
                        if (Candles.CloseSeries[bar] > Candles.HighSeries[bar - 1] &&
                            Candles.OpenSeries[bar] < Candles.CloseSeries[bar - 1])
                            CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие позиции");

                    if (LongPos.Count != 0 || ShortPos.Count != 0)
                    {
                        SendStandartStopFromForm(bar + 1, "");
                        SendTimePosCloseFromForm(bar + 1, "");
                        SendClosePosOnRiskFromForm(bar + 1, "");
                    }
                }
            }



        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2020, 02, 08);
            DesParamStratetgy.DateChange = new DateTime(2020, 05, 26);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec356216939";
            DesParamStratetgy.NameStrategy = "CandleRobot";
        }

    }
}
