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

    public class ChaikinVolatility : Script
    {
        public CreateIndicator ChaikinVol = new CreateIndicator(EnumIndicators.ChaikinVolatility, 1, "");

        public override void Execute()
        {
            PlotLine("0", 0, new SourceEts.Models.UserChartPropModel { NumberPanel = 1, ColorLine = Color.Black });
        }


        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 09, 11);
            DesParamStratetgy.DateChange = new DateTime(2020, 09, 13);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.None;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/8zlmuo9nk1-robot-volatilnost-chaikina";
            DesParamStratetgy.NameStrategy = "ChaikinVolatility";
        }

    
    }

    
}
