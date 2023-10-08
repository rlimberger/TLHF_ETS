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

namespace ETSBots
{
    public class WilliamsSar : Script
    {
        public CreateIndicator Parabolic = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");
        public CreateIndicator Williams = new CreateIndicator(EnumIndicators.WilliamsPercentsR, 1, "");

        public ParamOptimization WilliamsTrigger = new ParamOptimization(-50, 1, 500, 1, "WilliamsTrigger", "Williams entry trigger");

        List<double> _parabolic;
        List<double> _prevParabolic;
        List<double> _williams;

        IPosition _long;
        IPosition _short;

        public override void Execute()
        {
            _parabolic = Parabolic.param.LinesIndicators[0].PriceSeries;
            _prevParabolic = Parabolic.param.LinesIndicators[1].PriceSeries;
            _williams = Williams.param.LinesIndicators[0].PriceSeries;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
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
            if (CandleCount <= 2)
                return;

            if (LongPos.Count == 0 && ShortPos.Count == 0 && _williams[bar] > WilliamsTrigger.Value
                && _prevParabolic[bar] > 0 && _prevParabolic[bar] < Candles.HighSeries[bar] && _parabolic[bar] < Candles.LowSeries[bar])
            {
                if (_long != null && _long.ExitBar == bar + 1)
                    return;

                _long = BuyAtMarket(bar + 1, 1, "Long");
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _williams[bar] < WilliamsTrigger.Value
                && _prevParabolic[bar] > 0 && _prevParabolic[bar] > Candles.LowSeries[bar] && _parabolic[bar] > Candles.HighSeries[bar])
            {
                if (_short != null && _short.ExitBar == bar + 1)
                    return;

                _short = ShortAtMarket(bar + 1, 1, "Short");
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                SellAtStop(bar + 1, _long, _parabolic[bar], "long stop");
            }
            if (ShortPos.Count > 0)
            {
                CoverAtStop(bar + 1, _short, _parabolic[bar], "short stop");
            }
        }


        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 06, 23);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 23);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/8c425j7mm1-robot-williamssar";
            DesParamStratetgy.NameStrategy = "WilliamsSar";

        }
    }
}
