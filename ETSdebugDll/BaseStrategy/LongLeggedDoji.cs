using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Indicators.Model;
using ScriptSolution.Model;
using ScriptSolution.Model.Interfaces;
using SourceEts.Models;

namespace ETSBots.ETSBots.CandleFormations
{
    public class LongLeggedDoji : Script
    {
        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                Entry(bar);

                #region Закрытие позиции по времени, встроенным стопам и рискам
                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                    SendClosePosOnRiskFromForm(bar + 1, "");
                }
                #endregion
            }
        }

        private string DetectFormation(int bar)
        {
            if (Candles.OpenSeries[bar] == Candles.CloseSeries[bar]
                && Candles.HighSeries[bar] - Candles.CloseSeries[bar] == Candles.CloseSeries[bar] - Candles.LowSeries[bar]
                && Candles.OpenSeries[bar] - Candles.CloseSeries[bar] < Candles.CloseSeries[bar] - Candles.LowSeries[bar])
            {
                PlotArea("Plus", bar, new UserChartPropModel() { ColorLine = Color.LightGreen });
                return "trade";
            }
            return "";
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && DetectFormation(bar) == "trade")
                BuyAtMarket(bar + 1, 1, "Long");
            if (LongPos.Count == 0 && ShortPos.Count == 0 && DetectFormation(bar) == "trade")
                ShortAtMarket(bar + 1, 1, "Short");
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 06, 16);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 16);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Candle;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/96iznlroo1-8-svechnih-robotov";
            DesParamStratetgy.NameStrategy = "LongLeggedDoji";
        }
    }
}
