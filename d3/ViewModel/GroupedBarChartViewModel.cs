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
        private d3.Scale.Ordinal xScale;
        public d3.Scale.Ordinal XScale
        {
            get { return xScale; }
            set
            {
                xScale = value;
                OnPropertyChanged("XScale");
            }
        }

        private d3.Scale.Linear yScale;
        public d3.Scale.Linear YScale
        {
            get { return yScale; }
            set
            {
                yScale = value;
                OnPropertyChanged("YScale");
            }
        }

        public Double ChartHeight { get { return 350; } }
        public Double ChartWidth { get { return 580; } }
        public Double BarWidth { get { return 30; } }

        public Func<Object, Int32, Double> WidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => ChartHeight - yScale.Map((d as Tuple<Object, Object, Double>).Item3); } }
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => 
            xDictionary[(d as Tuple<Object, Object, Double>).Item1 as String].Map((d as Tuple<Object, Object, Double>).Item2 as String) - BarWidth / 2; 
        } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Object, Double>).Item3); } }

        private Dictionary<String, Scale.Ordinal> xDictionary = new Dictionary<String, Scale.Ordinal>();
        private List<Object> secondaryKeys = new List<Object>();

        private d3.Data legendData;
        public d3.Data LegendData { get { return legendData; } }

        private IEnumerable<Tuple<Object, Object, Double>> data;
        public IEnumerable<Tuple<Object, Object, Double>> Data
        {
            get { return data; }
            set
            {
                data = value;
                chartData = new d3.Data()
                {
                    List = data.Select(d => d as Object).ToList()
                };

                yScale = new d3.Scale.Linear()
                {
                    DomainStart = 0,
                    DomainEnd = data.Select(d => d.Item3).Max(),
                    RangeStart = ChartHeight,
                    RangeEnd = 50
                };

                yScale.Nice();

                YScale = yScale;

                xScale = new d3.Scale.Ordinal()
                {
                    RangeStart = 50,
                    RangeEnd = ChartWidth
                };

                xDictionary.Clear();

                foreach (String category1 in data.Select(d => d.Item1).Distinct())
                {
                    xScale.Domain.Add(category1);
                }

                XScale = xScale;

                secondaryKeys.Clear();
                foreach (String category2 in data.Select(d => d.Item2).Distinct())
                {
                    secondaryKeys.Add(category2);
                }
                
                foreach (String category1 in xScale.Domain)
                {
                    Int32 count = data.Where(d => d.Item1.ToString() == category1).Select(d => d.Item2).Distinct().Count();

                    Ordinal ordinal = new Ordinal()
                    {
                        RangeStart = xScale.Map(category1) - BarWidth * count / 2,
                        RangeEnd = xScale.Map(category1) + BarWidth * count / 2
                    };

                    foreach (String category2 in data.Where(d => d.Item1.ToString() == category1).Select(d => d.Item2).Distinct())
                    {
                        ordinal.Domain.Add(category2);
                    }
                    xDictionary.Add(category1, ordinal);
                }

                legendData = new d3.Data()
                {
                    List = secondaryKeys
                };

                OnPropertyChanged("Data");
                OnPropertyChanged("ChartData");
                OnPropertyChanged("LegendData");
            }
        }

        private d3.Data chartData;
        public d3.Data ChartData { get { return chartData; } }

        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchXGetter { get { return (d, index) => 0; } }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (ChartHeight - LegendData.List.Count() * 20 - (LegendData.List.Count() - 1) * 10) / 2 + index * 30;
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => 25; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => d.ToString(); } }
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
                return (d, index) => CategoricalColors[
                    secondaryKeys.IndexOf((d as Tuple< Object, Object, Double>).Item2) % CategoricalColors.Count
                    ]; //(bin as Model.Bin).Index % CategoricalColors.Count];
            }
        }

        public Func<Object, Int32, Color> LegendColorGetter
        {
            get
            {
                return (d, index) => CategoricalColors[
                    secondaryKeys.IndexOf(d) % CategoricalColors.Count
                    ]; //(bin as Model.Bin).Index % CategoricalColors.Count];
            }
        }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => (d as Tuple<Object, Object, Double>).Item3.ToString(); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) =>
            xDictionary[(d as Tuple<Object, Object, Double>).Item1 as String].Map((d as Tuple<Object, Object, Double>).Item2 as String) - BarWidth / 2; 
        } }
        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Object, Double>).Item3) - 18; } }
    }
}
