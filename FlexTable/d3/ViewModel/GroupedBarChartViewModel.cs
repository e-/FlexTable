using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using d3.Scale;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DataPoint = System.Tuple<object, object, double, object>;

namespace d3.ViewModel
{
    class GroupedBarChartViewModel : Notifiable
    {
        const Double PaddingLeft = 0;
        const Double PaddingTop = 30;
        public const Double PaddingRight = 10;
        const Double PaddingBottom = 0;
        const Double HorizontalAxisHeight = 25;
        const Double HorizontalAxisLabelHeight = 20;
        const Double VerticalAxisWidth = 40;
        const Double VerticalAxisLabelWidth = 20;
        public const Double LegendPatchWidth = 20;
        public const Double LegendPatchHeight = 20;
        public const Double LegendPatchSpace = 10;

        private Ordinal xScale = new Ordinal();
        public Ordinal XScale { get { return xScale; } set { xScale = value; OnPropertyChanged("XScale"); } }

        private Linear yScale = new Linear();
        public Linear YScale { get { return yScale; } set { yScale = value; OnPropertyChanged("YScale"); } }

        private Double height = 350;
        public Double Height { get { return height; } set { height = value; OnPropertyChanged("Height"); } }

        private Double width = 580;
        public Double Width { get { return width; } set { width = value; OnPropertyChanged("Width"); } }

        private Double chartAreaEndX;
        public Double ChartAreaEndX { get { return chartAreaEndX; } set { chartAreaEndX = value; OnPropertyChanged("ChartAreaEndX"); } }

        private Double chartAreaEndY = 300;
        public Double ChartAreaEndY { get { return chartAreaEndY; } set { chartAreaEndY = value; OnPropertyChanged("ChartAreaEndY"); } }

        public Double BarWidth { get { return Math.Min(60, xScale.RangeBand * 0.8 / MaxCountInGroup); } }
        public Int32 MaxCountInGroup { get; set; }

        public Func<Object, Int32, Double> WidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => ChartAreaEndY - yScale.Map((d as DataPoint).Item3); } }
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => 
            xDictionary[(d as DataPoint).Item1].Map((d as DataPoint).Item2) - BarWidth / 2; 
        } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as DataPoint).Item3); } }
        public Func<TextBlock, Double, Double> LabelFontSizeGetter
        {
            get
            {
                return (textBlock, currentSize) => textBlock.ActualWidth > xScale.RangeBand ? currentSize * xScale.RangeBand / textBlock.ActualWidth * 0.9 : currentSize;
            }
        }
        public Func<Object, Int32, Double> OpacityGetter { get { return (d, index) => IsSelecting ? ((d as DataPoint).Item1 == selectedKey1 && (d as DataPoint).Item2 == selectedKey2 ? 1.0 : 0.2) : 1.0; } }

        public Func<Object, Int32, Double> HandleHeightGetter { get { return (d, index) => ChartAreaEndY - PaddingTop; } }
        public Func<Object, Int32, Double> HandleYGetter { get { return (d, index) => PaddingTop; } }
        public Func<Object, Int32, Color> HandleColorGetter { get { return (bin, index) => Colors.Transparent; } }

        private Dictionary<Object, Ordinal> xDictionary = new Dictionary<Object, Ordinal>();
        private List<Object> secondaryKeys = new List<Object>();

        private Data legendData;
        public Data LegendData { get { return legendData; } }

        private Data chartData;
        public Data ChartData { get { return chartData; } }

        private IEnumerable<DataPoint> data;
        public IEnumerable<DataPoint> Data { get { return data; } set { data = value; } }

        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => LegendPatchWidth; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => LegendPatchHeight; } }
        public Func<Object, Int32, Double> LegendPatchXGetter { get { return (d, index) => 0; } }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - LegendData.List.Count() * LegendPatchHeight - (LegendData.List.Count() - 1) * LegendPatchSpace) / 2 + index * (LegendPatchHeight + LegendPatchSpace);
            }
        }
        public Func<Object, Int32, Double> LegendPatchOpacityGetter { get { return (d, index) => IsSelecting ? (d.ToString() == selectedKey2.ToString() ? 1.0 : 0.2) : 1.0; } }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => LegendPatchWidth + LegendPatchSpace; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => d.ToString(); } }
        public Func<Object, Int32, Color> LegendTextColorGetter { get { return (d, index) => /*(d as Model.Bin).IsFilteredOut ? Colors.LightGray :*/ Colors.Black; } }
        public Func<TextBlock, Object, Int32, Double> LegendTextOpacityGetter { get { return (textBlock, d, index) => IsSelecting ? (d.ToString() == selectedKey2.ToString() ? 1.0 : 0.2) : 1.0; } }

        public Func<Object, Int32, Color> ColorGetter
        {
            get
            {
                return (d, index) => ColorScheme.Category10.Colors[secondaryKeys.IndexOf((d as DataPoint).Item2) % ColorScheme.Category10.Colors.Count];
            }
        }

        public Func<Object, Int32, Color> LegendColorGetter
        {
            get
            {
                return (d, index) => ColorScheme.Category10.Colors[secondaryKeys.IndexOf(d) % ColorScheme.Category10.Colors.Count];
            }
        }

        private Data indicatorData;
        public Data IndicatorData => indicatorData;
        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => 100; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => Format.IntegerBalanced.Format((d as DataPoint).Item3); } }
        public Func<Object, Int32, Double> IndicatorXGetter
        {
            get
            {
                return (d, index) =>
                    xDictionary[(d as DataPoint).Item1].Map((d as DataPoint).Item2) - 50;
            }
        }

        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => yScale.Map((d as DataPoint).Item3) - 18; } }
        public Func<TextBlock, Object, Int32, Double> TextOpacityGetter { get { return (textBlock, d, index) => IsSelecting ? (d == selectedKey2 ? 1.0 : 0.2) : 1.0; } }



        private Visibility horizontalAxisVisibility;
        public Visibility HorizontalAxisVisibility { get { return horizontalAxisVisibility; } set { horizontalAxisVisibility = value; OnPropertyChanged("HorizontalAxisVisibility"); } }
        public Boolean IsHorizontalAxisVisible { get { return horizontalAxisVisibility == Visibility.Visible; } }

        private Visibility legendVisibility;
        public Visibility LegendVisibility { get { return legendVisibility; } set { legendVisibility = value; OnPropertyChanged("LegendVisibility"); } }
        public Boolean IsLegendVisible { get { return legendVisibility == Visibility.Visible; } }

        private Double horizontalAxisLabelCanvasTop;
        public Double HorizontalAxisLabelCanvasTop { get { return horizontalAxisLabelCanvasTop; } set { horizontalAxisLabelCanvasTop = value; OnPropertyChanged("HorizontalAxisLabelCanvasTop"); } }

        private Double horizontalAxisLabelCanvasLeft;
        public Double HorizontalAxisLabelCanvasLeft { get { return horizontalAxisLabelCanvasLeft; } set { horizontalAxisLabelCanvasLeft = value; OnPropertyChanged("HorizontalAxisLabelCanvasLeft"); } }

        private Double horizontalAxisLabelWidth;
        public Double HorizontalAxisLabelWidth { get { return horizontalAxisLabelWidth; } set { horizontalAxisLabelWidth = value; OnPropertyChanged("HorizontalAxisLabelWidth"); } }

        private Double verticalAxisLabelCanvasTop;
        public Double VerticalAxisLabelCanvasTop { get { return verticalAxisLabelCanvasTop; } set { verticalAxisLabelCanvasTop = value; OnPropertyChanged("VerticalAxisLabelCanvasTop"); } }

        private Double verticalAxisLabelCanvasLeft;
        public Double VerticalAxisLabelCanvasLeft { get { return verticalAxisLabelCanvasLeft; } set { verticalAxisLabelCanvasLeft = value; OnPropertyChanged("VerticalAxisLabelCanvasLeft"); } }

        private Double verticalAxisLabelHeight;
        public Double VerticalAxisLabelHeight { get { return verticalAxisLabelHeight; } set { verticalAxisLabelHeight = value; OnPropertyChanged("VerticalAxisLabelHeight"); } }

        private Double verticalAxisCanvasLeft;
        public Double VerticalAxisCanvasLeft { get { return verticalAxisCanvasLeft; } set { verticalAxisCanvasLeft = value; OnPropertyChanged("VerticalAxisCanvasLeft"); } }

        private Double legendAreaWidth = 140;
        public Double LegendAreaWidth { get { return legendAreaWidth; } set { legendAreaWidth = value; OnPropertyChanged("LegendAreaWidth"); } }

        private String horizontalAxisLabel;
        public String HorizontalAxisLabel { get { return horizontalAxisLabel; } set { horizontalAxisLabel = value; OnPropertyChanged("HorizontalAxisLabel"); } }

        private String verticalAxisLabel;
        public String VerticalAxisLabel { get { return verticalAxisLabel; } set { verticalAxisLabel = value; OnPropertyChanged("VerticalAxisLabel"); } }

        public Boolean YStartsWithZero { get; set; } = false;

        public Boolean IsSelecting { get; set; } = false;
        private Object selectedKey1 = null;
        private Object selectedKey2 = null;

        public void UpdateLegendData()
        {
            MaxCountInGroup = 0;
            xDictionary.Clear();
            secondaryKeys.Clear();

            foreach (Object category2 in data.Select(d => d.Item2).Distinct())
            {
                secondaryKeys.Add(category2);
            }

            foreach (Object category1 in xScale.Domain)
            {
                Int32 count = data.Where(d => d.Item1 == category1).Select(d => d.Item2).Distinct().Count();
                if (count > MaxCountInGroup)
                    MaxCountInGroup = count;
            }

            foreach (Object category1 in xScale.Domain)
            {
                Int32 count = data.Where(d => d.Item1 == category1).Select(d => d.Item2).Distinct().Count();
                Ordinal ordinal = new Ordinal()
                {
                    RangeStart = xScale.Map(category1) - BarWidth * count / 2,
                    RangeEnd = xScale.Map(category1) + BarWidth * count / 2
                };

                foreach (Object category2 in data.Where(d => d.Item1 == category1).Select(d => d.Item2).Distinct())
                {
                    ordinal.Domain.Add(category2);
                }
                xDictionary.Add(category1, ordinal);
            }

            legendData = new Data()
            {
                List = secondaryKeys
            };

            OnPropertyChanged("LegendData");
        }

        public void Update()
        {
            chartData = new Data()
            {
                List = data.Select(d => d as Object).ToList()
            };

            if (IsHorizontalAxisVisible)
            {
                ChartAreaEndY = height - PaddingBottom - HorizontalAxisHeight - HorizontalAxisLabelHeight;
            }
            else
            {
                ChartAreaEndY = height - PaddingBottom;
            }

            if (IsLegendVisible)
            {
                ChartAreaEndX = width - PaddingLeft - PaddingRight - LegendAreaWidth;
            }
            else
            {
                ChartAreaEndX = width - PaddingLeft - PaddingRight;
            }

            if (data.Count() > 20)
            {
                indicatorData = new Data() { List = new List<object>() };
            }
            else
            {
                indicatorData = chartData;
            }

            HorizontalAxisLabelCanvasLeft = PaddingLeft + VerticalAxisWidth + VerticalAxisLabelWidth;
            HorizontalAxisLabelCanvasTop = ChartAreaEndY + HorizontalAxisHeight;
            HorizontalAxisLabelWidth = ChartAreaEndX - PaddingLeft - VerticalAxisWidth - VerticalAxisLabelWidth;

            VerticalAxisCanvasLeft = PaddingLeft + VerticalAxisLabelWidth + VerticalAxisWidth;
            VerticalAxisLabelCanvasLeft = PaddingLeft + VerticalAxisLabelWidth / 2 - (ChartAreaEndY - PaddingTop) / 2;
            VerticalAxisLabelCanvasTop = PaddingTop + (ChartAreaEndY - PaddingTop) / 2;
            VerticalAxisLabelHeight = ChartAreaEndY - PaddingTop;

            IEnumerable<Double> values = data.Select(d => d.Item3);
            Double yMin = values.Min(), yMax = values.Max();
            if (YStartsWithZero) yMin = 0;
            else if (yMin == yMax)
            {
                if (yMin == 0.0)
                {
                    yMin = -1; yMax = 1;
                }
                else if (yMin < 0)
                {
                    yMin *= 1.2;
                    yMax *= 0.8;
                }
                else
                {
                    yMin *= 0.8;
                    yMax *= 1.2;
                }
            }

            Linear yScale = new Linear()
            {
                DomainStart = yMin,
                DomainEnd = yMax,
                RangeStart = ChartAreaEndY,
                RangeEnd = PaddingTop
            };

            yScale.Nice();

            YScale = yScale;

            Ordinal xScale = new Ordinal()
            {
                RangeStart = VerticalAxisCanvasLeft,
                RangeEnd = ChartAreaEndX + PaddingLeft
            };

            foreach (Object category1 in data.Select(d => d.Item1).Distinct())
            {
                xScale.Domain.Add(category1);
            }

            XScale = xScale;

            UpdateLegendData();

            OnPropertyChanged("ChartData");
            OnPropertyChanged("IndicatorData");
        }

        public void SelectBar(Object datum)
        {
            DataPoint dataPoint = datum as DataPoint;
            IsSelecting = true;
            selectedKey1 = dataPoint.Item1;
            selectedKey2 = dataPoint.Item2;
        }

        public void UnselectBar(Object datum)
        {
            IsSelecting = false;
            selectedKey1 = selectedKey2 = null;
        }
    }
}
