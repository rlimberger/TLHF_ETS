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

namespace ETSBots
{
    public class BollingerRsi : Script
    {
        public CreateIndicator Rsi = new CreateIndicator(EnumIndicators.Rsi, 1, "");
        public CreateIndicator Bollinger = new CreateIndicator(EnumIndicators.BollinderBands, 1, "");


        public override void Execute()
        {
            

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                Entry(bar);
                Exit(bar);

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

        private void Entry(int bar)
        {
            
        }

        private void Exit(int bar)
        {

        }
        


        public override void GetAttributesStratetgy()
        {
           

            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 06, 23);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 23);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/ecdhr029v1-robot-bollindzherrsi";
            DesParamStratetgy.NameStrategy = "BollingerRsi";
        }
    }
}
