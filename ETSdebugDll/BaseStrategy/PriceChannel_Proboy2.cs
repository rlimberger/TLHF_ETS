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
    internal class PriceChannel_Proboy2 : Script
    {
        public CreateIndicator ChannelUp = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");

        public ParamOptimization Proboy = new ParamOptimization(1, 5, 120, 5, "Величина пробоя",
            "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");
        public ParamOptimization Profit = new ParamOptimization(200, 5, 120, 5, "Прибыль, пункты",
      "Половина позиции закрывается при достижении заданной прибыли.  Данная величина умножается на шаг цены.");

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
                        BuyGreater(bar + 1, priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Позиция 1");
                        BuyGreater(bar + 1, priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Позиция 2");
                    }
                    if (LongPos.Count == 0 && ShortPos.Count == 0)
                    {
                        ShortLess(bar + 1, priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Позция 1");
                        ShortLess(bar + 1, priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Позция 2");
                    }

                    if (ShortPos.Count > 0)
                    {
                        //if (ShortPos.Count == 2)
                        //{
                        //    //CoverAtProfit(bar + 1, shortPos[1], shortPos[1].EntryPrice - Profit.ValueInt * FinInfo.MinStep, "Закрытие по профиту");
                        //    CoverLinkedStop(bar + 1, ShortPos[1], ShortPos[1].EntryPrice - Profit.ValueInt * FinInfo.Security.MinStep,
                        //        priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, "Связанная заявка");
                        //}

                        CoverAtStop(bar + 1, ShortPos[0], priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");

                        BuyGreater(bar + 1, priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Переворот. Позиция 1");
                        BuyGreater(bar + 1, priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Переворот. Позиция 2");
                    }

                    if (LongPos.Count > 0)
                    {
                        //if (LongPos.Count == 2)
                        //{
                        //    //SellAtProfit(bar + 1, longPos[1], longPos[1].EntryPrice + Profit.ValueInt * FinInfo.MinStep, "Закрытие по профиту");

                        //    SellLinkedStop(bar + 1, LongPos[1], LongPos[1].EntryPrice + Profit.ValueInt * FinInfo.Security.MinStep,
                        //        priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep,
                        //        "Связанная заявка");
                        //}

                        SellAtStop(bar + 1, LongPos[0], priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");

                        ShortLess(bar + 1, priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Переворот. Позиция 1");

                        ShortLess(bar + 1, priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Переворот. Позиция 2");
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
                        SendStandartStopFromForm(bar + 1, "");
                        SendTimePosCloseFromForm(bar + 1, "Время");
                        SendClosePosOnRiskFromForm(bar + 1, "");
                    }
                }

            }
        }



        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2015, 07, 27);
            DesParamStratetgy.DateChange = new DateTime(2019, 06, 14);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec363646199";
            DesParamStratetgy.NameStrategy = "PriceChannel Proboy2";

        }
    }
}
