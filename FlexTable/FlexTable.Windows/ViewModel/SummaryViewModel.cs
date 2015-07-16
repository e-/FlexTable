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
    public class SummaryViewModel : NotifyViewModel
    {
        private Model.Column column;
        public Model.Column Column { get { return column; } set { column = value; OnPropertyChanged("Column"); } }

        ViewModel.MainPageViewModel mainPageViewModel;
        public ViewModel.MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

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
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => ChartHeight - yScale.Map((d as Model.Bin).Count); } }
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => xScale.Map((d as Model.Bin).Name) - 25; } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Model.Bin).Count); } }
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
        
        /*public List<Color> CategoricalColors = new List<Color>()
        {
            Color.FromArgb(255, 141, 211, 199),
            Color.FromArgb(255, 255, 255, 179),
            Color.FromArgb(255, 190, 186, 218),
            Color.FromArgb(255, 251, 128, 114),
            Color.FromArgb(255, 128, 177, 211),
            Color.FromArgb(255, 253, 180, 098),
            Color.FromArgb(255, 179, 222, 105),
            Color.FromArgb(255, 252, 205, 229),
            Color.FromArgb(255, 217, 217, 217),
            Color.FromArgb(255, 199, 128, 189)
        };*/

        public List<Color> CategoricalColors = new List<Color>()
        {
            Color.FromArgb(255, 31, 119, 180),
            Color.FromArgb(255, 255, 127, 14),
            Color.FromArgb(255, 46, 160, 44),
            Color.FromArgb(255, 214, 39, 40),
            Color.FromArgb(255, 148, 103, 189),
            Color.FromArgb(255, 140, 86, 75),
            /*Color.FromArgb(255, 179, 222, 105),
            Color.FromArgb(255, 252, 205, 229),
            Color.FromArgb(255, 217, 217, 217),
            Color.FromArgb(255, 199, 128, 189)*/
        };
        
        public Func<Object, Int32, Color> ColorGetter
        {
            get
            {
                return (bin, index) => CategoricalColors[(bin as Model.Bin).Index % CategoricalColors.Count];
            }
        }

        private Boolean isHistogramVisible;
        public Boolean IsHistogramVisible { get { return isHistogramVisible; } set { isHistogramVisible = value; OnPropertyChanged("IsHistogramVisible"); } }

        private Boolean isStatisticalSummaryVisible;
        public Boolean IsStatisticalSummaryVisible { get { return isStatisticalSummaryVisible; } set { isStatisticalSummaryVisible = value; OnPropertyChanged("IsStatisticalSummaryVisible"); } }

        private Boolean isSelected = false;
        public Boolean IsSelected { get { return isSelected; } set { isSelected = value; OnPropertyChanged("IsSelected"); } }

        public SummaryViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void ShowSummary(Model.Column column)
        {
            /*Column = column;
            IsHistogramVisible = column.Type == Model.ColumnType.Categorical;
            IsStatisticalSummaryVisible = column.Type == Model.ColumnType.Numerical;

            if (IsHistogramVisible)
            {
                Double maxCount = column.Bins.Select(b => b.Count).Max();

                d3.Scale.Linear yScale = new d3.Scale.Linear()
                {
                    DomainStart = 0,
                    DomainEnd = Math.Round(maxCount * 1.2),
                    RangeStart = ChartHeight,
                    RangeEnd = 50
                };
                YScale = yScale;

                d3.Scale.Ordinal xScale = new d3.Scale.Ordinal()
                {
                    RangeStart = 50,
                    RangeEnd = ChartWidth
                };
                foreach (Model.Bin bin in column.Bins.Where(b => !b.IsFilteredOut)) { xScale.Domain.Add(bin.Name); }
                XScale = xScale;
                

                Data = new d3.Selection.Data()
                {
                    Real = column.Bins.Where(b => !b.IsFilteredOut).Select(b => b as Object).ToList()
                };

                LegendData = new d3.Selection.Data()
                {
                    Real = column.Bins.Select(b => b as Object).ToList()
                };
            }
            else
            {
                
            }*/
        }

        public void Hide()
        {
            if (!IsSelected)
            {
                IsHistogramVisible = false;
                IsStatisticalSummaryVisible = false;
            }
        }

        public void StrokeAdded(InkStroke stroke)
        {
            /*
            Int32 index = 0;
            Rect rect = stroke.BoundingRect;
            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            foreach (Model.Bin bin in column.Bins.Select(b => b as Object).ToList())
            {
                Double x0 = LegendTextXGetter(bin, index),
                       y0 = LegendPatchYGetter(bin, index) + 10,
                       y1 = y0 + LegendPatchHeightGetter(bin, index) + 10;

                if (x0 <= center.X - mainPageViewModel.Width / 2 + ChartWidth && y0 <= center.Y && center.Y <= y1)
                {
                    bin.IsFilteredOut = !bin.IsFilteredOut;
                    break;
                }             
                index++;
            }

            d3.Scale.Ordinal xScale = new d3.Scale.Ordinal()
            {
                RangeStart = 70,
                RangeEnd = ChartWidth
            };
            foreach (Model.Bin bin in column.Bins.Where(b => !b.IsFilteredOut)) { xScale.Domain.Add(bin.Name); }
            XScale = xScale;

            Data = new d3.Selection.Data()
            {
                Real = column.Bins.Where(b => !b.IsFilteredOut).Select(b => b as Object).ToList()
            };

            LegendData = new d3.Selection.Data()
            {
                Real = column.Bins.Select(b => b as Object).ToList()
            };

            mainPageViewModel.UpdateFiltering();*/
        }
    }
}
