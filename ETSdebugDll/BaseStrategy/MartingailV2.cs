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
    public class MartingailV2 : Script
    {
        public ParamOptimization Quantity = new ParamOptimization(1, 1, 500, 1, "Объём сделки", "Объём первой сделки");
        public ParamOptimization Steps = new ParamOptimization(3, 1, 500, 1, "Колличество шагов", "Колличество сделок на повышение лота");
        public ParamOptimization Take = new ParamOptimization(10, 1, 500, 1, "Тейк профит", "");
        public ParamOptimization Stop = new ParamOptimization(10, 1, 500, 1, "Стоп лосс", "");


        IPosition _long;
        IPosition _short;


        DateTime _prevBar = DateTime.MinValue;

        bool _buy = false;
        bool _trade = false;

        int _deals = 0;

        double _quantity;



        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                TradeSignal(bar);

                ParamDebug("Signal", _trade);

                Entry(bar);

                ParamDebug("Deals", _deals);
                ParamDebug("Quantity", _quantity);

                Exit(bar);

                ParamDebug("Buy", _buy);
            }
        }

        private void TradeSignal(int bar)
        {
            if (Candles.DateTimeCandle[bar] != _prevBar)
            {
                _prevBar = Candles.DateTimeCandle[bar];
                _trade = true;
            }
        }

        private void Entry(int bar)
        {
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _trade && !_buy && _deals < Steps.Value)
            {
                if (_deals == 0)
                {
                    _deals++;
                    _long = BuyAtMarket(bar + 1, Quantity.Value, "Long " + _deals.ToString());
                    _trade = false;
                    _quantity = Quantity.Value;
                    _buy = true;
                }
                else if (_deals >= 1 && _deals < Steps.Value && Steps.Value - _deals > 1)
                {
                    _deals++;
                    _quantity = _quantity * 2;
                    _long = BuyAtMarket(bar + 1, _quantity, "Long " + _deals.ToString());
                    _trade = false;
                    _buy = true;
                }
                else if (Steps.Value - _deals == 1)
                {
                    _deals++;
                    _quantity = _quantity * 2;
                    _long = BuyAtMarket(bar + 1, _quantity, "Long " + _deals.ToString());
                    _trade = false;
                    _buy = true;
                }
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _trade && _buy && _deals < Steps.Value)
            {
                if (_deals == 0)
                {
                    _deals++;
                    _short = ShortAtMarket(bar + 1, Quantity.Value, "Short " + _deals.ToString());
                    _trade = false;
                    _quantity = Quantity.Value;
                    _buy = false;
                }
                else if (_deals >= 1 && _deals < Steps.Value && Steps.Value - _deals > 1)
                {
                    _deals++;
                    _quantity = _quantity * 2;
                    _short = ShortAtMarket(bar + 1, _quantity, "Short " + _deals.ToString());
                    _trade = false;
                    _buy = false;
                }
                else if (Steps.Value - _deals == 1)
                {
                    _deals++;
                    _quantity = _quantity * 2;
                    _short = ShortAtMarket(bar + 1, _quantity, "Short " + _deals.ToString());
                    _trade = false;
                    _buy = false;
                }
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                SellAtProfit(bar + 1, _long, _long.EntryPrice + Take.Value * FinInfo.Security.MinStep, "long take");
                SellAtStop(bar + 1, _long, _long.EntryPrice - Stop.Value * FinInfo.Security.MinStep, "long stop");
            }
            if (ShortPos.Count > 0)
            {
                CoverAtProfit(bar + 1, _short, _short.EntryPrice - Take.Value * FinInfo.Security.MinStep, "short take");
                CoverAtStop(bar + 1, _short, _short.EntryPrice + Stop.Value * FinInfo.Security.MinStep, "short stop");
            }

            if (_buy && _long != null && _long.ExitOrderStatus == EnumOrderStatus.Performed && _long.ExitPrice > _long.EntryPrice
                && _deals == Steps.Value)
            {
                _trade = false;
                _deals = 0;
                _quantity = 0;
            }
            else if (!_buy && _short != null && _short.ExitOrderStatus == EnumOrderStatus.Performed && _short.ExitPrice < _short.EntryPrice
                && _deals == Steps.Value)
            {
                _trade = false;
                _deals = 0;
                _quantity = 0;
            }
            else if (_buy && _long != null && _long.ExitOrderStatus == EnumOrderStatus.Performed && _long.ExitPrice < _long.EntryPrice
                )
            {
                _trade = false;
                _deals = 0;
                _quantity = 0;
            }
            else if (!_buy && _short != null && _short.ExitOrderStatus == EnumOrderStatus.Performed && _short.ExitPrice > _short.EntryPrice
                )
            {
                _trade = false;
                _deals = 0;
                _quantity = 0;
            }
        }


        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 12);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 12);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357372473";
            DesParamStratetgy.NameStrategy = "MartingailV2";
        }
    }
}
