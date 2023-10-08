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
    public class StochasticAdxMa : Script
    {
        public CreateIndicator Channel = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");
        public CreateIndicator Adx = new CreateIndicator(EnumIndicators.Adx, 1, "");
        public CreateIndicator Stochastic = new CreateIndicator(EnumIndicators.Stochastic, 2, "");
        public CreateIndicator MAfast = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Fast");
        public CreateIndicator MAmiddle = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Middle");
        public CreateIndicator MAslow = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Slow");


        public ParamOptimization Multiplicity = new ParamOptimization(2, 1, 500, 1, "Multiplicity", "");
        public ParamOptimization AlternateEntry = new ParamOptimization(true, "AlternateEntry", "Альтернативный вход");


        IPosition _long;
        IPosition _short;
        double _longStopPrice;
        double _shortStopPrice;


        public override void Execute()
        {
            if (CandleCount < 20)
                return;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                var priceUp = Channel.param.LinesIndicators[0].PriceSeries;
                var priceDown = Channel.param.LinesIndicators[1].PriceSeries;
                var stochastic = Stochastic.param.LinesIndicators[0].PriceSeries;
                var adx = Adx.param.LinesIndicators[0].PriceSeries;
                var smaFast = MAfast.param.LinesIndicators[0].PriceSeries;
                var smaMiddle = MAmiddle.param.LinesIndicators[0].PriceSeries;
                var smaSlow = MAslow.param.LinesIndicators[0].PriceSeries;

                if (!AlternateEntry.ValueBool)
                {
                    if (stochastic[bar - 1] < 50 && stochastic[bar] > 50 && adx[bar] > 20 && smaFast[bar] > smaMiddle[bar] && smaMiddle[bar] > smaSlow[bar]
                    && LongPos.Count == 0 && ShortPos.Count == 0)
                    {
                        _long = BuyAtMarket(bar + 1, 1, "Long");
                        _longStopPrice = priceDown[bar];
                    }
                    if (stochastic[bar - 1] > 50 && stochastic[bar] < 50 && adx[bar] > 20 && smaFast[bar] < smaMiddle[bar] && smaMiddle[bar] < smaSlow[bar]
                        && LongPos.Count == 0 && ShortPos.Count == 0)
                    {
                        _short = ShortAtMarket(bar + 1, 1, "Short");
                        _shortStopPrice = priceUp[bar];
                    }
                    if (LongPos.Count > 0)
                    {
                        double longProfitTarget = _long.EntryPrice + (_long.EntryPrice - _longStopPrice) * Multiplicity.Value;
                        SellAtProfit(bar + 1, _long, longProfitTarget, "long take");
                        SellAtStop(bar + 1, _long, _longStopPrice, "long stoploss");
                    }
                    if (ShortPos.Count > 0)
                    {
                        double shortProfitTarget = _short.EntryPrice - (_shortStopPrice - _short.EntryPrice) * Multiplicity.Value;
                        CoverAtProfit(bar + 1, _short, shortProfitTarget, "short take");
                        CoverAtStop(bar + 1, _short, _shortStopPrice, "short stoploss");
                    }
                }
                else
                {
                    if (stochastic[bar] > 50 && adx[bar] > 20 && smaFast[bar] > smaMiddle[bar] && smaMiddle[bar] > smaSlow[bar]
                    && LongPos.Count == 0 && ShortPos.Count == 0)
                    {
                        _long = BuyAtMarket(bar + 1, 1, "Long");
                        _longStopPrice = priceDown[bar];
                    }
                    if (stochastic[bar] < 50 && adx[bar] > 20 && smaFast[bar] < smaMiddle[bar] && smaMiddle[bar] < smaSlow[bar]
                        && LongPos.Count == 0 && ShortPos.Count == 0)
                    {
                        _short = ShortAtMarket(bar + 1, 1, "Short");
                        _shortStopPrice = priceUp[bar];
                    }
                    if (LongPos.Count > 0)
                    {
                        double longProfitTarget = _long.EntryPrice + (_long.EntryPrice - _longStopPrice) * Multiplicity.Value;
                        SellAtProfit(bar + 1, _long, longProfitTarget, "long take");
                        SellAtStop(bar + 1, _long, _longStopPrice, "long stoploss");
                    }
                    if (ShortPos.Count > 0)
                    {
                        double shortProfitTarget = _short.EntryPrice - (_shortStopPrice - _short.EntryPrice) * Multiplicity.Value;
                        CoverAtProfit(bar + 1, _short, shortProfitTarget, "short take");
                        CoverAtStop(bar + 1, _short, _shortStopPrice, "short stoploss");
                    }
                }

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

        public override void SetSettingDefault()
        {
            MAfast.param.LinesIndicators[0].LineParam[0].Value = 5;
            MAmiddle.param.LinesIndicators[0].LineParam[0].Value = 15;
            MAslow.param.LinesIndicators[0].LineParam[0].Value = 30;
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
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/da0zrd6fm1-robot-stochastic-adx-ma";
            DesParamStratetgy.NameStrategy = "StochasticAdxMa";

        }
    }
}
