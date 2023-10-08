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
    internal class PriceChannel_Proboy4 : Script
    {
        public CreateIndicator ChannelUp = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");
        public ParamOptimization Proboy = new ParamOptimization(1, 5, 120, 5, "Величина пробоя",
            "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");

        private string _dir { get; set; }
        private double _high = 0;
        private double _low = 0;
        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (ChannelUp.param.LinesIndicators[0].LineParam[0].Value < bar)
                {
                    var priceUp = ChannelUp.param.LinesIndicators[0].PriceSeries;
                    var priceDown = ChannelUp.param.LinesIndicators[1].PriceSeries;


                    if (Candles.HighSeries[bar] >= priceUp[bar] && _dir != "Long")
                    {
                        _high = Candles.HighSeries[bar];
                        _low = 0;
                        _dir = "Long";
                    }

                    if (Candles.LowSeries[bar] <= priceDown[bar] && _dir != "Short")
                    {
                        _low = Candles.LowSeries[bar];
                        _high = 0;
                        _dir = "Short";
                    }


                    if (ShortPos.Count > 0)
                        CoverAtStop(bar + 1, ShortPos[0], priceUp[bar + 1] + Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");
                    if (_dir == "Long" && _high > 0.000000001)
                        BuyGreater(bar + 1, _high + Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                            "Переворот. Открытие длинной позиции");

                    if (LongPos.Count > 0)
                        SellAtStop(bar + 1, LongPos[0], priceDown[bar + 1] - Proboy.ValueInt * FinInfo.Security.MinStep,
                            "Переворот.");
                    if (_dir == "Short" && _low > 0.000000001)
                        ShortLess(bar + 1, _low - Proboy.ValueInt * FinInfo.Security.MinStep, 1,
                        "Переворот. Открытие короткой позиции");

                    if (bar > 2)
                    {
                        ParamDebug("Направление", _dir);
                        ParamDebug("High пробоя", _high);
                        ParamDebug("Low пробоя", _low);
                    }

                    if (LongPos.Count != 0 || ShortPos.Count != 0)
                    {
                        if (LongPos.Count != 0)
                            _high = 0;
                        if (ShortPos.Count != 0)
                            _low = 0;

                        SendStandartStopFromForm(bar + 1, "");
                        SendTimePosCloseFromForm(bar + 1, "");
                        SendClosePosOnRiskFromForm(bar + 1, "");
                    }

                    ParamDebug("Направление", _dir);
                }

            }

        }


        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 02, 08);
            DesParamStratetgy.DateChange = new DateTime(2020, 02, 08);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec363678086";
            DesParamStratetgy.NameStrategy = "PriceChannel Proboy4";

        }
    }
}
