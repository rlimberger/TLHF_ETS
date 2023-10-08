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
    public class MacdEma : Script
    {
        public CreateIndicator Ma = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "");
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");
        public CreateIndicator Macd = new CreateIndicator(EnumIndicators.Macd, 1, "");
        public CreateIndicator Ao = new CreateIndicator(EnumIndicators.Ao, 2, "");

        public ParamOptimization Multiplicity = new ParamOptimization(2, 1, 500, 1, "Множитель", "Кратность размера тейка относительно стопа");

        IPosition _long;
        IPosition _short;


        List<double> _macdB;
        List<double> _macdG;
        List<double> _ao;
        List<double> _parabolic;
        List<double> _ma;

        double _longStopLoss;
        double _shortStopLoss;

        public override void Execute()
        {
            if (CandleCount <= 2)
                return;


            _ma = Ma.param.LinesIndicators[0].PriceSeries;
            _macdB = Macd.param.LinesIndicators[0].PriceSeries;
            _macdG = Macd.param.LinesIndicators[1].PriceSeries;
            _ao = Ao.param.LinesIndicators[0].PriceSeries;
            _parabolic = Sar.param.LinesIndicators[0].PriceSeries;


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
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _macdB[bar] > _macdG[bar] && _macdB[bar - 1] < 0
                && _macdB[bar] < 0 && _ao[bar - 1] < 0 && _ao[bar] > 0 && _parabolic[bar] < Candles.LowSeries[bar] && Candles.CloseSeries[bar] > _ma[bar])
            {
                _long = BuyAtMarket(bar + 1, 1, "Long");
                _longStopLoss = _parabolic[bar];
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _macdB[bar] < _macdG[bar] && _macdB[bar - 1] > 0
                && _macdB[bar] > 0 && _ao[bar - 1] > 0 && _ao[bar] < 0 && _parabolic[bar] > Candles.HighSeries[bar] && Candles.CloseSeries[bar] < _ma[bar])
            {
                _short = ShortAtMarket(bar + 1, 1, "Short");
                _shortStopLoss = _parabolic[bar];
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                double longProfitTarget = _long.EntryPrice + (_long.EntryPrice - _longStopLoss) * Multiplicity.Value;
                SellAtProfit(bar + 1, _long, longProfitTarget, "long take");
                SellAtStop(bar + 1, _long, _longStopLoss, "long stop");
            }
            if (ShortPos.Count > 0)
            {
                double shortProfitTarget = _short.EntryPrice - (_shortStopLoss - _short.EntryPrice) * Multiplicity.Value;
                CoverAtProfit(bar + 1, _short, shortProfitTarget, "short take");
                CoverAtStop(bar + 1, _short, _shortStopLoss, "short stop");
            }
        }

        public override void SetSettingDefault()
        {
            Macd.param.LinesIndicators[0].LineParam[0].Value = 34;
            Macd.param.LinesIndicators[0].LineParam[1].Value = 89;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 06, 24);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 24);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/bbun31cl61-robot-macdema";
            DesParamStratetgy.NameStrategy = "MacdEma";

        }
    }
}
