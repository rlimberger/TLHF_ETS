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
    public class DoubleChannel : Script
    {
        public CreateIndicator Channel = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");
        public CreateIndicator MaFast = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Ma fast");
        public CreateIndicator MaSlow = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Ma slow");
        public CreateIndicator Stochastic = new CreateIndicator(EnumIndicators.Stochastic, 1, "");
        public CreateIndicator Rsi = new CreateIndicator(EnumIndicators.Rsi, 2, "");

        public ParamOptimization StocOverBought = new ParamOptimization(80, 1, 5000, 1, "Stochastic", "Порог входа в лонг по значению индикатора Stochastic");
        public ParamOptimization StocOverSold = new ParamOptimization(20, 1, 5000, 1, "Stochastic", "Порог входа в шорт по значению индикатора Stochastic");
        public ParamOptimization RsiTrigger = new ParamOptimization(50, 1, 500, 1, "Rsi", "Порог входа по значению индикатора Rsi");

        IPosition _long;
        IPosition _short;

        List<double> _maF;
        List<double> _maS;
        List<double> _stochasticK;
        List<double> _stochasticD;
        List<double> _rsi;
        List<double> _channelUp;
        List<double> _channelDown;


        double _longStopLoss;
        double _shortStopLoss;
        double _longTakeProfit;
        double _shortTakeProfit;

        public override void Execute()
        {
            if (CandleCount <= 2)
                return;

            _maF = MaFast.param.LinesIndicators[0].PriceSeries;
            _maS = MaSlow.param.LinesIndicators[0].PriceSeries;
            _stochasticK = Stochastic.param.LinesIndicators[0].PriceSeries;
            _stochasticD = Stochastic.param.LinesIndicators[1].PriceSeries;
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
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _maF[bar - 1] < _maS[bar - 1] && _maF[bar] > _maS[bar] 
                && _stochasticK[bar] > _stochasticD[bar] && _stochasticK[bar - 1] < _stochasticK[bar]
                && _stochasticK[bar] < StocOverBought.Value && _rsi[bar] > RsiTrigger.Value)
            {

                _long = BuyAtMarket(bar + 1, 1, "Long");
                _longTakeProfit = _channelUp[bar + 1];
                _longStopLoss = _channelDown[bar + 1];
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _maF[bar - 1] > _maS[bar - 1] && _maF[bar] < _maS[bar]
                && _stochasticK[bar] < _stochasticD[bar] && _stochasticD[bar - 1] > _stochasticD[bar] 
                && _stochasticK[bar] > StocOverSold.Value && _rsi[bar] < RsiTrigger.Value)
            {
                _short = ShortAtMarket(bar + 1, 1, "Short");
                _shortTakeProfit = _channelDown[bar + 1];
                _shortStopLoss = _channelUp[bar + 1];
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0)
            {
                SellAtProfit(bar + 1, _long, _longTakeProfit, "long takeprofit");
                SellAtStop(bar + 1, _long, _longStopLoss, "long stoploss");
            }
            if (ShortPos.Count > 0)
            {
                CoverAtProfit(bar + 1, _short, _shortTakeProfit, "short takeprofit");
                CoverAtStop(bar + 1, _short, _shortStopLoss, "short stoploss");
            }
        }

        public override void SetSettingDefault()
        {
            MaFast.param.LinesIndicators[0].LineParam[0].Value = 5;
            MaSlow.param.LinesIndicators[0].LineParam[0].Value = 10;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 06, 22);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 22);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/t4gn4mpyr1-robot-doublechannel";
            DesParamStratetgy.NameStrategy = "DoubleChannel";
        }

    
    }
}
