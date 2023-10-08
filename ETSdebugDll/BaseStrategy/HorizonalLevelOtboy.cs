using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Model;
using SourceEts;
using SourceEts.Config;

namespace ModulSolution.Robots
{

             /// <summary>
    /// Стратегия скользящие средние
    /// </summary>

    public class HorizonalLevelOtboy : Script
    {
        public ParamOptimization Level1 = new ParamOptimization(0, 5, 120, 5, "Уровень1", "Уровень 1, указывается вручную трейдером, если уровень равен 0, то торговля по не не ведется");
        public ParamOptimization TypeTorg1 = new ParamOptimization("Лонг", new List<string>() { "Лонг", "Шорт" }, "Направлние 1", "Направление торговли для уровня 1");
        public ParamOptimization Proboy1 = new ParamOptimization(1, 5, 120, 5, "Величина пробоя 1", "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");
        public ParamOptimization Level2 = new ParamOptimization(0, 5, 120, 5, "Уровень2", "Уровень 2, указывается вручную трейдером, если уровень равен 0, то торговля по не не ведется");
        public ParamOptimization TypeTorg2 = new ParamOptimization("Лонг", new List<string>() { "Лонг", "Шорт" }, "Направлние 2", "Направление торговли для уровня 2");
        public ParamOptimization Proboy2 = new ParamOptimization(1, 5, 120, 5, "Величина пробоя 2", "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");


        string crossLevel1 = "";
        string crossLevel2 = "";
        private double _prevPrice = 0;

        public override void Execute()
        {

            #region Открытие позиции. Уровень 1

            if (Math.Abs(Level1.Value) > 0.000000001)
            {
                //цена пересекает уровень
                if (_prevPrice < Level1.Value &&
                    FinInfo.Security.LastPrice >= Level1.Value)
                    crossLevel1 = "Down";

                //цена пересекает уровень
                if (_prevPrice > Level1.Value &&
                    FinInfo.Security.LastPrice <= Level1.Value)
                    crossLevel1 = "Up";

                if (TypeTorg1.ValueString == "Лонг" && crossLevel1 == "Up")
                    BuyAtLimit(0, Level1.Value- Proboy1.ValueInt*FinInfo.Security.MinStep, 1, "Уровень 1");

                if (TypeTorg1.ValueString == "Шорт" && crossLevel1 == "Down")
                    ShortAtLimit(0, Level1.Value + Proboy1.ValueInt * FinInfo.Security.MinStep, 1, "Уровень 1");
            }

            #endregion

            #region Открытие позиции. Уровень 2

            if (Math.Abs(Level2.Value) > 0.000000001)
            {
                //цена пересекает уровень
                if (_prevPrice <= Level2.Value &&
                    FinInfo.Security.LastPrice > Level2.Value)
                    crossLevel2 = "Down";

                //цена пересекает уровень
                if (_prevPrice >= Level2.Value &&
                    FinInfo.Security.LastPrice < Level2.Value)
                    crossLevel2 = "Up";

                if (TypeTorg2.ValueString == "Лонг" && crossLevel2 == "Up")
                    BuyAtLimit(0, Level2.Value - Proboy2.ValueInt * FinInfo.Security.MinStep, 1, "Уровень 2");

                if (TypeTorg2.ValueString == "Шорт" && crossLevel2 == "Down")
                    ShortAtLimit(0, Level2.Value + Proboy2.ValueInt * FinInfo.Security.MinStep, 1, "Уровень 2");
            }

            #endregion

            #region Закрытие позиции по времени, встроенным стопам и рискам
            if (LongPos.Count != 0 || ShortPos.Count != 0)
            {
                SendStandartStopFromForm(0, "");
                SendTimePosCloseFromForm(0, "");
                SendClosePosOnRiskFromForm(0, "");
                crossLevel1 = "";
                crossLevel2 = "";
            }
            #endregion



        }


        public override void SetSettingDefault()
        {
            DefaultParamModel.TypeTimeframe = EnumTimeFrame.NotUse;
            DefaultParamModel.IsNotCalculationScript = true;
            DefaultParamModel.TypeTorg = EnumTradingType.LongAndShort;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2019, 06, 15);
            DesParamStratetgy.DateChange = new DateTime(2019, 10, 24);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Level;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/fzzze8vl11-robot-horizonallevelotboy-gorizontalnie";
            DesParamStratetgy.NameStrategy = "HorizonalLevelOtboy";

        }
    }

    
}
