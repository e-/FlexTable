using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using d3.Scale;
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Series = System.Tuple<System.String, System.Collections.Generic.List<System.Tuple<System.Object, System.Double>>>;
using DataPoint = System.Tuple<System.Object, System.Double>;

namespace d3.ViewModel
{
    class LineChartViewModel : Notifiable
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
        
        public Func<Object, Int32, List<Point>> CoordinateGetter { get {
                return (d, index) => (d as Series).Item2.Select( tp => new Point(
                    xScale.Map(tp.Item1),
                    yScale.Map(tp.Item2)
                    )).ToList();
        } }

        private Data chartData;
        public Data ChartData => chartData;

        private Data circleData;
        public Data CircleData => circleData;

        private Data indicatorData;
        public Data IndicatorData => indicatorData;

        private IEnumerable<Series> data;
        public IEnumerable<Series> Data { get { return data; } set { data = value; } }
        
        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => LegendPatchWidth; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => LegendPatchHeight; } }
        public Func<Object, Int32, Double> LegendPatchXGetter { get { return (d, index) => 0; } }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - Data.Count() * LegendPatchHeight - (Data.Count() - 1) * LegendPatchSpace) / 2 + index * (LegendPatchHeight + LegendPatchSpace);
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => LegendPatchWidth + LegendPatchSpace; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as Series).Item1.ToString(); } }
        public Func<Object, Int32, Color> LegendTextColorGetter { get { return (d, index) => Colors.Black; } }
        
        public Func<Object, Int32, Color> ColorGetter { get { return (bin, index) => (AutoColor ? ColorScheme.Category10.Colors[index % 10] : ColorScheme.Category10.Colors.First()); } }
        public Func<Object, Int32, Color> StrokeGetter { get { return (bin, index) => (AutoColor ? ColorScheme.Category10.Colors[index % 10] : ColorScheme.Category10.Colors.First()); } }
        public Func<Object, Int32, Double> StrokeThicknessGetter { get { return (bin, index) => 3.0; } }

        public Func<Object, Int32, Double> XGetter { get { return (d, index) => xScale.Map((d as Tuple<Object, Double, Int32>).Item1); } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Double, Int32>).Item2); } }
        public Func<Object, Int32, Color> CircleColorGetter { get { return (d, index) => ColorScheme.Category10.Colors[(d as Tuple<Object, Double, Int32>).Item3 % 10]; } }
        public Func<Object, Int32, Double> RadiusGetter { get { return (d, index) => 10; } }
        public Func<Object, Int32, Double> OpacityGetter { get { return (d, index) => 0.8; } }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => xScale.RangeBand; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => Format.IntegerBalanced.Format((d as DataPoint).Item2); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) => xScale.Map((d as DataPoint).Item1) - xScale.RangeBand / 2; } }
        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => yScale.Map((d as DataPoint).Item2) - 18; } }

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

        public Boolean AutoColor { get; set; } = true;

        public void Update()
        {
            chartData = new d3.Data()
            {
                List = data.Select(d => d as Object).ToList()
            };

            Int32 index = 0;
            List<Object> circleList = new List<Object>();
            foreach (Series ser in data)
            {
                foreach(DataPoint dp in ser.Item2)
                {
                    circleList.Add(new Tuple<Object, Double, Int32>(dp.Item1, dp.Item2, index));
                }
                index++;
            }

            circleData = new d3.Data()
            {
                List = circleList
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

            HorizontalAxisLabelCanvasLeft = PaddingLeft + VerticalAxisWidth + VerticalAxisLabelWidth;
            HorizontalAxisLabelCanvasTop = ChartAreaEndY + HorizontalAxisHeight;
            HorizontalAxisLabelWidth = ChartAreaEndX - PaddingLeft - VerticalAxisWidth - VerticalAxisLabelWidth;
            
            VerticalAxisCanvasLeft = PaddingLeft + VerticalAxisLabelWidth + VerticalAxisWidth;
            VerticalAxisLabelCanvasLeft = PaddingLeft + VerticalAxisLabelWidth / 2 - (ChartAreaEndY - PaddingTop) / 2;
            VerticalAxisLabelCanvasTop = PaddingTop + (ChartAreaEndY - PaddingTop) / 2;
            VerticalAxisLabelHeight = ChartAreaEndY - PaddingTop;

            yScale = new Linear()
            {
                DomainStart = 0,
                DomainEnd = data.Select(series => (series as Series).Item2.Select(p => (p as DataPoint).Item2).Max()).Max(),
                RangeStart = ChartAreaEndY,
                RangeEnd = PaddingTop
            };

            yScale.Nice();

            YScale = yScale;

            xScale = new Ordinal()
            {
                RangeStart = VerticalAxisCanvasLeft,
                RangeEnd = ChartAreaEndX + PaddingLeft
            };
            foreach (DataPoint d in (data.First() as Series).Item2)
            {
                xScale.Domain.Add(d.Item1);
            }
            XScale = xScale;


            indicatorData = new Data()
            {
                List = data.SelectMany(d => d.Item2).Select(tp => tp as Object).ToList()
            };


            OnPropertyChanged("ChartData");
            OnPropertyChanged("CircleData");
            OnPropertyChanged("IndicatorData");
        }
    }
}
