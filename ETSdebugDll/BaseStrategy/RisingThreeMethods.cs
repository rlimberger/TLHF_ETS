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
    public class RisingThreeMethods : Script
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
            if (CandleCount < 6)
                return "";

            if (Candles.OpenSeries[bar - 4] < Candles.CloseSeries[bar - 4]
                && Candles.OpenSeries[bar - 3] > Candles.CloseSeries[bar - 3] 
                && Candles.HighSeries[bar - 3] <= Candles.HighSeries[bar - 4]
                && Candles.LowSeries[bar - 3] > Candles.LowSeries[bar - 2]
                && Candles.CloseSeries[bar - 3] < Candles.OpenSeries[bar - 2]
                && Candles.OpenSeries[bar - 2] > Candles.CloseSeries[bar - 2] 
                && Candles.OpenSeries[bar - 2] < Candles.OpenSeries[bar - 3]
                && Candles.CloseSeries[bar - 2] < Candles.CloseSeries[bar - 3]
                && Candles.LowSeries[bar - 2] > Candles.LowSeries[bar - 1]
                && Candles.OpenSeries[bar - 1] > Candles.CloseSeries[bar - 1]
                && Candles.OpenSeries[bar - 1] > Candles.CloseSeries[bar - 2]
                && Candles.LowSeries[bar - 1] >= Candles.LowSeries[bar - 4]
                && Candles.CloseSeries[bar] > Candles.CloseSeries[bar - 4])
            {
                PlotArea("Rising", bar, new UserChartPropModel() { ColorLine = Color.LightGreen });
                return "buy";
            }
            if (Candles.OpenSeries[bar - 4] > Candles.CloseSeries[bar - 4]
                && Candles.OpenSeries[bar - 3] < Candles.CloseSeries[bar - 3]
                && Candles.LowSeries[bar - 3] >= Candles.LowSeries[bar - 4]
                && Candles.HighSeries[bar - 3] < Candles.HighSeries[bar - 2]
                && Candles.OpenSeries[bar - 2] < Candles.CloseSeries[bar - 2]
                && Candles.OpenSeries[bar - 2] > Candles.OpenSeries[bar - 3]
                && Candles.OpenSeries[bar - 2] < Candles.CloseSeries[bar - 3]
                && Candles.HighSeries[bar - 2] < Candles.HighSeries[bar - 1]
                && Candles.OpenSeries[bar - 1] < Candles.CloseSeries[bar - 1]
                && Candles.CloseSeries[bar - 1] > Candles.CloseSeries[bar - 2]
                && Candles.HighSeries[bar - 1] <= Candles.HighSeries[bar - 4]
                && Candles.CloseSeries[bar] < Candles.CloseSeries[bar - 4])
            {
                PlotArea("Falling", bar, new UserChartPropModel() { ColorLine = Color.OrangeRed });
                return "sell";
            }
            return "";
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && DetectFormation(bar) == "buy")
                BuyAtMarket(bar + 1, 1, "Long");
            else if (LongPos.Count == 0 && ShortPos.Count == 0 && DetectFormation(bar) == "sell")
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
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/96iznlroo1-8-svechnih-robotov";
            DesParamStratetgy.NameStrategy = "RisingThreeMethods";
        }
    }
}
