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
using ScriptSolution.ScanerModel;
using SourceEts.Config;
using SourceEts.Models;


namespace ETSBots
{
    public class TrendNRTR : Script
    {
        public ParamOptimization PercentK = new ParamOptimization(3, 1, 50000, 1, "Процент", "Процент отступа от экстремумов для расчёта индикатора NRTR");
        public ParamOptimization Period = new ParamOptimization(10, 1, 50000, 1, "Период", "Период расчёта экстремумов");

        double _highest;
        double _lowest;

        List<double> _upNrtrSeries = new List<double>();
        List<double> _downNrtrSeries = new List<double>();
        double _nrtr;

        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                while (_upNrtrSeries.Count <= bar + 1)
                {
                    _upNrtrSeries.Add(Double.NaN);
                    _downNrtrSeries.Add(Double.NaN);
                }

                if (_lastBar != bar && bar > Period.ValueInt)
                    NRTRCalculate(bar);
                Drawing();



                ParamDebug("NRTR", _nrtr);

                #region Закрытие позиции по времени, встроенным стопам и рискам
                if (LongPos.Count != 0 || ShortPos.Count != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                }

                SendClosePosOnRiskFromForm(bar + 1, "");

                #endregion
            }
        }
        int _lastBar;

        private void NRTRCalculate(int bar)
        {
            _lastBar = bar;
            ExtremesCalculate(bar);

            if (_highest == 0 || _lowest == 0)
                return;

            double upNrtr = _highest * (1 - (PercentK.Value / 100));
            double downNrtr = _lowest * (1 + (PercentK.Value / 100));

            ParamDebug("High", upNrtr);
            ParamDebug("Lowest", downNrtr);

            if (Candles.LowSeries[bar] < upNrtr && Candles.LowSeries[bar - 1] >= upNrtr)
            {
                if (LongPos.Count > 0)
                    SellAtMarket(bar + 1, LongPos[0], "Close Long Reverse");
                ShortAtMarket(bar + 1, 1, "Short");
            }
            else
            if (Candles.HighSeries[bar] > downNrtr && Candles.HighSeries[bar - 1] <= downNrtr)
            {

                if (ShortPos.Count > 0)
                    CoverAtMarket(bar + 1, ShortPos[0], "Close Short Reverse");
                BuyAtMarket(bar + 1, 1, "Long");
            }

            _upNrtrSeries[_upNrtrSeries.Count - 1] = upNrtr;
            _downNrtrSeries[_downNrtrSeries.Count - 1] = downNrtr;
        }


        private void ExtremesCalculate(int bar)
        {
            if (Candles.HighSeries.Count - 1 <= Period.Value)
                return;

            _highest = Candles.CloseSeries.GetRange(bar + 1 - (int)Period.Value, (int)Period.Value).Max();
            _lowest = Candles.CloseSeries.GetRange(bar + 1 - (int)Period.Value, (int)Period.Value).Min();


        }


        /// <summary>
        /// Рисование линии NRTR
        /// </summary>
        private void Drawing()
        {
            Plotseries("NRTR Up", _upNrtrSeries, new UserChartPropModel() { ColorLine = Color.Blue });
            Plotseries("NRTR Down", _downNrtrSeries, new UserChartPropModel() { ColorLine = Color.Red });
        }




        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 13);
            DesParamStratetgy.DateChange = new DateTime(2022, 12, 01);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/nxj0610tz1-robot-trendnrtr";
            DesParamStratetgy.NameStrategy = "TrendNRTR";
        }
    }
}
