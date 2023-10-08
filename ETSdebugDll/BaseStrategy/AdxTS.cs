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
    public class AdxTS : Script
    {
        public CreateIndicator MA = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "");
        public CreateIndicator Adx = new CreateIndicator(EnumIndicators.Adx, 1, "");

        public ParamOptimization AdxMain = new ParamOptimization(20, 1, 500, 1, "ADX", "Порог входа в сделку по основной линии ADX");
        public ParamOptimization DIp = new ParamOptimization(20, 1, 500, 1, "ADX DI+", "Порог входа в сделку по DI+ линии ADX");
        public ParamOptimization DIm = new ParamOptimization(20, 1, 500, 1, "ADX DI-", "Порог входа в сделку по DI- линии ADX");
        public ParamOptimization UseMultipleTake = new ParamOptimization(true, "UseMultipleTake", "Альтернативный выход, Multiplicity * величина стоплосса");
        public ParamOptimization Multiplicity = new ParamOptimization(2, 1, 500, 1, "Multiplicity", "Кратность размера тейка относительно стопа");

        IPosition _long;
        IPosition _short;

        string _signal = "";
        double _prevPrice;
        double _price;
        double _stopPrice;


        public override void Execute()
        {
            if (IndexBar < 1)
                return;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                ClosePrice(bar);
                ClosePosition(bar);

                if (EntryCandleDetection(bar) == "buy" && Candles.CloseSeries[bar] > MA.param.LinesIndicators[0].PriceSeries[bar]
                    && Adx.param.LinesIndicators[0].PriceSeries[bar] > AdxMain.Value 
                    && Adx.param.LinesIndicators[1].PriceSeries[bar] > DIp.Value 
                    && Adx.param.LinesIndicators[1].PriceSeries[bar] > Adx.param.LinesIndicators[2].PriceSeries[bar] 
                    && Adx.param.LinesIndicators[2].PriceSeries[bar] < 20
                    && LongPos.Count == 0 && ShortPos.Count == 0)
                {
                    _long = BuyAtMarket(bar + 1, 1, "Long");
                    _signal = "bull";
                    _price = _prevPrice;
                    _stopPrice = Candles.LowSeries[bar];
                }
                else if (EntryCandleDetection(bar) == "sell" && Candles.CloseSeries[bar] < MA.param.LinesIndicators[0].PriceSeries[bar]
                    && Adx.param.LinesIndicators[0].PriceSeries[bar] > AdxMain.Value 
                    && Adx.param.LinesIndicators[2].PriceSeries[bar] > DIm.Value 
                    && Adx.param.LinesIndicators[2].PriceSeries[bar] > Adx.param.LinesIndicators[1].PriceSeries[bar] 
                    && Adx.param.LinesIndicators[1].PriceSeries[bar] < 20
                    && LongPos.Count == 0 && ShortPos.Count == 0)
                {
                    _short = ShortAtMarket(bar + 1, 1, "Short");
                    _signal = "bear";
                    _price = _prevPrice;
                    _stopPrice = Candles.HighSeries[bar];
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

        private string EntryCandleDetection(int bar)
        {
            if (Candles.OpenSeries[bar] < Candles.CloseSeries[bar] || Candles.OpenSeries[bar] == Candles.CloseSeries[bar])
            {
                double upTail = Candles.HighSeries[bar] - Candles.CloseSeries[bar];
                double downTail = Candles.OpenSeries[bar] - Candles.LowSeries[bar];
                double bodySize = Candles.CloseSeries[bar] - Candles.OpenSeries[bar];

                if (downTail > bodySize * 3 && upTail <= downTail / 2)
                    return "buy";
                else
                    return "flat";
            }
            if (Candles.OpenSeries[bar] > Candles.CloseSeries[bar] || Candles.OpenSeries[bar] == Candles.CloseSeries[bar])
            {
                double upTail = Candles.HighSeries[bar] - Candles.OpenSeries[bar];
                double downTail = Candles.CloseSeries[bar] - Candles.LowSeries[bar];
                double bodySize = Candles.OpenSeries[bar] - Candles.CloseSeries[bar];

                if (upTail > bodySize * 3 && downTail <= upTail / 2)
                    return "sell";
                else
                    return "flat";
            }
            return "flat";
        }

        /// <summary>
        /// Определение цены закрытия предыдущего дня
        /// </summary>
        /// <param name="bar"></param>
        private void ClosePrice(int bar)
        {
            if (Candles.DateTimeCandle[bar].Day != Candles.DateTimeCandle[bar + 1].Day)
            {
                _prevPrice = Candles.CloseSeries[bar];
            }
        }

        private void ClosePosition(int bar)
        {
            if (UseMultipleTake.ValueBool)
            {
                double longProfitTarget = _long != null ? _long.EntryPrice + (_long.EntryPrice - _stopPrice) * Multiplicity.Value : 0;
                double shortProfitTarget = _short != null ? _short.EntryPrice - (_stopPrice - _short.EntryPrice) * Multiplicity.Value : 0;

                if (_signal == "bull" && LongPos.Count > 0)
                {
                    SellAtProfit(bar + 1, _long, longProfitTarget, "long take");
                }
                if (_signal == "bear" && ShortPos.Count > 0)
                {
                    CoverAtProfit(bar + 1, _short, shortProfitTarget, "short take");
                }
            }

            if (_signal == "bull" && LongPos.Count > 0 && _price > 0 && _prevPrice > 0 && _price != _prevPrice)
            {
                SellAtMarket(bar + 1, _long, "long close");
            }
            if (_signal == "bear" && ShortPos.Count > 0 && _price > 0 && _prevPrice > 0 && _price != _prevPrice)
            {
                CoverAtMarket(bar + 1, _short, "short close");
            }
            if (_signal == "bull" && LongPos.Count > 0)
            {
                SellAtStop(bar + 1, _long, _stopPrice, "long stoploss");
            }
            if (_signal == "bear" && ShortPos.Count > 0)
            {
                CoverAtStop(bar + 1, _short, _stopPrice, "short stoploss");
            }
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
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec355805412";
            DesParamStratetgy.NameStrategy = "AdxTS";

        }
    }
}
