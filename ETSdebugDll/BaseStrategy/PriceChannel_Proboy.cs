using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Indicators.Model;
using ScriptSolution.Model;

namespace ModulSolution.Robots
{
    internal class PriceChannel_Proboy : Script
    {
        public CreateIndicator ChannelUp = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");
        public ParamOptimization Proboy = new ParamOptimization(1, 5, 120, 5, "Величина пробоя",
            "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");

        private string _dir { get; set; }
        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {


                if (ChannelUp.param.LinesIndicators[0].LineParam[0].Value < bar)
                {
                    var priceUp = ChannelUp.param.LinesIndicators[0].PriceSeries;
                    var priceDown = ChannelUp.param.LinesIndicators[1].PriceSeries;

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


                    if (bar > 2)
                    {
                        ParamDebug("Верх. канал тек.",
                            Math.Round(ChannelUp.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                        ParamDebug("Ниж. канал тек.",
                            Math.Round(ChannelUp.param.LinesIndicators[1].PriceSeries[bar + 1], 4));
                    }

                    if (LongPos.Count != 0 || ShortPos.Count != 0)
                    {
                        if (LongPos.Count != 0)
                            _dir = "Long";
                        if (ShortPos.Count != 0)
                            _dir = "Short";

                        SendStandartStopFromForm(bar + 1, "");
                        SendTimePosCloseFromForm(bar + 1, "");
                        SendClosePosOnRiskFromForm(bar + 1, "");
                    }
                    //else
                    //{
                    //    _dir = "";
                    //}

                    ParamDebug("Направление", _dir);
                }

            }

        }



        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "7";
            DesParamStratetgy.DateRelease = new DateTime(2015, 07, 27);
            DesParamStratetgy.DateChange = new DateTime(2020, 01, 29);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/uejm081tn1-robot-pricechannel-proboy-proboi-kanala";
            DesParamStratetgy.NameStrategy = "PriceChannel Proboy";

        }
    }
}
