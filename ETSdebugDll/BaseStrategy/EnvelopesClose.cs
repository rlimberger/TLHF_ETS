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
    /// Стратегия по конвретам
    /// </summary>

    public class EnvelopesClose : Script
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
                    var priceUp = Envelopes.param.LinesIndicators[1].PriceSeries;
                    var priceDown = Envelopes.param.LinesIndicators[2].PriceSeries;

                    if (ShortPos.Count >= 0 && _dir != "Long" &&
                        Candles.CloseSeries[bar] >= priceUp[bar] + Proboy.ValueInt * FinInfo.Security.MinStep)
                    {
                        _dir = "Long";
                        if (ShortPos.Count > 0)
                            CoverAtMarket(bar + 1, ShortPos[0], "Переворот.");
                        BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                    }

                    if (LongPos.Count >= 0 && _dir != "Short" &&
                        Candles.CloseSeries[bar] <= priceDown[bar] - Proboy.ValueInt * FinInfo.Security.MinStep)
                    {
                        _dir = "Short";
                        if (LongPos.Count > 0)
                            SellAtMarket(bar + 1, LongPos[0], "Переворот.");
                        ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
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
            DesParamStratetgy.DateRelease = new DateTime(2020, 07, 13);
            DesParamStratetgy.DateChange = new DateTime(2020, 07, 13);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/i5be0mfi01-robot-envelopesclose-konverti";
            DesParamStratetgy.NameStrategy = "EnvelopesClose";

        }

    }
}
