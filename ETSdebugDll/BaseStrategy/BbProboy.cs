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

    public class BbProboy : Script
    {
        public CreateIndicator Bb = new CreateIndicator(EnumIndicators.BollinderBands, 0, "");
        public ParamOptimization Proboy = new ParamOptimization(1, 5, 120, 5, "Величина пробоя",
            "Величина на которую должен пробиться полоса. Данная величина умножается на шаг цены.");

        private string _dir { get; set; }
        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (Bb.param.LinesIndicators[0].LineParam[0].Value < bar)
                {
                    var priceUp = Bb.param.LinesIndicators[1].PriceSeries;
                    var priceDown = Bb.param.LinesIndicators[2].PriceSeries;

                    if (LongPos.Count == 0 && ShortPos.Count == 0)
                    {
                        if (ShortPos.Count == 0 && _dir != "Long")
                            BuyGreater(bar + 1, priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                "Открытие длинной позиции");
                        if (LongPos.Count == 0 && _dir != "Short")
                            ShortLess(bar + 1, priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                "Открытие короткой позиции");
                    }

                    if (ShortPos.Count > 0)
                    {
                        CoverAtStop(bar + 1, ShortPos[0], priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");
                        BuyGreater(bar + 1, priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Переворот. Открытие длинной позиции");
                    }

                    if (LongPos.Count > 0)
                    {
                        SellAtStop(bar + 1, LongPos[0], priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");
                        ShortLess(bar + 1, priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Переворот. Открытие короткой позиции");

                    }

                    if (Candles.LowSeries[bar + 1] <= priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep)
                        _dir = "Short";

                    if (Candles.HighSeries[bar + 1] >= priceUp[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep)
                        _dir = "Long";

                    if (bar > 2)
                    {
                        ParamDebug("Верх. канал тек.",
                            Math.Round(Bb.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                        ParamDebug("Ниж. канал тек.",
                            Math.Round(Bb.param.LinesIndicators[1].PriceSeries[bar + 1], 4));
                    }

                    if (LongPos.Count != 0 || ShortPos.Count != 0)
                    {

                        SendStandartStopFromForm(bar + 1, "");
                        SendTimePosCloseFromForm(bar + 1, "");
                        SendClosePosOnRiskFromForm(bar + 1, "");
                    }
                    else
                    {
                    }

                    ParamDebug("Направление", _dir);
                }

            }

        }


        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 02, 06);
            DesParamStratetgy.DateChange = new DateTime(2020, 02, 06);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec356184850";
            DesParamStratetgy.NameStrategy = "BbProboy";


        }

    }
}
