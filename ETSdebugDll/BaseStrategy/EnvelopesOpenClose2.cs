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
    ///
    /// </summary>

    public class EnvelopesOpenClose2 : Script
    {
        public CreateIndicator Envelopes = new CreateIndicator(EnumIndicators.Envelopes, 0, "");
        public ParamOptimization Proboy = new ParamOptimization(1, 5, 120, 5, "Величина пробоя",
    "Величина на которую должен пробиться полоса. Данная величина умножается на шаг цены.");

        private string _dir { get; set; }
        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (Envelopes.param.LinesIndicators[0].LineParam[0].Value < bar)
                {
                    if (bar == 86)
                    { }

                    var middle = Envelopes.param.LinesIndicators[0].PriceSeries;
                    var priceUp = Envelopes.param.LinesIndicators[1].PriceSeries;
                    var priceDown = Envelopes.param.LinesIndicators[2].PriceSeries;

                    if (ShortPos.Count >= 0 && _dir != "Long" &&
                        Candles.CloseSeries[bar] >= priceUp[bar] + Proboy.ValueInt * FinInfo.Security.MinStep)
                    {
                        if (ShortPos.Count > 0)
                            CoverAtMarket(bar + 1, ShortPos[0], "Переворот.");
                        var pos = BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                        if (pos != null)
                            _dir = "Long";
                    }

                    if (LongPos.Count >= 0 && _dir != "Short" &&
                        Candles.CloseSeries[bar] <= priceDown[bar] - Proboy.ValueInt * FinInfo.Security.MinStep)
                    {
                        if (LongPos.Count > 0)
                            SellAtMarket(bar + 1, LongPos[0], "Переворот.");
                        var pos = ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                        if (pos != null)
                            _dir = "Short";

                    }

                    if (middle[bar - 1] > Candles.CloseSeries[bar - 1] &&
                        middle[bar] <= Candles.CloseSeries[bar])
                    {
                        if (ShortPos.Count > 0)
                            CoverAtMarket(bar + 1, ShortPos[0], "Закрытие по средней линии.");
                        if (_dir == "Short")
                            _dir = "";
                    }

                    if (
                        middle[bar - 1] < Candles.CloseSeries[bar - 1] &&
                        middle[bar] >= Candles.CloseSeries[bar])
                    {
                        if (LongPos.Count > 0)
                            SellAtMarket(bar + 1, LongPos[0], "Закрытие по средней линии.");
                        if (_dir == "Long")
                            _dir = "";
                    }

                    if (bar > 2)
                    {
                        ParamDebug("Верх. канал ",
                                    Math.Round(priceUp[bar], FinInfo.Security.Accuracy + 2));
                        ParamDebug("Ниж. канал ",
                            Math.Round(priceDown[bar], FinInfo.Security.Accuracy + 2));
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
            DesParamStratetgy.DateRelease = new DateTime(2020, 02, 09);
            DesParamStratetgy.DateChange = new DateTime(2020, 02, 17);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec587273961";
            DesParamStratetgy.NameStrategy = "EnvelopesClose2";

        }

    }
}
