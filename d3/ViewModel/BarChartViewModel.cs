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

        public Double ChartHeight { get { return 350; } }
        public Double ChartWidth { get { return 580; } }

        private Double BarWidth { get { return Math.Min(60, xScale.RangeBand / 2); } }

        public Func<Object, Int32, Double> WidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => ChartHeight - yScale.Map((d as Tuple<Object, Double>).Item2); } }
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => xScale.Map((d as Tuple<Object, Double>).Item1) - BarWidth / 2; } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Double>).Item2); } }

        private d3.Data chartData;
        public d3.Data ChartData { get { return chartData; } }

        private IEnumerable<Tuple<Object, Double>> data;
        public IEnumerable<Tuple<Object, Double>> Data
        {
            get { return data; }
            set
            {
                data = value;
                chartData = new d3.Data()
                {
                    List = data.Select(d => d as Object).ToList()
                };

                yScale = new Linear()
                {
                    DomainStart = 0,
                    DomainEnd = data.Select(d => d.Item2).Max(),
                    RangeStart = ChartHeight,
                    RangeEnd = 50
                };

                yScale.Nice();

                YScale = yScale;

                xScale = new Ordinal()
                {
                    RangeStart = 50,
                    RangeEnd = ChartWidth
                };
                foreach (Tuple<Object, Double> d in data)
                {
                    xScale.Domain.Add(d.Item1);
                }
                XScale = xScale;

                OnPropertyChanged("ChartData");
            }
        }

        
        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchXGetter { get { return (d, index) => 0; } }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (ChartHeight - Data.Count() * 20 - (Data.Count() - 1) * 10) / 2 + index * 30;
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => 25; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as Tuple<Object, Double>).Item1.ToString(); } }
        public Func<Object, Int32, Color> LegendTextColorGetter { get { return (d, index) => /*(d as Model.Bin).IsFilteredOut ? Colors.LightGray :*/ Colors.Black; } }

        public List<Color> CategoricalColors = new List<Color>()
        {
            Color.FromArgb(255, 31, 119, 180),
            Color.FromArgb(255, 255, 127, 14),
            Color.FromArgb(255, 46, 160, 44),
            Color.FromArgb(255, 214, 39, 40),
            Color.FromArgb(255, 148, 103, 189),
            Color.FromArgb(255, 140, 86, 75)
        };

        public Func<Object, Int32, Color> ColorGetter
        {
            get
            {
                return (bin, index) => CategoricalColors[index % CategoricalColors.Count]; //(bin as Model.Bin).Index % CategoricalColors.Count];
            }
        }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => xScale.RangeBand; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => (d as Tuple<Object, Double>).Item2.ToString(); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) => xScale.Map((d as Tuple<Object, Double>).Item1) - xScale.RangeBand / 2; } }
        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Double>).Item2) - 18; } }
    }
}
