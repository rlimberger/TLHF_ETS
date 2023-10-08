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

    public class HorizonalLevel_2 : Script
    {
        public ParamOptimization PriceCalc = new ParamOptimization(5, 5, 120, 5, "Количество лонг", "");
        public ParamOptimization LevelsLong = new ParamOptimization(5, 5, 120, 5, "Количество лонг", "");
        public ParamOptimization TypeTorg1 = new ParamOptimization("Лонг", new List<string>() { "Лонг", "Шорт" }, "Направлние 1", "Направление торговли для уровня 1");
        public ParamOptimization Profit1 = new ParamOptimization(1, 5, 120, 5, "Профит 1", "Указывается абсолютное значение");
        public ParamOptimization Level1 = new ParamOptimization(0, 5, 120, 5, "Уровень1", "Уровень 1, указывается вручную трейдером, если уровень равен 0, то торговля по не не ведется");
        public ParamOptimization Stop1 = new ParamOptimization(1, 5, 120, 5, "Стоп 1", "Указывается абсолютное значение");
        public ParamOptimization Level2 = new ParamOptimization(0, 5, 120, 5, "Уровень2", "Уровень 2, указывается вручную трейдером, если уровень равен 0, то торговля по не не ведется");
        public ParamOptimization TypeTorg2 = new ParamOptimization("Лонг", new List<string>() { "Лонг", "Шорт" }, "Направлние 2", "Направление торговли для уровня 2");
        public ParamOptimization Profit2 = new ParamOptimization(1, 5, 120, 5, "Профит 2", "Указывается абсолютное значение");
        public ParamOptimization Stop2 = new ParamOptimization(1, 5, 120, 5, "Стоп 2", "Указывается абсолютное значение");
        //public ParamOptimization Proboy2 = new ParamOptimization(1, 5, 120, 5, "Величина пробоя 2", "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");


        string crossLevel1 = "";
        string crossLevel2 = "";
        private double _prevPrice = 0;
        public override void Execute()
        {

            #region Открытие позиции. Уровень 1

            if (Math.Abs(Level1.Value) > 0.000000001 && _prevPrice > 0.0000001)
            {
                //цена пересекает уровень
                if (_prevPrice <= Level1.Value &&
                    FinInfo.Security.LastPrice > Level1.Value)
                    crossLevel1 = "Up";

                //цена пересекает уровень
                if (_prevPrice >= Level1.Value &&
                    FinInfo.Security.LastPrice < Level1.Value)
                    crossLevel1 = "Down";

                if (TypeTorg1.ValueString == "Лонг" && crossLevel1 == "Up")
                    BuyGreater(0, Level1.Value, 1, "Уровень 1");

                if (TypeTorg1.ValueString == "Шорт" && crossLevel1 == "Down")
                    ShortLess(0, Level1.Value, 1, "Уровень 1");
            }

            #endregion

            #region Открытие позиции. Уровень 2

            if (Math.Abs(Level2.Value) > 0.000000001 && _prevPrice > 0.0000001)
            {
                //цена пересекает уровень
                if (_prevPrice <= Level2.Value &&
                    FinInfo.Security.LastPrice > Level2.Value)
                    crossLevel2 = "Up";

                //цена пересекает уровень
                if (_prevPrice >= Level2.Value &&
                    FinInfo.Security.LastPrice < Level2.Value)
                    crossLevel2 = "Down";

                if (TypeTorg2.ValueString == "Лонг" && crossLevel2 == "Up")
                    BuyGreater(0, Level2.Value, 1, "Уровень 2");

                if (TypeTorg2.ValueString == "Шорт" && crossLevel2 == "Down")
                    ShortLess(0, Level2.Value, 1, "Уровень 2");
            }

            #endregion

            _prevPrice = FinInfo.Security.LastPrice;
            #region Закрытие позиции по времени, встроенным стопам и рискам
            if (LongPos.Count != 0 || ShortPos.Count != 0)
            {
                for (int i = LongPos.Count - 1; i >= 0; i--)
                {
                    if (LongPos[i].EntryNameSignal == "Уровень 1")
                        SendStandartStopFromForm(0, LongPos[i], "Закрытие", Profit1.Value, Stop1.Value, 0);
                    if (LongPos[i].EntryNameSignal == "Уровень 2")
                        SendStandartStopFromForm(0, LongPos[i], "Закрытие", Profit2.Value, Stop2.Value, 0);
                }
                for (int i = ShortPos.Count - 1; i >= 0; i--)
                {
                    if (ShortPos[i].EntryNameSignal == "Уровень 1")
                        SendStandartStopFromForm(0, ShortPos[i], "Закрытие", Profit1.Value, Stop1.Value, 0);
                    if (ShortPos[i].EntryNameSignal == "Уровень 2")
                        SendStandartStopFromForm(0, ShortPos[i], "Закрытие", Profit2.Value, Stop2.Value, 0);
                }

                SendTimePosCloseFromForm(0, "");
                crossLevel1 = "";
                crossLevel2 = "";
            }
            SendClosePosOnRiskFromForm(0, "");

            #endregion

            ParamDebug("Уровень 1", Level1.Value);
            ParamDebug("Тек. пересеч. 1", crossLevel1);
            ParamDebug("Уровень 2", Level2.Value);
            ParamDebug("Тек. пересеч. 2", crossLevel2);

        }

        public override void SetSettingDefault()
        {
            DefaultParamModel.TypeTorg = EnumTradingType.LongAndShort;
            DefaultParamModel.TypeTimeframe = EnumTimeFrame.NotUse;
            DefaultParamModel.IsNotCalculationScript = true;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "5";
            DesParamStratetgy.DateRelease = new DateTime(2015, 06, 21);
            DesParamStratetgy.DateChange = new DateTime(2021, 01, 25);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "";
            DesParamStratetgy.NameStrategy = "EM1";

        }
    }


}
