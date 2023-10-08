using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SourceEts;
using ScriptSolution;
using ScriptSolution.Indicators;
using ScriptSolution.Model;

namespace ModulSolution.Robots
{

    /// <summary>
    /// Стратегия скользящие средние
    /// </summary>

    public class MMF : Script
    {

        public CreateIndicator FastMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Быстрая");
        public CreateIndicator SlowMa = new CreateIndicator(EnumIndicators.MovingAvarage, 0, "Медленная");
        public CreateIndicator Fractal = new CreateIndicator(EnumIndicators.Fractals, 0, "");
        public CreateIndicator VolBuySell = new CreateIndicator(EnumIndicators.VolBuySell, 1, "");

        bool _isAcrossLong = false;//Пересечение было фрактала или нет после пересечения МА
        bool _isAcrossShort = false;//Пересечение было фрактала или нет после пересечения МА
        bool _isMaCrossLong = false;
        bool _isMaCrossShort = false;

        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                #region Открытие позиции и переворт позиции

                if (CrossOver(FastMa.param.LinesIndicators[0].PriceSeries,
                  SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                {
                    _isMaCrossLong = true;
                    _isMaCrossShort = false;
                    _isAcrossShort = false;
                    _isAcrossLong = false;

                    if (Candles.OpenSeries[bar + 1] > Fractal.param.LinesIndicators[0].PriceSeries[bar + 1])
                        _isAcrossLong = true;
                    CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                    //BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                }
                if (_isMaCrossLong && !_isAcrossLong)
                    BuyGreater(bar + 1, Fractal.param.LinesIndicators[0].PriceSeries[bar + 1], 1,"");
                if (LongPos.Count > 0)
                    _isMaCrossLong = false;

                if (CrossUnder(FastMa.param.LinesIndicators[0].PriceSeries,
                    SlowMa.param.LinesIndicators[0].PriceSeries, bar))
                {
                    _isMaCrossLong = false;
                    _isMaCrossShort = true;
                    _isAcrossShort = false;
                    _isAcrossLong = false;
                    if (Candles.OpenSeries[bar + 1] < Fractal.param.LinesIndicators[1].PriceSeries[bar + 1])
                        _isAcrossShort = true;
                    SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
                    //ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                }

                if (_isMaCrossShort && !_isAcrossShort)
                    ShortLess(bar + 1, Fractal.param.LinesIndicators[1].PriceSeries[bar + 1], 1,"");
                if (ShortPos.Count > 0)
                    _isMaCrossShort = false;

                #endregion

                #region Закрытие позиции по времени, встроенным стопам и рискам
                if (MarketPosition != 0)
                {
                    SendStandartStopFromForm(bar + 1, "");
                    SendTimePosCloseFromForm(bar + 1, "");
                    SendClosePosOnRiskFromForm(bar + 1, "");
                }
                #endregion

                #region Вывод информации
                if (bar > 2)
                {
                    ParamDebug("Fast MA тек.", Math.Round(FastMa.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                    ParamDebug("Fast MA пред.", Math.Round(FastMa.param.LinesIndicators[0].PriceSeries[bar], 4));

                }

                if (bar > 2 && SlowMa.param.LinesIndicators[0].PriceSeries.Count > 2)
                {
                    ParamDebug("slow MA тек.", Math.Round(SlowMa.param.LinesIndicators[0].PriceSeries[bar + 1], 4));
                    ParamDebug("slow MA пред.", Math.Round(SlowMa.param.LinesIndicators[0].PriceSeries[bar], 4));

                }
                ParamDebug("_isMaCrossLong", _isMaCrossLong);
                ParamDebug("_isAcrossLong", _isAcrossLong);
                ParamDebug("_isMaCrossShort", _isMaCrossShort);
                ParamDebug("_isAcrossShort", _isAcrossShort);

                #endregion
            }

        }

        public override void SetSettingDefault()
        {
            FastMa.param.LinesIndicators[0].ArgbLine = Color.Green.ToArgb();
            FastMa.param.LinesIndicators[0].LineParam[0].Value = 50;
            SlowMa.param.LinesIndicators[0].ArgbLine = Color.Red.ToArgb();
            SlowMa.param.LinesIndicators[0].LineParam[0].Value = 100;
        }


        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2015, 06, 21);
            DesParamStratetgy.DateChange = new DateTime(2020, 02, 08);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec357394324";
            DesParamStratetgy.NameStrategy = "MMF";

        }
    }


}
