using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Model;
using SourceEts.Config;

namespace ModulSolution.Robots
{

             /// <summary>
    /// Стратегия скользящие средние
    /// </summary>

    public class SignalDeposit : Script
    {

        public ParamOptimization MinValue = new ParamOptimization(1, 5, 120, 5, "Мин. депозит", "");
        DateTime _dateSignal = new DateTime();
        public override void Execute()
        {
            if (_dateSignal.Year == 1)
                _dateSignal = Service.ServerTime;
            //if ()


        }


        public override void SetSettingDefault()
        {
            DefaultParamModel.TypeTimeframe = EnumTimeFrame.NotUse;
        }



        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2020, 10, 19);
            DesParamStratetgy.DateChange = new DateTime(2020, 10, 19);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "";
            DesParamStratetgy.NameStrategy = "SignalDeposit";

        }
    }

    
}
