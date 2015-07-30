using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using d3.Scale;

namespace d3.ViewModel
{
    class BarChartViewModel : Notifiable
    {
        private Ordinal xScale = new Ordinal();
        public Ordinal XScale { get { return xScale; } set { xScale = value; OnPropertyChanged("XScale"); } }

        private Linear yScale = new Linear();
        public Linear YScale { get { return yScale; } set { yScale = value; OnPropertyChanged("YScale"); } }

        private Double height = 350;
        public Double Height
        {
            get { return height; }
            set
            {
                height = value;
                HorizontalAxisCanvasTop = value - 30;
                OnPropertyChanged("Height");
            }
        }

        private Double width = 580;
        public Double Width
        {
            get { return width; }
            set
            {
                width = value;
                LegendAreaWidth = 140;
                ChartAreaWidth = width - LegendAreaWidth;
                OnPropertyChanged("Width");
            }
        }
        
        private Double chartAreaWidth;
        public Double ChartAreaWidth { get { return chartAreaWidth; } set { chartAreaWidth = value; OnPropertyChanged("ChartAreaWidth"); } }

        private Double legendAreaWidth;
        public Double LegendAreaWidth { get { return legendAreaWidth; } set { legendAreaWidth = value; OnPropertyChanged("LegendAreaWidth"); } }

        private Double horizontalAxisCanvasTop = 300;
        public Double HorizontalAxisCanvasTop { get { return horizontalAxisCanvasTop; } set { horizontalAxisCanvasTop = value; OnPropertyChanged("HorizontalAxisCanvasTop"); } }

        private Double BarWidth { get { return Math.Min(60, xScale.RangeBand / 2); } }

        public Func<Object, Int32, Double> WidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => HorizontalAxisCanvasTop - yScale.Map((d as Tuple<Object, Double>).Item2); } }
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => xScale.Map((d as Tuple<Object, Double>).Item1) - BarWidth / 2; } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Double>).Item2); } }

        private d3.Data chartData;
        public d3.Data ChartData { get { return chartData; } }

        private IEnumerable<Tuple<Object, Double>> data;
        public IEnumerable<Tuple<Object, Double>> Data { get { return data; } set { data = value; } }
        
        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchXGetter { get { return (d, index) => 0; } }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - Data.Count() * 20 - (Data.Count() - 1) * 10) / 2 + index * 30;
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => 25; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as Tuple<Object, Double>).Item1.ToString(); } }
        public Func<Object, Int32, Color> LegendTextColorGetter { get { return (d, index) => /*(d as Model.Bin).IsFilteredOut ? Colors.LightGray :*/ Colors.Black; } }

        
        public Func<Object, Int32, Color> ColorGetter
        {
            get
            {
                return (bin, index) => ColorScheme.Category10.Colors[index % ColorScheme.Category10.Colors.Count]; //(bin as Model.Bin).Index % CategoricalColors.Count];
            }
        }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => xScale.RangeBand; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => (d as Tuple<Object, Double>).Item2.ToString("0.##"); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) => xScale.Map((d as Tuple<Object, Double>).Item1) - xScale.RangeBand / 2; } }
        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Double>).Item2) - 18; } }

        public void Update()
        {
            chartData = new d3.Data()
            {
                List = data.Select(d => d as Object).ToList()
            };

            yScale = new Linear()
            {
                DomainStart = 0,
                DomainEnd = data.Select(d => d.Item2).Max(),
                RangeStart = HorizontalAxisCanvasTop,
                RangeEnd = 50
            };

            yScale.Nice();

            YScale = yScale;

            xScale = new Ordinal()
            {
                RangeStart = 50,
                RangeEnd = chartAreaWidth
            };
            foreach (Tuple<Object, Double> d in data)
            {
                xScale.Domain.Add(d.Item1);
            }
            XScale = xScale;

            OnPropertyChanged("ChartData");
        }

    }
}
