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
    public class ADX50 : Script
    {
        public CreateIndicator Channel = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");
        public CreateIndicator Adx = new CreateIndicator(EnumIndicators.Adx, 1, "");
        public CreateIndicator Rsi = new CreateIndicator(EnumIndicators.Rsi, 2, "");

        public ParamOptimization AdxMain = new ParamOptimization(50, 1, 500, 1, "ADX", "Порог входа в сделку по индикатору ADX");
        public ParamOptimization RsiLongSignal = new ParamOptimization(30, 1, 500, 1, "RsiLongSignal", "Порог входа в лонг по индикатору RSI");
        public ParamOptimization RsiShortSignal = new ParamOptimization(70, 1, 500, 1, "RsiShortSignal", "Порог входа в шорт по индикатору RSI");
        public ParamOptimization Multiplicity = new ParamOptimization(2, 1, 500, 1, "Multiplicity", "Кратность размера тейка относительно стопа");
        public ParamOptimization UseMultipleTake = new ParamOptimization(true, "UseMultipleTake", "Использовать альтернативный выход");

        string _signal = "";
        double _longStopPrice;
        double _shortStopPrice;

        IPosition _long;
        IPosition _short;

        public override void Execute()
        {
            if (CandleCount < 2 || Adx.param.LinesIndicators.Count == 0 || Rsi.param.LinesIndicators.Count == 0 
                || Adx.param.LinesIndicators[0].PriceSeries.Count == 0 || Rsi.param.LinesIndicators[0].PriceSeries.Count == 0)
                return;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                var priceUp = Channel.param.LinesIndicators[0].PriceSeries;
                var priceDown = Channel.param.LinesIndicators[1].PriceSeries;

                
                    OpenPosition(bar, priceDown[bar], priceUp[bar]);
                    ClosePosition(bar);

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

        private void OpenPosition(int bar, double priceDown, double priceUp)
        {
            if (Adx.param.LinesIndicators[0].PriceSeries[bar] > AdxMain.Value && Rsi.param.LinesIndicators[0].PriceSeries[bar - 1] < RsiLongSignal.Value
                && Rsi.param.LinesIndicators[0].PriceSeries[bar] >= RsiLongSignal.Value && LongPos.Count == 0 && ShortPos.Count == 0)
            {
                _long = BuyAtMarket(bar + 1, 1, "Long");
                _longStopPrice = priceDown;
                _signal = "buy";
            }
            else if (Adx.param.LinesIndicators[0].PriceSeries[bar] > AdxMain.Value && Rsi.param.LinesIndicators[0].PriceSeries[bar - 1] > RsiShortSignal.Value
                && Rsi.param.LinesIndicators[0].PriceSeries[bar] <= RsiShortSignal.Value && ShortPos.Count == 0 && LongPos.Count == 0)
            {
                _short = ShortAtMarket(bar + 1, 1, "Short");
                _shortStopPrice = priceUp;
                _signal = "sell";
            }
        }

        private void ClosePosition(int bar)
        {
            if (_signal == "buy" && LongPos.Count > 0)
            {
                double longProfitTarget = _long.EntryPrice + (_long.EntryPrice - _longStopPrice) * Multiplicity.Value;

                SellAtProfit(bar + 1, _long, longProfitTarget, "long take");
                SellAtStop(bar + 1, _long, _longStopPrice, "long stoploss");

                if (UseMultipleTake.ValueBool && Adx.param.LinesIndicators[0].PriceSeries[bar] < AdxMain.Value)
                {
                    SellAtMarket(bar + 1, _long, "long stoploss adx");
                }
            }

            if (_signal == "sell" && ShortPos.Count > 0)
            {
                double shortProfitTarget = _short.EntryPrice - (_shortStopPrice - _short.EntryPrice) * Multiplicity.Value;

                CoverAtProfit(bar + 1, _short, shortProfitTarget, "short take");
                CoverAtStop(bar + 1, _short, _shortStopPrice, "short stoploss");

                if (UseMultipleTake.ValueBool && Adx.param.LinesIndicators[0].PriceSeries[bar] < AdxMain.Value)
                {
                    CoverAtMarket(bar + 1, _short, "short stoploss adx");
                }
            }
        }

        public override void SetSettingDefault()
        {
            Channel.param.LinesIndicators[0].LineParam[0].Value = 14;
            Adx.param.LinesIndicators[0].LineParam[0].Value = 14;
            Rsi.param.LinesIndicators[0].LineParam[0].Value = 14;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 6, 21); 
            DesParamStratetgy.DateChange = new DateTime(2021, 6, 21);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/9rs1ooch21-robot-adx-50"; 
            DesParamStratetgy.NameStrategy = "ADX50";
        }
    }
}
