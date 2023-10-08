using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Model;
using ScriptSolution.ScanerModel;
using SourceEts;
using SourceEts.Config;
using SourceEts.Models;

namespace ModulSolution.Robots
{

    /// <summary>
    /// Стратегия скользящие средние
    /// </summary>

    public class HorizonalLevelCandle : Script
    {
        public ParamOptimization Level1 = new ParamOptimization(0, 5, 120, 5, "Уровень1", "Уровень 1, указывается вручную трейдером, если уровень равен 0, то торговля по не не ведется");
        public ParamOptimization TypeTorg1 = new ParamOptimization("Лонг", new List<string>() { "Лонг", "Шорт" }, "Направлние 1", "Направление торговли для уровня 1");
        //public ParamOptimization Proboy1 = new ParamOptimization(1, 5, 120, 5, "Величина пробоя 1", "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");
        public ParamOptimization Level2 = new ParamOptimization(0, 5, 120, 5, "Уровень2", "Уровень 2, указывается вручную трейдером, если уровень равен 0, то торговля по не не ведется");
        public ParamOptimization TypeTorg2 = new ParamOptimization("Лонг", new List<string>() { "Лонг", "Шорт" }, "Направлние 2", "Направление торговли для уровня 2");
        //public ParamOptimization Proboy2 = new ParamOptimization(1, 5, 120, 5, "Величина пробоя 2", "Величина на которую должен пробиться уровень. Данная величина умножается на шаг цены.");


        string crossLevel1 = "";
        string crossLevel2 = "";
        private double _prevPrice = 0;
        public override void Execute()
        {

            #region Открытие позиции. Уровень 1
            PlotLine("Уровень1", Level1.Value, new SourceEts.Models.UserChartPropModel { ColorLine = Color.Black });
            PlotLine("Уровень2", Level2.Value, new SourceEts.Models.UserChartPropModel { ColorLine = Color.Brown });

            #endregion
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {


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
                        BuyGreater(bar + 1, Level1.Value, 1, "Уровень 1");

                    if (TypeTorg1.ValueString == "Шорт" && crossLevel1 == "Down")
                        ShortLess(bar + 1, Level1.Value, 1, "Уровень 1");
                }


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
                        BuyGreater(bar + 1, Level2.Value, 1, "Уровень 2");

                    if (TypeTorg2.ValueString == "Шорт" && crossLevel2 == "Down")
                        ShortLess(bar + 1, Level2.Value, 1, "Уровень 2");
                }

                #endregion

                _prevPrice = FinInfo.Security.LastPrice;
                #region Закрытие позиции по времени, встроенным стопам и рискам
                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                    crossLevel1 = "";
                    crossLevel2 = "";
                }
                foreach (var item in GetPosition())
                {
                    if (item.IsLong)
                    {
                        PlotLine("Профит лонг", item.CalcProfit, new UserChartPropModel { ColorLine = Color.Green });
                        PlotLine("Стоп лонг", item.CalcStop, new UserChartPropModel { ColorLine = Color.Red });
                    }
                    if (!item.IsLong)
                    {
                        PlotLine("Профит шорт", item.CalcProfit, new UserChartPropModel { ColorLine = Color.Green });
                        PlotLine("Стоп шорт", item.CalcStop, new UserChartPropModel { ColorLine = Color.Red });
                    }
                }

                SendClosePosOnRiskFromForm(bar + 1, "");

                #endregion
            }

            ParamDebug("Уровень 1", Level1.Value);
            ParamDebug("Тек. пересеч. 1", crossLevel1);
            ParamDebug("Уровень 2", Level2.Value);
            ParamDebug("Тек. пересеч. 2", crossLevel2);

            MainTableColumn[0].ValueString = Level1.Value + " (" + TypeTorg1.ValueString + ")";
            MainTableColumn[1].ValueString = Level2.Value + " (" + TypeTorg2.ValueString + ")";

        }

        public override List<ScanerParamModel> AddColumnToMainTable()
        {
            List<ScanerParamModel> list = new List<ScanerParamModel>();
            ScanerParamModel param = new ScanerParamModel();

            param = new ScanerParamModel();
            param.Name = "Уровень1";
            param.TypeValue = CfgSourceEts.EnumScanerParamType.typeString;
            list.Add(param);

            param = new ScanerParamModel();
            param.Name = "Уровень2";
            param.TypeValue = CfgSourceEts.EnumScanerParamType.typeString;
            list.Add(param);


            return list;
        }

        public override void SetSettingDefault()
        {
            DefaultParamModel.TypeTorg = EnumTradingType.LongAndShort;
            DefaultParamModel.IsNotCalculationScript = true;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2015, 06, 21);
            DesParamStratetgy.DateChange = new DateTime(2021, 11, 23);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec356604981";
            DesParamStratetgy.NameStrategy = "HorizonalLevelCandle";

        }
    }


}
