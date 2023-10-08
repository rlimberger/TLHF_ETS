﻿using ScriptSolution;
using ScriptSolution.Indicators;
using SourceEts;
using System;

namespace ModulSolution.Robots
{
    /// <summary>
    /// Стратегия по фракталу. Фрактальный канал.
    /// </summary>
    public class FractalChannelOneOpen : Script
    {
        public CreateIndicator Fractals = new CreateIndicator(EnumIndicators.Fractals, 0, "Indi");
        private string lastFractal = "";
        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (Fractals.param.LinesIndicators[0].PriceSeries[bar + 1] > Fractals.param.LinesIndicators[1].PriceSeries[bar + 1])
                {
                    if (Fractals.param.LinesIndicators[0].PriceSeries[bar + 1] > 0)
                    {
                        if (ShortPos.Count >= 0 && LongPos.Count == 0 && lastFractal != "Long")
                        {
                            CoverAtStop(bar + 1, GetLastPosition(), Fractals.param.LinesIndicators[0].PriceSeries[bar + 1], "Реверс. Закрытие шорт");
                            BuyGreater(bar + 1, Fractals.param.LinesIndicators[0].PriceSeries[bar + 1], 1, "Открытие лонг");
                            if (LongPos.Count > 0 || Fractals.param.LinesIndicators[0].PriceSeries[bar + 1] <= Candles.HighSeries[bar + 1])
                                lastFractal = "Long";
                        }
                    }

                    if (Fractals.param.LinesIndicators[1].PriceSeries[bar + 1] > 0)
                    {
                        if (LongPos.Count >= 0 && ShortPos.Count == 0 && lastFractal != "Short")
                        {
                            SellAtStop(bar + 1, GetLastPosition(), Fractals.param.LinesIndicators[1].PriceSeries[bar + 1], "Реверс. Закрытие лонг");
                            ShortLess(bar + 1, Fractals.param.LinesIndicators[1].PriceSeries[bar + 1], 1, "Открытие шорт");
                            if (ShortPos.Count > 0 || Fractals.param.LinesIndicators[1].PriceSeries[bar + 1] >= Candles.LowSeries[bar + 1])
                                lastFractal = "Short";
                        }
                    }
                }



                ParamDebug("Посл. сделка", lastFractal);

                if (LongPos.Count > 0 || ShortPos.Count > 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                }
                SendClosePosOnRiskFromForm(bar + 1, "");
            }

        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 05, 13);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 07);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/dhzk489x41-robot-fractalchanneloneopen-fraktal-odin";
            DesParamStratetgy.NameStrategy = "FractalChannelOneOpen";


        }

    }
}
