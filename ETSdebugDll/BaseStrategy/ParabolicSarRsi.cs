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
    public class ParabolicSarRsi : Script
    {
        public CreateIndicator Channel = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");
        public CreateIndicator Sar = new CreateIndicator(EnumIndicators.ParabolicSar, 0, "");
        public CreateIndicator Rsi = new CreateIndicator(EnumIndicators.Rsi, 1, "");

        public ParamOptimization RsiLong = new ParamOptimization(65, 1, 500, 1, "Rsi", "Порог входа в лонг по индикатору Rsi");
        public ParamOptimization RsiShort = new ParamOptimization(35, 1, 500, 1, "Rsi", "Порог входа в шорт по индикатору Rsi");
        public ParamOptimization Multiplicity = new ParamOptimization(2, 1, 500, 1, "Multiplicity", "Кратность размера тейка относительно стопа");


        List<double> _parabolic;
        List<double> _prevParabolic;
        List<double> _rsi;
        List<double> _channelUp;
        List<double> _channelDown;

        IPosition _long;
        IPosition _short;

        double _longStopLoss;
        double _shortStopLoss;


        public override void Execute()
        {
            if (CandleCount <= 2)
                return;


            _parabolic = Sar.param.LinesIndicators[0].PriceSeries;
            _prevParabolic = Sar.param.LinesIndicators[1].PriceSeries;
            _rsi = Rsi.param.LinesIndicators[0].PriceSeries;
            _channelUp = Channel.param.LinesIndicators[0].PriceSeries;
            _channelDown = Channel.param.LinesIndicators[1].PriceSeries;


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
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _parabolic[bar] < Candles.LowSeries[bar] 
                && Candles.CloseSeries[bar] > Candles.HighSeries[bar - 1] && _rsi[bar] < RsiLong.Value)
            {
                if (_long != null && _long.ExitBar == bar + 1)
                    return;

                _long = BuyAtMarket(bar + 1, 1, "Long");
                _longStopLoss = _channelDown[bar + 1];
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _parabolic[bar] > Candles.HighSeries[bar]
                && Candles.CloseSeries[bar] < Candles.LowSeries[bar - 1] && _rsi[bar] > RsiShort.Value)
            {
                if (_short != null && _short.ExitBar == bar + 1)
                    return;

                _short = ShortAtMarket(bar + 1, 1, "Short");
                _shortStopLoss = _channelUp[bar + 1];
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                double longProfitTarget = _long.EntryPrice + (_long.EntryPrice - _longStopLoss) * Multiplicity.Value;

                SellAtProfit(bar + 1, _long, longProfitTarget, "long take");
                SellAtStop(bar + 1, _long, _longStopLoss, "long stoploss");
            }
            if (ShortPos.Count > 0)
            {
                double shortProfitTarget = _short.EntryPrice - (_shortStopLoss - _short.EntryPrice) * Multiplicity.Value;

                CoverAtProfit(bar + 1, _short, shortProfitTarget, "short take");
                CoverAtStop(bar + 1, _short, _shortStopLoss, "short stoploss");
            }
        }



        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2023, 06, 23);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 23);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357404524";
            DesParamStratetgy.NameStrategy = "ParabolicSarRsi";
        }
    }
}
