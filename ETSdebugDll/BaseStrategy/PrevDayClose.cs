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

namespace TestETSApi
{
    public class PrevDayClose : Script
    {

        double _prevPrice;
        double _percentChange;
        public override void Execute()
        {
            if (CandleCount < 2)
                return;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                ClosePrice(bar);
                _percentChange = PercentChange(bar);

                ParamDebug("Percent", _percentChange);
            }
            
        }

        /// <summary>
        /// Определение цены закрытия предыдущего дня
        /// </summary>
        /// <param name="bar"></param>
        private void ClosePrice(int bar)
        {
            if (Candles.DateTimeCandle[bar].Day != Candles.DateTimeCandle[bar + 1].Day)
            {
                _prevPrice = Candles.CloseSeries[bar];
            }
        }

        /// <summary>
        /// Расчёт процентного изменения цены относительно цены закрытия предыдущего дня
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        private double PercentChange(int bar)
        {
            double percent = 0;

            if (_prevPrice > 0)
            {
                percent = Math.Round((Candles.CloseSeries[bar] - _prevPrice) / _prevPrice * 100, 2);
            }

            return percent;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2015, 07, 27);
            DesParamStratetgy.DateChange = new DateTime(2020, 01, 29);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec363703939";
            DesParamStratetgy.NameStrategy = "PrevDayClose";
        }

    
    }
}
