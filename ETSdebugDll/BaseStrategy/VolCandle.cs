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
using SourceEts.Config;
using SourceEts.Models;


namespace ETSBots
{
    public class VolCandle : Script
    {
        public ParamOptimization Size = new ParamOptimization(30, 1, 50000, 1, "Процент стопа", "Процент  от тела предыдущей свечи (Close-Open) по модулю. Данная величина откладывается от цены открытия свечи");
        public ParamOptimization Param = new ParamOptimization(true, "Расчёт по закрытию свечи", "Расчёт по закрытию свечи - True, либо по текущему значению - False");
        public ParamOptimization Time = new ParamOptimization(true, "Настройка выхода", "Допускать выход на свече входа - True, не допускать - False");


        DateTime _time = DateTime.MinValue;

        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (CandleCount - 1 < 2)
                    return;

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
            if (Param.ValueBool)
            {
                if ((!Time.ValueBool || Time.ValueBool && _time != Candles.DateTimeCandle[bar + 1]))
                    if (LongPos.Count == 0 && ShortPos.Count == 0 && Candles.CloseSeries[bar - 1] > Candles.OpenSeries[bar - 1]
                        && Candles.CloseSeries[bar] > Candles.OpenSeries[bar] && Candles.Volume[bar - 1] < Candles.Volume[bar])
                    {
                        BuyAtMarket(bar + 1, 1, "Long");
                        _time = Candles.DateTimeCandle[bar + 1];
                    }
                    else
                    if (LongPos.Count == 0 && ShortPos.Count == 0 && Candles.CloseSeries[bar - 1] < Candles.OpenSeries[bar - 1]
                    && Candles.CloseSeries[bar] < Candles.OpenSeries[bar] && Candles.Volume[bar - 1] < Candles.Volume[bar]
                     )
                    {
                        ShortAtMarket(bar + 1, 1, "Short");
                        _time = Candles.DateTimeCandle[bar + 1];
                    }

            }
            else
            {
                if (!Time.ValueBool || Time.ValueBool && _time != Candles.DateTimeCandle[bar + 1])
                    if (LongPos.Count == 0 && ShortPos.Count == 0 && Candles.CloseSeries[bar] > Candles.OpenSeries[bar]
                        && Candles.CloseSeries[bar + 1] > Candles.OpenSeries[bar + 1] && Candles.Volume[bar] < Candles.Volume[bar + 1])
                    {
                        BuyAtMarket(bar + 1, 1, "Long");
                        _time = Candles.DateTimeCandle[bar + 1];
                    }
                    else if (LongPos.Count == 0 && ShortPos.Count == 0 && Candles.CloseSeries[bar] < Candles.OpenSeries[bar]
                        && Candles.CloseSeries[bar + 1] < Candles.OpenSeries[bar + 1] && Candles.Volume[bar] < Candles.Volume[bar + 1])
                    {
                        ShortAtMarket(bar + 1, 1, "Short");
                        _time = Candles.DateTimeCandle[bar + 1];
                    }

            }
        }

        private void Exit(int bar)
        {
            double percent = Math.Abs(Candles.CloseSeries[bar] - Candles.OpenSeries[bar]) * Size.Value / 100;
            double stop = Candles.OpenSeries[bar + 1] - percent;

            if (LongPos.Count > 0 && _time != Candles.DateTimeCandle[bar + 1])
            {
                if (Candles.CloseSeries[bar - 1] > Candles.OpenSeries[bar - 1] && Candles.CloseSeries[bar] > Candles.OpenSeries[bar]
                    && Candles.Volume[bar] < Candles.Volume[bar - 1])
                {
                    SellAtMarket(bar + 1, LongPos[0], "Exit long Volume");
                    _time = Candles.DateTimeCandle[bar + 1];
                }
                else if (
                     (!Time.ValueBool || Time.ValueBool &&
                    _time != Candles.DateTimeCandle[bar + 1]))
                {
                    SellAtStop(bar + 1, LongPos[0], stop, "Exit long Percent " + stop);
                    _time = Candles.DateTimeCandle[bar + 1];
                }
            }
            if (ShortPos.Count > 0 && _time != Candles.DateTimeCandle[bar + 1])
            {
                stop = Candles.OpenSeries[bar + 1] + percent;

                if (Candles.CloseSeries[bar - 1] < Candles.OpenSeries[bar - 1] && Candles.CloseSeries[bar] < Candles.OpenSeries[bar]
                    && Candles.Volume[bar] < Candles.Volume[bar - 1])
                {
                    CoverAtMarket(bar + 1, ShortPos[0], "Exit short Volume");
                    _time = Candles.DateTimeCandle[bar + 1];
                }
                else if (
                     (!Time.ValueBool || Time.ValueBool &&
                    _time != Candles.DateTimeCandle[bar + 1]))
                {
                    CoverAtStop(bar + 1, ShortPos[0], stop, "Exit short Percent " + stop);
                    _time = Candles.DateTimeCandle[bar + 1];
                }
            }
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "3";
            DesParamStratetgy.DateRelease = new DateTime(2021, 08, 02);
            DesParamStratetgy.DateChange = new DateTime(2023, 02, 23);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec367084827";
            DesParamStratetgy.NameStrategy = "VolCandle";
        }
    }
}
