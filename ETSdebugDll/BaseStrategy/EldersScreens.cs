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
    public class EldersScreens : Script
    {
        public CreateIndicator Stochastic = new CreateIndicator(EnumIndicators.Stochastic, 1, "Stochastic");
        public CreateIndicator MA = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Moving Average");
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "ParabolicSar");


        IPosition _long;
        IPosition _short;


        List<double> _stochK;
        List<double> _stochD;
        List<double> _ma;
        List<double> _parabolic;


        string _trend = string.Empty;
        string _signal = string.Empty;


        public override void Execute()
        {
            _parabolic = Sar.param.LinesIndicators[0].PriceSeries;
            _ma = MA.param.LinesIndicators[0].PriceSeries;
            _stochK = Stochastic.param.LinesIndicators[0].PriceSeries;
            _stochD = Stochastic.param.LinesIndicators[1].PriceSeries;


            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                TrendClassify(bar);
                DetectCorrection(bar);
                Entry(bar);
                Exit(bar);

            }
        }

        private void TrendClassify(int bar)
        {
            if (_ma[bar] < Candles.CloseSeries[bar])
                _trend = "up";
            else if (_ma[bar] > Candles.CloseSeries[bar])
                _trend = "down";
            else
                _trend = "";
        }

        private void DetectCorrection(int bar)
        {
            if (CandleCount <= 2)
                return;

            if (_trend == "up" && _stochK[bar - 1] < _stochD[bar - 1] && _stochK[bar] > _stochD[bar] 
                && _stochK[bar - 1] < 20 && _stochK[bar] < 20 && _stochD[bar - 1] < 20 && _stochD[bar] < 20)
                _signal = "buy";
            else if (_trend == "down" && _stochK[bar - 1] > _stochD[bar - 1] && _stochK[bar] < _stochD[bar]
                && _stochK[bar - 1] > 80 && _stochK[bar] > 80 && _stochD[bar - 1] > 80 && _stochD[bar] > 80)
                _signal = "sell";
            else
                _signal = "";
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _signal == "buy" && Candles.LowSeries[bar] > _parabolic[bar])
            {
                 _long = BuyAtMarket(bar + 1, 1, "Long");
            }

            if (LongPos.Count == 0 && ShortPos.Count == 0 && _signal == "sell" && Candles.HighSeries[bar] < _parabolic[bar])
            {
                _short = ShortAtMarket(bar + 1, 1, "Short");
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                SellAtStop(bar + 1, _long, _parabolic[bar], "long stop " + _long.EntryPrice.ToString());
            }
            if (ShortPos.Count > 0)
            {
                CoverAtStop(bar + 1, _short, _parabolic[bar], "short stop " + _short.EntryPrice.ToString());
            }
        }

        public override void SetSettingDefault()
        {
            MA.param.LinesIndicators[0].LineParam[0].Value = 13;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 29);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 29);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/3x2b04e6h1-robot-eldersscreens";
            DesParamStratetgy.NameStrategy = "EldersScreens";
        }
    }
}
