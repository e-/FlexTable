using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace FlexTable.ViewModel
{
    public class ChartViewModel : NotifyViewModel
    {
        ViewModel.MainPageViewModel mainPageViewModel;
        public ViewModel.MainPageViewModel MainPageViewModel { 
            get { return mainPageViewModel; } 
        }

        private d3.Scale.ScaleBase xScale;
        public d3.Scale.ScaleBase XScale {
            get { return xScale; }
            set
            {
                xScale = value;
                OnPropertyChanged("XScale");
            }
        }

        private d3.Scale.ScaleBase yScale;
        public d3.Scale.ScaleBase YScale
        {
            get { return yScale; }
            set
            {
                yScale = value;
                OnPropertyChanged("YScale");
            }
        }

        public Double ChartHeight { get { return 400; } }
        public Double ChartWidth { get { return 600; } }

        public Func<Object, Int32, Double> WidthGetter { get { return (d, index) => 50; } }
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => ChartHeight - yScale.Map((d as Tuple<String, Double>).Item2); } }
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => xScale.Map((d as Tuple<String, Double>).Item1) - 25; } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Tuple<String, Double>).Item2); } }
        private d3.Selection.Data data;
        public d3.Selection.Data Data { get { return data; } set { data = value; OnPropertyChanged("Data"); } }

        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchXGetter { get { return (d, index) => 0; } }
        public Func<Object, Int32, Double> LegendPatchYGetter { get { 
            return (d, index) => (ChartHeight - LegendData.Real.Count * 20 - (LegendData.Real.Count - 1) * 10) / 2 + index * 30; 
        } }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => 25; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as Model.Bin).Name; } }
        public Func<Object, Int32, Color> LegendTextColorGetter { get { return (d, index) => (d as Model.Bin).IsFilteredOut ? Colors.LightGray : Colors.Black; } }

        private d3.Selection.Data legendData;
        public d3.Selection.Data LegendData { get { return legendData; } set { legendData = value; OnPropertyChanged("LegendData"); } }
        
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
                return (bin, index) => CategoricalColors[index % CategoricalColors.Count];
            }
        }

        private Boolean isChartVisible;
        public Boolean IsChartVisible { get { return isChartVisible; } set { isChartVisible = value; OnPropertyChanged("IsChartVisible"); } }

        private Model.AggregationType aggregationType;
        public Model.AggregationType AggregationType { get { return aggregationType; } set { aggregationType = value; OnPropertyChanged("AggregationType"); } }

        private Model.Column column;
        public Model.Column Column { get { return column; } set { column = value; OnPropertyChanged("Column"); } }

        private Model.Column groupedColumn;
        public Model.Column GroupedColumn { get { return groupedColumn; } set { groupedColumn = value; OnPropertyChanged("GroupedColumn"); } }

        public ChartViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void Draw(Model.Column groupedBy, IEnumerable<Tuple<String, Double>> values, Model.Column column)
        {
            IsChartVisible = true;
            Column = column;
            AggregationType = column.AggregationType;
            GroupedColumn = groupedBy;

            Double maxValue = values.Select(d => d.Item2).Max();

            d3.Scale.Linear yScale = new d3.Scale.Linear()
            {
                DomainStart = 0,
                DomainEnd = Math.Round(maxValue * 1.2),
                RangeStart = ChartHeight,
                RangeEnd = 50
            };
            YScale = yScale;

            d3.Scale.Ordinal xScale = new d3.Scale.Ordinal()
            {
                RangeStart = 50,
                RangeEnd = ChartWidth
            };

            foreach (Tuple<String, Double> value in values)
            {
                xScale.Domain.Add(value.Item1);
            }
            XScale = xScale;

            Data = new d3.Selection.Data()
            {
                Real = values.Select(b => b as Object).ToList()
            };

            LegendData = new d3.Selection.Data()
            {
                Real = groupedBy.Bins.Select(b => b as Object).ToList()
            };
        }

        public void Hide()
        {
            IsChartVisible = false;
        }
    }
}
