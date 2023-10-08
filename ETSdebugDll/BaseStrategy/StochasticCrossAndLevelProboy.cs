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

    public class StochasticCrossAndLevelProboy : Script
    {
        public CreateIndicator Stochastic = new CreateIndicator(EnumIndicators.Stochastic, 1, "");
        public ParamOptimization LevelUp = new ParamOptimization(70, 1, 10, 5, "Верх. уровень", "Уровень, выше которого должно произойти пересечение %D пересекает сверху вниз %K для открытия шорт и закрытие лонг. Достаточно чтоб хотя бы одна из линий находилась в пределах этой зоны при пересечении");
        public ParamOptimization LevelDown = new ParamOptimization(30, 1, 10, 5, "Ниж. уровень", "Уровень, выше которого должно произойти пересечение %D пересекает снизу верх %K для открытия лонг и закрытие шорт. Достаточно чтоб хотя бы одна из линий находилась в пределах этой зоны при пересечении");
        public ParamOptimization Proboy = new ParamOptimization(1, 1, 10, 5, "Пробой", "Величина, на которую должен произойти пробой свечи, на которой было пересечение. Умножается на минимальный шаг цены.");


        private double _high; //high свечи, на которой было пересечение %D пересекает снизу верх %K
        private double _low;//_low свечи, на которой было пересечение %D пересекает сверху вниз %K
        private int _lastBar = 0;
        public override void Execute()
        {
            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 1)
                    continue;
                #region Открытие позиции и переворт позиции

                if (_lastBar != bar)
                {
                    if (Stochastic.param.LinesIndicators[0].PriceSeries[bar - 1] <= LevelDown.Value ||
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar - 1] <= LevelDown.Value)
                        if (CrossOver(Stochastic.param.LinesIndicators[0].PriceSeries,
                            Stochastic.param.LinesIndicators[1].PriceSeries, bar))
                        {
                            _high = Candles.HighSeries[bar];
                        }

                    if (Stochastic.param.LinesIndicators[0].PriceSeries[bar - 1] >= LevelUp.Value ||
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar - 1] >= LevelUp.Value)
                        if (CrossUnder(Stochastic.param.LinesIndicators[0].PriceSeries,
                            Stochastic.param.LinesIndicators[1].PriceSeries, bar))
                        {
                            _low = Candles.LowSeries[bar];
                        }
                    _lastBar = bar;
                }

                var pos = GetShortPositions(FinInfo);
                if (pos.Count != 0 && _high > 0)
                    CoverAtStop(bar + 1, pos[0], _high + Proboy.ValueInt * FinInfo.Security.MinStep, "Закрытие короткой позиции");

                if (Math.Abs(_high) > 0.00000001)
                    BuyGreater(bar + 1, _high + Proboy.ValueInt * FinInfo.Security.MinStep, 1, "Открытие длинной позиции");

                if (LongPos.Count > 0 && LongPos[0].Pos != 0)
                    _high = 0;

                pos = GetLongPosition(FinInfo);
                if (pos.Count != 0 && _low > 0)
                    SellAtStop(bar + 1, pos[0], _low - Proboy.ValueInt * FinInfo.Security.MinStep, "Закрытие длинной позиции");

                if (Math.Abs(_low) > 0.00000001)
                    ShortLess(bar + 1, _low - Proboy.ValueInt * FinInfo.Security.MinStep, 1, "Открытие короткой позиции");

                if (ShortPos.Count > 0 && ShortPos[0].Pos != 0)
                    _low = 0;

                #endregion


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

        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "2";
            DesParamStratetgy.DateRelease = new DateTime(2017, 03, 03);
            DesParamStratetgy.DateChange = new DateTime(2018, 02, 07);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Flat;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/tpost/1luzb5k7r1-robot-stochasticcrossandlevelproboy-stoh";
            DesParamStratetgy.NameStrategy = "StochasticCrossAndLevelProboy";

        }
    }


}
