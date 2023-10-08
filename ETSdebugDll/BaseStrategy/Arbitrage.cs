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
using SourceEts;
using SourceEts.Config;
using SourceEts.Models;

namespace ETSBots
{
    public class Arbitrage : Script
    {
        public ParamOptimization Instr = new ParamOptimization(EnumTypeGetIEnumerable.GetSeccodeList, "Второй инструмент");
        public ParamOptimization Period = new ParamOptimization(20, 1, 50000, 1, "Period", "");
        public ParamOptimization StdvPeriod = new ParamOptimization(20, 1, 50000, 1, "", "");
        public ParamOptimization Instr1_vol = new ParamOptimization(1, 1, 500, 1, "Объём сделки", "Объём первой сделки");
        public ParamOptimization Instr2_vol = new ParamOptimization(1, 1, 500, 1, "Объём сделки", "Объём первой сделки");


        IPosition _long;
        IPosition _short;
        IPosition _long2;
        IPosition _short2;

        string signal = "";

        double _spread;


        List<double> _sprd = new List<double>();
        List<double> _sprdMa = new List<double>();
        List<double> _dlta = new List<double>();
        List<double> _stdvP = new List<double>();
        List<double> _stdvM = new List<double>();


        DateTime time = DateTime.MinValue;
        DateTime time2 = DateTime.MinValue;

        public override void Execute()
        {
            


            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                Calculations(bar);
                Stdv(bar);

                Entry(bar);
                Exit(bar);


                Drawing();

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

        private void Calculations(int bar)
        {
            if (Candles.CloseSeries[bar] > 0 && Instr.FinInfo.Candles.CloseSeries[bar] > 0)
            {
                _spread = Math.Log(Candles.CloseSeries[bar] / Instr.FinInfo.Candles.CloseSeries[bar]);
            }
            else
            {
                _spread = _sprd.Count > 0 ? _sprd.Last() : 0;
            }

            if (time != Candles.DateTimeCandle[bar])
            {
                _sprd.Add(_spread);
                time = Candles.DateTimeCandle[bar];
            }
            

            Sma(_sprd, (int)Period.Value, _sprdMa);

            _dlta.Clear();

            for (int i = 0; i < _sprd.Count && i < _sprdMa.Count; i++)
            {
                _dlta.Add(_sprd[i] - _sprdMa[i]);
            }
        }

        private void Stdv(int bar)
        {
            List<double> avg = new List<double>();

            Sma(_dlta, (int)StdvPeriod.Value, avg);

            List<double> resault = new List<double>();

            for (int i = 0; i < _dlta.Count; i++)
            {
                resault.Add(Math.Pow(_dlta[i] - avg[i], 2));
            }

            //_stdvP.Clear();
            //_stdvM.Clear();

            if (resault.Count == 0)
                return;

            var stdv = Math.Sqrt(resault.Sum() / (resault.Count - 1));

            if (Candles.DateTimeCandle[bar] != time2)
            {
                _stdvP.Add(stdv);
                _stdvM.Add(-1 * stdv);
                time2 = Candles.DateTimeCandle[bar];
            }
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _dlta[bar] > _stdvP[bar])
            {
                _short = ShortAtMarket(bar + 1, Instr1_vol.Value, "Short Instr1");
                _long2 = BuyAtMarket(Instr.FinInfo, bar + 1, Instr2_vol.Value, "Long Instr1");
                signal = "sell";
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _dlta[bar] < _stdvM[bar])
            {
                _long = BuyAtMarket(bar + 1, Instr1_vol.Value, "Long Instr1");
                _short2 = ShortAtMarket(Instr.FinInfo, bar + 1, Instr2_vol.Value, "Short Instr2");
                signal = "buy";
            }
        }

        private void Exit(int bar)
        {
            if (ShortPos.Count > 0 && signal == "sell" && _dlta[bar] <= 0)
            {
                CoverAtClose(bar + 1, _short, "Close Instr1");
                SellAtClose(bar + 1, _long2, "Close Instr2");
                signal = "";
            }
            if (LongPos.Count > 0 && signal == "buy" && _dlta[bar] >= 0)
            {
                SellAtClose(bar + 1, _long, "Close Instr1");
                CoverAtClose(bar + 1, _short2, "Close Instr2");
                signal = "";
            }
        }

        private void Drawing()
        {
            Plotseries("StdvP", _stdvP, new UserChartPropModel()
            {
                ColorLine = Color.OrangeRed,
                Width = 2,
                NumberPanel = 2,
                TypeLine =EnumTypeLine.Line,
                Transparency = 100
            });
            Plotseries("StdvM", _stdvM, new UserChartPropModel()
            {
                ColorLine = Color.OrangeRed,
                Width = 2,
                NumberPanel = 2,
                TypeLine =EnumTypeLine.Line,
                Transparency = 100
            });
            Plotseries("Delta", _dlta, new UserChartPropModel()
            {
                ColorLine = Color.Purple,
                NumberPanel = 2,
                TypeLine =EnumTypeLine.Line,
                Transparency = 100
            });
            PlotCanldes("SecondPrice", Instr.FinInfo, new UserChartPropModel()
            {
                ColorLine = Color.Green,
                NumberPanel = 1,
                TypeLine =EnumTypeLine.Line,
                Transparency = 100
            });
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 7, 14);
            DesParamStratetgy.DateChange = new DateTime(2021, 7, 14);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Arbitrage;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/x5nlsbvd81-robot-arbitrazh";
            DesParamStratetgy.NameStrategy = "Arbitrage";
        }
    }
}
