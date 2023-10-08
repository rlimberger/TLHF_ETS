using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using SourceEts.Config;
using SourceEts.Models;

namespace ETSBots
{
    public class SecondInstrSignal : Script
    {
        public CreateIndicator Alligator = new CreateIndicator(EnumIndicators.Alligator, 0, "");

        public ParamOptimization Instr = new ParamOptimization(EnumTypeGetIEnumerable.GetSeccodeList, "Поводырь");

        List<double> _alligatorsFast;
        List<double> _alligatorsMiddle;
        List<double> _alligatorsSlow;

        IPosition _long;
        IPosition _short;


        public override void Execute()
        {
            _alligatorsFast = Alligator.param.LinesIndicators[2].PriceSeries;
            _alligatorsMiddle = Alligator.param.LinesIndicators[1].PriceSeries;
            _alligatorsSlow = Alligator.param.LinesIndicators[0].PriceSeries;

            for (var bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                if (CandleCount <= 2)
                    return;

                Trade(bar);



                Drawing();

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

        private void Trade(int bar)
        {
            if (CandleCount == 0 || Instr.FinInfo.Candles.CloseSeries[bar] == 0)
                return;

            if ((_long == null || _long.ExitOrderStatus == EnumOrderStatus.Performed) && _alligatorsFast[bar] > _alligatorsMiddle[bar] 
                && _alligatorsMiddle[bar] > _alligatorsSlow[bar])
            {
                if (_short != null && _short.ExitOrderStatus == EnumOrderStatus.None)
                    CoverAtClose(bar + 1, _short, "Short Exit");

                _long = BuyAtMarket(Instr.FinInfo, bar + 1, 1, "Long");
            }
            if ((_short == null || _short.ExitOrderStatus == EnumOrderStatus.Performed) && _alligatorsFast[bar] < _alligatorsMiddle[bar] 
                && _alligatorsMiddle[bar] < _alligatorsSlow[bar])
            {
                if (_long != null && _long.ExitOrderStatus == EnumOrderStatus.None)
                    SellAtClose(bar + 1, _long, "Long Exit");

                _short = ShortAtMarket(Instr.FinInfo, bar + 1, 1, "Short");
            }
        }

        private void Drawing()
        {
            PlotCanldes("FirstdPrice", Instr.FinInfo, new UserChartPropModel()
            {
                NumberPanel = 1,
                Transparency = 100
            });
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2021, 07, 20);
            DesParamStratetgy.DateChange = new DateTime(2021, 07, 20);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "https://etstrading.ru/roboti#rec366282089";
            DesParamStratetgy.NameStrategy = "SecondInstrSignal";
        }
    }
}
