using ScriptSolution;
using ScriptSolution.Indicators;
using SourceEts;
using System;

namespace ModulSolution.Robots
{
    /// <summary>
    /// Стратегия по фракталу. Фрактальный канал.
    /// </summary>
    public class CandleRobot2 : Script
    {
        int _index = 0;
        public override void Execute()
        {
            
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 5)
                    continue;

                if (LongPos.Count == 0 && ShortPos.Count == 0 && bar!=_index)
                {
                    if (Candles.CloseSeries[bar] > Candles.OpenSeries[bar] &&
                        Candles.CloseSeries[bar - 1] > Candles.OpenSeries[bar - 1] &&
                        Candles.CloseSeries[bar - 2] > Candles.OpenSeries[bar - 2])
                    {
                        BuyAtMarket(bar + 1, 1, "");
                    }
                    if (Candles.CloseSeries[bar] < Candles.OpenSeries[bar] &&
                        Candles.CloseSeries[bar - 1] < Candles.OpenSeries[bar - 1] &&
                        Candles.CloseSeries[bar - 2] < Candles.OpenSeries[bar - 2])
                    {
                        ShortAtMarket(bar + 1, 1, "");
                    }
                }
                else
                {
                    if (LongPos.Count != 0 || ShortPos.Count != 0)
                    {
                        SendStandartStopFromForm(bar + 1, "");
                        SendTimePosCloseFromForm(bar + 1, "");
                        SendClosePosOnRiskFromForm(bar + 1, "");
                    }
                }
                _index = bar;

            }

        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 01, 17);
            DesParamStratetgy.DateChange = new DateTime(2020, 01, 17);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Candle;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/dcethi9bg1-robot-candlerobot2-tri-svechi-podryad";
            DesParamStratetgy.NameStrategy = "CandleRobot2";


        }

    }
}
