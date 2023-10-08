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

    public class StochasticLevel : Script
    {
        public CreateIndicator Stochastic = new CreateIndicator(EnumIndicators.Stochastic, 1, "");
        public ParamOptimization LevelUp = new ParamOptimization(80, 1, 10, 5, "Верх. уровень", "Уровень, выше которого должно произойти пересечение %D пересекает сверху вниз %K для открытия шорт и закрытие лонг. Достаточно чтоб хотя бы одна из линий находилась в пределах этой зоны при пересечении");
        public ParamOptimization LevelDown = new ParamOptimization(20, 1, 10, 5, "Ниж. уровень", "Уровень, выше которого должно произойти пересечение %D пересекает снизу верх %K для открытия лонг и закрытие шорт. Достаточно чтоб хотя бы одна из линий находилась в пределах этой зоны при пересечении");


        private bool _kUp;
        private bool _dUp;
        private bool _kDown;
        private bool _dDown;

        public override void Execute()
        {

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (bar < 1)
                    continue;
                #region Открытие позиции и переворт позиции

                if (Stochastic.param.LinesIndicators[0].PriceSeries[bar - 1] <= LevelDown.Value)
                    _kUp = true;

                if (Stochastic.param.LinesIndicators[1].PriceSeries[bar - 1] <= LevelDown.Value)
                    _dUp = true;

                if (_kUp && _dUp)
                    if (Stochastic.param.LinesIndicators[1].PriceSeries[bar] > LevelDown.Value &&//D% вышла раньше пересекла уровень, потом пеересекает %K
                        Stochastic.param.LinesIndicators[0].PriceSeries[bar - 1] <= LevelDown.Value &&
                        Stochastic.param.LinesIndicators[0].PriceSeries[bar] > LevelDown.Value ||

                        Stochastic.param.LinesIndicators[0].PriceSeries[bar] > LevelDown.Value && //K% вышла раньше пересекла уровень, потом пеересекает %D
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar - 1] <= LevelDown.Value &&
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar] > LevelDown.Value ||

                        Stochastic.param.LinesIndicators[0].PriceSeries[bar - 1] <= LevelDown.Value && //Одновременно пересечение уровня
                        Stochastic.param.LinesIndicators[0].PriceSeries[bar] > LevelDown.Value &&
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar - 1] <= LevelDown.Value &&
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar] > LevelDown.Value
                       )
                    {
                        CoverAtMarket(bar + 1, GetLastPosition(), "Закрытие короткой позиции");
                        BuyAtMarket(bar + 1, 1, "Открытие длинной позиции");
                        _kUp = false;
                        _dUp = false;
                        _kDown = false;
                        _dDown = false;
                    }

                if (Stochastic.param.LinesIndicators[0].PriceSeries[bar - 1] >= LevelUp.Value)
                    _kDown = true;

                if (Stochastic.param.LinesIndicators[1].PriceSeries[bar - 1] >= LevelUp.Value)
                    _dDown = true;

                if (_kDown && _dDown)
                    if (Stochastic.param.LinesIndicators[1].PriceSeries[bar] < LevelUp.Value &&//D% вышла раньше пересекла уровень, потом пеересекает %K
                        Stochastic.param.LinesIndicators[0].PriceSeries[bar - 1] >= LevelUp.Value &&
                        Stochastic.param.LinesIndicators[0].PriceSeries[bar] < LevelUp.Value ||

                        Stochastic.param.LinesIndicators[0].PriceSeries[bar] < LevelUp.Value && //K% вышла раньше пересекла уровень, потом пеересекает %D
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar - 1] >= LevelUp.Value &&
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar] < LevelUp.Value ||

                        Stochastic.param.LinesIndicators[0].PriceSeries[bar - 1] >= LevelUp.Value && //Одновременно пересечение уровня
                        Stochastic.param.LinesIndicators[0].PriceSeries[bar] < LevelUp.Value &&
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar - 1] >= LevelUp.Value &&
                        Stochastic.param.LinesIndicators[1].PriceSeries[bar] < LevelUp.Value
                       )
                    {
                        SellAtMarket(bar + 1, GetLastPosition(), "Закрытие длинной позиции");
                        ShortAtMarket(bar + 1, 1, "Открытие короткой позиции");
                        _kUp = false;
                        _dUp = false;
                        _kDown = false;
                        _dDown = false;
                    }

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
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2017, 03, 03);
            DesParamStratetgy.DateChange = new DateTime(2017, 03, 03);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Flat;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec366705935";
            DesParamStratetgy.NameStrategy = "StochasticLevel";

        }
    }


}
