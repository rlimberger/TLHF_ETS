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
    internal class PirceChannelOtboy : Script
    {
        public CreateIndicator ChannelUp = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");

        public ParamOptimization MaxChannel = new ParamOptimization(1, 5, 120, 5, "Max канал",
            "Максимальная ширина канала, при которой откроется позиция. Данная величина умножается на шаг цены.");

        public ParamOptimization ReversSignal = new ParamOptimization(false, "Сигнал в обратку", "Если включен данный пункт, то вход в позицию будет череодовать лонг, потом шорт и т.д.");
        public ParamOptimization Proboy = new ParamOptimization(1, 5, 120, 5, "Величина пробоя",
    "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");

        private string _signal = "";
        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (ChannelUp.param.LinesIndicators[0].LineParam[0].Value < bar)
                {
                    var priceUp = ChannelUp.param.LinesIndicators[0].PriceSeries;
                    var priceDown = ChannelUp.param.LinesIndicators[1].PriceSeries;
                    var longPos = GetLongPosition(FinInfo);
                    var shortPos = GetShortPositions(FinInfo);
                    var width = priceUp[bar + 1] - priceDown[bar + 1];

                    if (longPos.Count == 0 && shortPos.Count == 0 && width < MaxChannel.ValueInt * FinInfo.Security.MinStep)
                    {
                        if (MarketPosition == 0 && (!ReversSignal.ValueBool ||
                            ReversSignal.ValueBool && _signal != "short"))
                            ShortAtLimit(bar + 1, priceUp[bar + 1] +Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                "Открытие короткой позиции");
                        if (MarketPosition == 0 && (!ReversSignal.ValueBool ||
                            ReversSignal.ValueBool && _signal != "long"))
                            BuyAtLimit(bar + 1, priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                "Открытие длинной позиции");

                        if (ShortPos.Count > 0)
                            _signal = "short";

                        if (LongPos.Count > 0)
                            _signal = "long";
                    }

                    if (longPos.Count > 0)
                    {
                        SellAtProfit(bar + 1, longPos[0], priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");
                        if (width < MaxChannel.ValueInt * FinInfo.Security.MinStep)
                        {
                            ShortAtLimit(bar + 1, priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                "Переворот. Открытие короткой позиции");
                            if (ShortPos.Count > 0)
                                _signal = "short";
                        }
                    }

                    if (shortPos.Count > 0)
                    {
                        CoverAtProfit(bar + 1, shortPos[0], priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");
                        if (width < MaxChannel.ValueInt * FinInfo.Security.MinStep)
                        {
                            BuyAtLimit(bar + 1, priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                                "Переворот. Открытие длинной позиции");
                            if (LongPos.Count > 0)
                                _signal = "long";
                        }

                    }


                    if (bar > 2)
                    {
                        ParamDebug("Верх. канал тек.",
                            Math.Round(ChannelUp.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                        ParamDebug("Ниж. канал тек.", Math.Round(ChannelUp.param.LinesIndicators[1].PriceSeries[bar + 1], 4));
                        ParamDebug("Ширина канала", width);
                        ParamDebug("Последний сигнал", _signal);

                    }


                    if (MarketPosition != 0)
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
            DesParamStratetgy.DateRelease = new DateTime(2018, 11, 19);
            DesParamStratetgy.DateChange = new DateTime(2020, 02, 08);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec361071409";
            DesParamStratetgy.NameStrategy = "PriceChannelOtboy";

        }
    }
}
