using ScriptSolution;
using ScriptSolution.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceEts;
using System.Drawing;
using ScriptSolution.Indicators;
using SourceEts.Models.TimeFrameTransformModel;
using SourceEts.Models;

namespace Robot.Бесплатный_робот
{
    internal class HeikenAshiCompress : Script
    {
        public ParamOptimization TimeFrame = new ParamOptimization(10, 5, 20, 1, "ТаймФрейм");
        public ParamOptimization MaPeriod = new ParamOptimization(2, 5, 20, 1, "Период");

        public ParamOptimization VisibleCompress = new ParamOptimization(false, "Компрессия");
        public ParamOptimization VisibleDecomoress = new ParamOptimization(false, "Декомпрессия");
        public ParamOptimization VisibleHeikenAshiBase = new ParamOptimization(false, "HeikenAshi base");
        public ParamOptimization VisibleHeikenAshiCompress = new ParamOptimization(false, "HeikenAshi компрессия");
        public ParamOptimization VisibleHeikenAshiDecompress = new ParamOptimization(false, "HeikenAshi декомпрессия");



        CandlesSeries _candlesCompress = new CandlesSeries();
        CandlesSeries _candlesDecompress = new CandlesSeries();
        CandlesSeries _candlesHeikenAhi = new CandlesSeries();
        CandlesSeries _candlesHeikenAhiBigCompress = new CandlesSeries();
        CandlesSeries _candlesHeikenAhiBigDecompress = new CandlesSeries();
        List<double> _sma = new List<double>();
        List<double> _smaDecompress = new List<double>();
        public override void Execute()
        {

            for (int bar = IndexBar; bar < CandleCount - 1; bar++)
            {
                CompressTo(Candles, _candlesCompress, bar + 1, TimeFrame.ValueInt); //сжимаем базовый таймфрейм в указанный таймфрейм в интерфейсе во вкладке робот
                DecompressFrom(Candles, _candlesCompress, _candlesDecompress, bar + 1); //Разжимаем сжатый таймфрейм в базовый таймфрейм
                GetHeikenAshi(Candles, _candlesHeikenAhi, bar + 1); //преобразуем свечи базового таймфрейма в свечи HeikenAshi
                if (bar + 1 == 49)
                { }
                GetHeikenAshi(_candlesCompress, _candlesHeikenAhiBigCompress, _candlesCompress.CandleCount - 1); // преобразуем свечи сжатого таймфрейма в свечи HeikenAshi
                //CompressTo(_candlesCompress, _candlesHeikenAhiBigCompress, bar + 1, TimeFrame.ValueInt);
                DecompressFrom(Candles, _candlesHeikenAhiBigCompress, _candlesHeikenAhiBigDecompress, bar + 1); // Разжимаем сжатый таймфрейм HeikenAshi в базовый таймфрейм
            }

            if (VisibleCompress.ValueBool)
                PlotPriceCanldes("Compress", _candlesCompress.OpenSeries,
                                _candlesCompress.HighSeries, _candlesCompress.LowSeries, _candlesCompress.CloseSeries,
                                new UserChartPropModel { NumberPanel = 1 });

            if (VisibleDecomoress.ValueBool)
                PlotPriceCanldes("Decompress", _candlesDecompress.OpenSeries,
                                _candlesDecompress.HighSeries, _candlesDecompress.LowSeries, _candlesDecompress.CloseSeries,
                                new UserChartPropModel { NumberPanel = 2 });

            //Plotseries("Volume", _candlesDecompress.Volume, new UserChartPropModel { ColorLine = Color.Red, NumberPanel = 3, TypeLine =EnumTypeLine.Histogram });
            if (VisibleHeikenAshiBase.ValueBool)
                PlotPriceCanldes("HeikenAhiBaseTimeFrame", _candlesHeikenAhi.OpenSeries,
                    _candlesHeikenAhi.HighSeries, _candlesHeikenAhi.LowSeries, _candlesHeikenAhi.CloseSeries,
                    new UserChartPropModel { NumberPanel = 3 });
            if (VisibleHeikenAshiCompress.ValueBool)
                PlotPriceCanldes("HeikenAhiBaseTimeFrameBigCompress", _candlesHeikenAhiBigCompress.OpenSeries,
                    _candlesHeikenAhiBigCompress.HighSeries, _candlesHeikenAhiBigCompress.LowSeries, _candlesHeikenAhiBigCompress.CloseSeries,
                    new UserChartPropModel { NumberPanel = 4 });
            if (VisibleHeikenAshiDecompress.ValueBool)
                PlotPriceCanldes("HeikenAhiBaseTimeFrameBigDecopress", _candlesHeikenAhiBigDecompress.OpenSeries,
                        _candlesHeikenAhiBigDecompress.HighSeries, _candlesHeikenAhiBigDecompress.LowSeries, _candlesHeikenAhiBigDecompress.CloseSeries,
                        new UserChartPropModel { NumberPanel = 5 });

            //Sma(_candlesCompress.CloseSeries, MaPeriod.ValueInt, _sma);
            //if (VisibleHeikenAshiCompress.ValueBool)
            //    Plotseries("SMA compress", _sma, new UserChartPropModel { ColorLine = Color.Blue, NumberPanel = 4 });

            //DecompressFrom(_candlesHeikenAhi, _sma, _smaDecompress, bar + 1);

            //if (VisibleHeikenAshiDecompress.ValueBool)
            //    Plotseries("SMA decompress", _smaDecompress, new UserChartPropModel { ColorLine = Color.Blue, NumberPanel = 5 });
        }
        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1";
            DesParamStratetgy.DateRelease = new DateTime(2022, 02, 28);
            DesParamStratetgy.DateChange = new DateTime(2022, 03, 04);
            DesParamStratetgy.Author = "1EX";

            DesParamStratetgy.Description = "";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.TypeStrategy = EnumStrategy.Trend;
            DesParamStratetgy.LinkFullDescription = "";
            DesParamStratetgy.NameStrategy = "HeikenAshi Compress";
        }
    }
}
