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
    public class CCIStochMa : Script
    {
        public CreateIndicator Channel = new CreateIndicator(EnumIndicators.PriceChannel, 0, "");
        public CreateIndicator MaFast = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Ma fast");
        public CreateIndicator MaSlow = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Ma slow");
        public CreateIndicator Stochastic = new CreateIndicator(EnumIndicators.Stochastic, 1, "");
        public CreateIndicator Cci = new CreateIndicator(EnumIndicators.Cci, 2, "");


        public ParamOptimization StochLong = new ParamOptimization(20, 1, 500, 1, "ADX", "Порог входа в лонг по индикатору Stochastic");
        public ParamOptimization StochShort = new ParamOptimization(80, 1, 500, 1, "ADX", "Порог входа в шорт по индикатору Stochastic");
        public ParamOptimization CciLong = new ParamOptimization(-150, 1, 50000, 1, "ADX", "Порог входа в лонг по индикатору Cci");
        public ParamOptimization CciShort = new ParamOptimization(150, 1, 50000, 1, "ADX", "Порог входа в шорт по индикатору Cci");


        IPosition _long;
        IPosition _short;

        List<double> _maF;
        List<double> _maS;
        List<double> _stochastic;
        List<double> _cci;
        List<double> _channelUp;
        List<double> _channelDown;

        double _longStopLoss;
        double _shortStopLoss;

        public override void Execute()
        {
            if (CandleCount <= 2)
                return;

            _maF = MaFast.param.LinesIndicators[0].PriceSeries;
            _maS = MaSlow.param.LinesIndicators[0].PriceSeries;
            _stochastic = Stochastic.param.LinesIndicators[0].PriceSeries;
            _cci = Cci.param.LinesIndicators[0].PriceSeries;
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
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _maF[bar - 1] < _maS[bar - 1] && _maF[bar] > _maS[bar] && _maF[bar - 1] < _maF[bar] 
                && _maS[bar - 1] < _maS[bar] && _stochastic[bar] < StochLong.Value && _cci[bar] <= CciLong.Value)
            {
                if (_long != null && _long.ExitBar == bar + 1)
                    return;

                _long = BuyAtMarket(bar + 1, 1, "Long");
                _longStopLoss = _channelDown[bar + 1];
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _maF[bar - 1] > _maS[bar - 1] && _maF[bar] < _maS[bar] && _maF[bar - 1] > _maF[bar]
                && _maS[bar - 1] > _maS[bar] && _stochastic[bar] > StochShort.Value && _cci[bar] >= CciShort.Value)
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
                if (_cci[bar] >= CciShort.Value)
                    SellAtMarket(bar + 1, _long, "long stop " + _long.EntryPrice.ToString());
                else
                    SellAtStop(bar + 1, _long, _longStopLoss, "long stoploss");
            }
            if (ShortPos.Count > 0)
            {
                if (_cci[bar] <= CciLong.Value)
                    CoverAtMarket(bar + 1, _short, "short stop " + _short.EntryPrice.ToString());
                else
                    CoverAtStop(bar + 1, _short, _shortStopLoss, "short stoploss");
            }
        }

        public override void SetSettingDefault()
        {
            MaFast.param.LinesIndicators[0].LineParam[0].Value = 7;
            MaSlow.param.LinesIndicators[0].LineParam[0].Value = 15;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 06, 23);
            DesParamStratetgy.DateChange = new DateTime(2021, 06, 23);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Flat;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/b67bl14ps1-robot-ccistochma";
            DesParamStratetgy.NameStrategy = "CCIStochMa";
        }
    }
}
