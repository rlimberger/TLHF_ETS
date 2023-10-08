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

    public class BbOpenProboyRevers : Script
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

                    if (ShortPos.Count >= 0 && _dir != "Short")
                    {
                        _dir = "Short";
                        if (LongPos.Count > 0)
                            SellAtProfit(bar + 1, LongPos[0], priceUp[bar] + Proboy.ValueInt * FinInfo.Security.MinStep, "Переворот.");
                        ShortAtLimit(bar + 1, priceUp[bar] + Proboy.ValueInt * FinInfo.Security.MinStep, 1, "Открытие короткой позиции");
                    }

                    if (LongPos.Count >= 0 && _dir != "Long" )
                    {
                        _dir = "Long";
                        if (ShortPos.Count > 0)
                            CoverAtProfit(bar + 1, ShortPos[0], priceDown[bar] - Proboy.ValueInt * FinInfo.Security.MinStep, "Переворот.");
                        BuyAtLimit(bar + 1, priceDown[bar] - Proboy.ValueInt * FinInfo.Security.MinStep, 1, "Открытие длинной позиции");
                    }

                    if (LongPos.Count != 0 || ShortPos.Count != 0)
                    {
                        SendStandartStopFromForm(bar + 1, "");
                        SendTimePosCloseFromForm(bar + 1, "");
                    }
                    SendClosePosOnRiskFromForm(bar + 1, "");

                    ParamDebug("Направление", _dir);
                }
            }
        }

        public override void SetSettingDefault()
        {

        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "4";
            DesParamStratetgy.DateRelease = new DateTime(2020, 02, 06);
            DesParamStratetgy.DateChange = new DateTime(2020, 02, 06);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec356177679";
            DesParamStratetgy.NameStrategy = "BbOpenProboyRevers";

        }

    }
}
