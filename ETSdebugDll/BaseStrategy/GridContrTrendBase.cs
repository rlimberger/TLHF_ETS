
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceEts;
using SourceEts.Config;

namespace Robot.Роботы_на_заказ
{
    internal class GridContrTrendBase : Script
    {
        public ParamOptimization StartPrice = new ParamOptimization(50, 50, 100, 10, "Начальная цена", "Цена от которой рассчитывается сетка, и только при достижении цены будет начинатся торговля");
        public ParamOptimization StepGrid = new ParamOptimization(50, 50, 100, 10, "Шаг сетки", "Указывается в валюте цены");
        public ParamOptimization CountLevel = new ParamOptimization(2, 2, 5, 1, "Кол-во уровней");
        public ParamOptimization Profit = new ParamOptimization(2, 2, 5, 1, "Профит", "Указывается в валюте цены");


        public override void Execute()
        {
            if (Service.TypeTorg==EnumTradingType.Long)
            {
                for (int i = 0; i < CountLevel.ValueInt; i++)
                {
                    var level = Math.Round(StartPrice.Value - i * StepGrid.Value, FinInfo.Security.Accuracy);
                    ParamDebug("Уровень " + (i + 1), level);
                    BuyAtLimit(0, level, 1, level.ToString());
                }

                for (int i = LongPos.Count - 1; i >= 0; i--)
                {
                    SellAtProfit(0, LongPos[i], Convert.ToDouble(LongPos[i].EntryNameSignal) + Profit.Value, "Профит");
                }
            }

            if (Service.TypeTorg == EnumTradingType.Short)
            {
                for (int i = 0; i < CountLevel.ValueInt; i++)
                {
                    var level = Math.Round(StartPrice.Value + i * StepGrid.Value, FinInfo.Security.Accuracy);
                    ParamDebug("Уровень " + (i + 1), level);
                    ShortAtLimit(0, level, 1, level.ToString());
                }

                for (int i = ShortPos.Count - 1; i >= 0; i--)
                {
                    CoverAtProfit(0, ShortPos[i], Convert.ToDouble(ShortPos[i].EntryNameSignal) - Profit.Value, "Профит");
                }
            }


            if (LongPos.Count != 0 || ShortPos.Count != 0)
            {
                SendTimePosCloseFromForm(0, "");
                SendClosePosOnRiskFromForm(0, "");
            }

        }

        public override void SetSettingDefault()
        {
            DefaultParamModel.TypeTorg = EnumTradingType.LongAndShort;
            DefaultParamModel.IsNotCalculationScript = true;
            DefaultParamModel.TypeTimeframe = EnumTimeFrame.NotUse;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2023, 05, 25);
            DesParamStratetgy.DateChange = new DateTime(2023, 05, 25);
            DesParamStratetgy.Author = "РобоКоммерц";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.NameStrategy = "Сетка конттренд";
        }
    }
}
