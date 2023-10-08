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
    public class AdxMa : Script
    {
        public CreateIndicator MaFast = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Ma fast");
        public CreateIndicator MaSlow = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Ma slow");
        public CreateIndicator Adx = new CreateIndicator(EnumIndicators.Adx, 1, "");

        public ParamOptimization AdxMain = new ParamOptimization(33, 1, 500, 1, "ADX", "Порог входа в сделку по индикатору ADX");


        IPosition _long;
        IPosition _short;

        List<double> _maF;
        List<double> _maS;
        List<double> _adx;
        public override void Execute()
        {
            if (CandleCount <= 2)
                return;

            _maF = MaFast.param.LinesIndicators[0].PriceSeries;
            _maS = MaSlow.param.LinesIndicators[0].PriceSeries;
            _adx = Adx.param.LinesIndicators[0].PriceSeries;

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
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _maF[bar] > _maS[bar] && Candles.LowSeries[bar] > _maF[bar] 
                && Candles.OpenSeries[bar] < Candles.CloseSeries[bar] && _adx[bar] > AdxMain.Value)
            {
                _long = BuyAtMarket(bar + 1, 1, "Long");
            }
            if (LongPos.Count == 0 && ShortPos.Count == 0 && _maF[bar] < _maS[bar] && Candles.HighSeries[bar] < _maF[bar]
                && Candles.OpenSeries[bar] > Candles.CloseSeries[bar] && _adx[bar] > AdxMain.Value)
            {
                _short = ShortAtMarket(bar + 1, 1, "Short");
            }
        }

        private void Exit(int bar)
        {
            if (LongPos.Count > 0 && Candles.DateTimeCandle[bar].Day != Candles.DateTimeCandle[bar + 1].Day && _long.EntryTime.Day != Candles.DateTimeCandle[bar + 1].Day)
            {
                SellAtMarket(bar + 1, _long, "long stop " + _long.EntryPrice.ToString());
            }
            if (ShortPos.Count > 0 && Candles.DateTimeCandle[bar].Day != Candles.DateTimeCandle[bar + 1].Day && _short.EntryTime.Day != Candles.DateTimeCandle[bar + 1].Day)
            {
                CoverAtMarket(bar + 1, _short, "short stop " + _short.EntryPrice.ToString());
            }
        }

        public override void SetSettingDefault()
        {
            MaFast.param.LinesIndicators[0].LineParam[0].Value = 9;
            MaSlow.param.LinesIndicators[0].LineParam[0].Value = 21;
            Adx.param.LinesIndicators[0].LineParam[0].Value = 14;
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2021, 6, 22);
            DesParamStratetgy.DateChange = new DateTime(2021, 6, 22);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/zup9fbv6n1-robot-adxma";
            DesParamStratetgy.NameStrategy = "AdxMa";
        }
    }
}
