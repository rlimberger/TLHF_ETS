using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Model;
using SourceEts.Models;

namespace ModulSolution.Robots
{
    public class MacdDivergence : Script
    {

        public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");
        public ParamOptimization CountCandle = new ParamOptimization(1, 1, 10, 5, "Свечи для лок. min/max", "количество свечей, которое используется для определения min/max. Например, указав 2, для определения локального минимума, слева и справа от  гистограммы должно быть 2 значения меньше слева и справа.");


        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < (5 + CountCandle.ValueInt))
                    continue;

                SetDivergence(bar, Macd.param.LinesIndicators[2].PriceSeries);

                if (_minDiver)
                {
                    CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                    BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                }

                if (_maxDiver)
                {
                    SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
                    ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                }

                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                    SendClosePosOnRiskFromForm(bar + 1, "");
                }
            }
        }


        private int _barPrevLocalMax;
        private int _barLocalMax;
        private int _barPrevLocalMin;
        private int _barLocalMin;
        private bool _maxDiver;
        private bool _minDiver;

        /// <summary>
        /// Определение дивергенции
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="series">серия данных по которым ищатся последние пики</param>
        private void SetDivergence(int bar, List<double> series)
        {
            bool localMax = true;
            bool localMin = true;

            for (int i = 0; i < CountCandle.ValueInt; i++)
            {
                if (series[bar - CountCandle.ValueInt] > 0
                    && series[bar - CountCandle.ValueInt - 1 - i] < series[bar - CountCandle.ValueInt]
                    && series[bar - CountCandle.ValueInt] > series[bar - i])
                {
                }
                else
                {
                    localMax = false;
                }

                if (series[bar - CountCandle.ValueInt] < 0
                    && series[bar - CountCandle.ValueInt - 1 - i] > series[bar - CountCandle.ValueInt]
                    && series[bar - CountCandle.ValueInt] < series[bar - i])
                {
                }
                else
                {
                    localMin = false;
                }
            }
            if (localMax)
            {
                _barPrevLocalMax = _barLocalMax;
                _barLocalMax = bar - CountCandle.ValueInt;
            }

            if (localMin)
            {
                _barPrevLocalMin = _barLocalMin;
                _barLocalMin = bar - CountCandle.ValueInt;
            }

            _maxDiver = false;
            if (_barLocalMax == bar - CountCandle.ValueInt)
                if (series[_barPrevLocalMax] > series[_barLocalMax] &&
                    Candles.HighSeries[_barPrevLocalMax] < Candles.HighSeries[_barLocalMax])
                    _maxDiver = true;

            _minDiver = false;
            if (_barLocalMin == bar - CountCandle.ValueInt)
                if (series[_barPrevLocalMin] < series[_barLocalMin]
                    && Candles.LowSeries[_barPrevLocalMin] > Candles.LowSeries[_barLocalMin])
                    _minDiver = true;

            #region Вывод данных по свечам для дивергенции
            ParamDebug("Бар предыд. локал. max", _barPrevLocalMax);
            ParamDebug("Бар посл. локал. max", _barLocalMax);
            ParamDebug("Бар предыд. локал. min", _barPrevLocalMin);
            ParamDebug("Бар посл. локал. min", _barLocalMin);
            #endregion

            if (_maxDiver)
                if (_barPrevLocalMax > 0 && _barLocalMax > 0)
                {
                    for (int i = _barPrevLocalMax; i < _barLocalMax; i++)
                    {
                        PlotArea("шорт", i, new UserChartPropModel { ColorLine = Color.LightPink });
                    }
                }

            if (_minDiver)
                if (_barPrevLocalMin > 0 && _barLocalMin > 0)
                {
                    for (int i = _barPrevLocalMin; i < _barLocalMin; i++)
                    {
                        PlotArea("Лонг", i, new UserChartPropModel { ColorLine = Color.LightGreen });
                    }
                }
        }



        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2017, 03, 03);
            DesParamStratetgy.DateChange = new DateTime(2017, 04, 15);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/h25f6ar0u1-robot-macddivergence";
            DesParamStratetgy.NameStrategy = "MacdDivergence";

        }

    }
}
