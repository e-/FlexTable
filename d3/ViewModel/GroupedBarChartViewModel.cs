using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using d3.Scale;

namespace d3.ViewModel
{
    class GroupedBarChartViewModel : Notifiable
    {
        private d3.Scale.Ordinal xScale = new Ordinal();
        public d3.Scale.Ordinal XScale { get { return xScale; } set { xScale = value; OnPropertyChanged("XScale"); } }

        private d3.Scale.Linear yScale = new Linear();
        public d3.Scale.Linear YScale { get { return yScale; } set { yScale = value; OnPropertyChanged("YScale"); } }

        public Double BarWidth { get { return Math.Min(60, xScale.RangeBand * 0.8 / MaxCountInGroup); } }
        public Int32 MaxCountInGroup { get; set; }

        public Func<Object, Int32, Double> WidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => horizontalAxisCanvasTop - yScale.Map((d as Tuple<Object, Object, Double>).Item3); } }
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => 
            xDictionary[(d as Tuple<Object, Object, Double>).Item1].Map((d as Tuple<Object, Object, Double>).Item2) - BarWidth / 2; 
        } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Object, Double>).Item3); } }

        private Dictionary<Object, Scale.Ordinal> xDictionary = new Dictionary<Object, Scale.Ordinal>();
        private List<Object> secondaryKeys = new List<Object>();

        private d3.Data legendData;
        public d3.Data LegendData { get { return legendData; } }

        private d3.Data chartData;
        public d3.Data ChartData { get { return chartData; } }

        private Double chartAreaWidth;
        public Double ChartAreaWidth { get { return chartAreaWidth; } set { chartAreaWidth = value; OnPropertyChanged("ChartAreaWidth"); } }

        private Double legendAreaWidth;
        public Double LegendAreaWidth { get { return legendAreaWidth; } set { legendAreaWidth = value; OnPropertyChanged("LegendAreaWidth"); } }

        private Double horizontalAxisCanvasTop = 300;
        public Double HorizontalAxisCanvasTop { get { return horizontalAxisCanvasTop; } set { horizontalAxisCanvasTop = value; OnPropertyChanged("HorizontalAxisCanvasTop"); } }

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

        private IEnumerable<Tuple<Object, Object, Double>> data;
        public IEnumerable<Tuple<Object, Object, Double>> Data { get { return data; } set { data = value; } }
        

        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchXGetter { get { return (d, index) => 0; } }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - LegendData.List.Count() * 20 - (LegendData.List.Count() - 1) * 10) / 2 + index * 30;
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => 25; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => d.ToString(); } }
        public Func<Object, Int32, Color> LegendTextColorGetter { get { return (d, index) => /*(d as Model.Bin).IsFilteredOut ? Colors.LightGray :*/ Colors.Black; } }

        public Func<Object, Int32, Color> ColorGetter
        {
            get
            {
                return (d, index) => ColorScheme.Category10.Colors[secondaryKeys.IndexOf((d as Tuple< Object, Object, Double>).Item2) % ColorScheme.Category10.Colors.Count]; 
            }
        }

        public Func<Object, Int32, Color> LegendColorGetter
        {
            get
            {
                return (d, index) => ColorScheme.Category10.Colors[secondaryKeys.IndexOf(d) % ColorScheme.Category10.Colors.Count];
            }
        }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => 100; /* BarWidth;*/ } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => (d as Tuple<Object, Object, Double>).Item3.ToString("0.##"); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) =>
            xDictionary[(d as Tuple<Object, Object, Double>).Item1].Map((d as Tuple<Object, Object, Double>).Item2) - 50 /*BarWidth / 2*/; 
        } }
        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Object, Double>).Item3) - 18; } }

        public void Update()
        {
            chartData = new d3.Data()
            {
                List = data.Select(d => d as Object).ToList()
            };

            Linear yScale = new d3.Scale.Linear()
            {
                DomainStart = 0,
                DomainEnd = data.Select(d => d.Item3).Max(),
                RangeStart = horizontalAxisCanvasTop,
                RangeEnd = 50
            };

            yScale.Nice();

            YScale = yScale;

            Ordinal xScale = new d3.Scale.Ordinal()
            {
                RangeStart = 50,
                RangeEnd = ChartAreaWidth
            };

            foreach (Object category1 in data.Select(d => d.Item1).Distinct())
            {
                xScale.Domain.Add(category1);
            }

            XScale = xScale;

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

            legendData = new d3.Data()
            {
                List = secondaryKeys
            };

            OnPropertyChanged("ChartData");
            OnPropertyChanged("LegendData");
        }
    }
}
