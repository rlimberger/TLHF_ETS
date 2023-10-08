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
    public class AvgVolume : Script
    {
        public ParamOptimization BarsQuantity = new ParamOptimization(10, 1, 1000, 1, "Колл-во свечей", "Кол-во свечей для расчёта средней");
        public ParamOptimization Koef = new ParamOptimization(1, 1, 1000, 1, "Коэф. изм.", "Множитель для среднего объема для открытия позиции. Если объем на закрытой свече будет выше чем средний объем умноженный на данный коэффициент, то будет открыта позиция");
        public ParamOptimization UseRevers = new ParamOptimization(false, "Выход по обрат. сигналу", "Выход из позиции по обратному сигналу");

        double _avgVol;
        DateTime _dateTime = DateTime.MinValue;
        int _lastBar = -1;
        string _dir = "";

        public override void Execute()
        {
            if (IndexBar <= BarsQuantity.ValueInt)
                return;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 1)
                    continue;

                if (bar != _lastBar)
                {
                    _lastBar = bar;
                    CalculateAvgVol(bar - 1);

                }
                //ParamDebug("Volume Bar+1", Candles.Volume[bar + 1]); // Объём текущего бара
                ParamDebug("Volume Bar", Candles.Volume[bar], bar); // Объём предыдущего бара
                ParamDebug("AvgVol", _avgVol, bar);
                if (UseRevers.ValueBool)
                    ParamDebug("Direction", _dir, bar);

                if (Candles.Volume[bar] > _avgVol * Koef.Value)
                {
                    if (Candles.OpenSeries[bar] < Candles.CloseSeries[bar])
                    {
                        PlotArea("Long", bar, new UserChartPropModel() { ColorLine = Color.LightGreen });
                        if (LongPos.Count == 0 && UseRevers.ValueBool ||
                            !UseRevers.ValueBool && LongPos.Count == 0 && ShortPos.Count == 0 && _dir != "Long")
                        {
                            if (ShortPos.Count > 0)
                                CoverAtMarket(bar + 1, ShortPos[0], "Переворот");
                            BuyAtMarket(bar + 1, 1, "Открытие");
                        }
                        _dir = "Long";

                    }

                    if (Candles.OpenSeries[bar] > Candles.CloseSeries[bar])
                    {
                        PlotArea("Short", bar, new UserChartPropModel() { ColorLine = Color.LightPink });
                        if (ShortPos.Count == 0 && UseRevers.ValueBool ||
                            !UseRevers.ValueBool && LongPos.Count == 0 && ShortPos.Count == 0 && _dir != "Short")
                        {
                            if (LongPos.Count > 0)
                                SellAtMarket(bar + 1, LongPos[0], "Переворот");
                            ShortAtMarket(bar + 1, 1, "Открытие");

                        }
                        _dir = "Short";

                    }
                }
                else
                {
                    if (LongPos.Count == 0 && ShortPos.Count == 0)
                        _dir = "";
                }
                //if (Candles.Volume[bar + 1] > _avgVol && _dateTime != Candles.DateTimeCandle[bar + 1])
                //{
                //    _dateTime = Candles.DateTimeCandle[bar + 1];
                //    //PlotArea("signal", bar + 1, new UserChartPropModel() { ColorLine = Color.Green });

                //    string message = FinInfo.Security.Symbol + "  текущий объем="
                //                     + Candles.Volume[bar + 1] + " ср. объем="
                //                     + _avgVol;

                //    SendAlert(message);
                //}



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

        /// <summary>
        /// Расчет срденего объема
        /// </summary>
        /// <param name="bar"></param>
        private void CalculateAvgVol(int bar)
        {
            _avgVol = 0;
            for (int i = bar; i > bar - BarsQuantity.ValueInt; i--)
            {
                _avgVol += Candles.Volume[i];
            }

            _avgVol = _avgVol / BarsQuantity.ValueInt;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "4";
            DesParamStratetgy.DateRelease = new DateTime(2021, 6, 19);
            DesParamStratetgy.DateChange = new DateTime(2023, 2, 21);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/60odb2o7n1-robot-avgvolume";
            DesParamStratetgy.NameStrategy = "AvgVolume";

        }

        
    }
}
