using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

namespace FlexTable.ViewModel
{
    public class SummaryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Model.Column column;
        public Model.Column Column { get { return column; } set { column = value; OnPropertyChanged("Column"); } }

        ViewModel.MainPageViewModel mainPageViewModel;

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

        public Func<Object, Double> WidthGetter { get { return d => 50; } }
        public Func<Object, Double> HeightGetter { get { return d => 400 - yScale.Map((d as Model.Bin).Count); } }
        public Func<Object, Double> XGetter { get { return d => xScale.Map((d as Model.Bin).Name) - 25; } }
        public Func<Object, Double> YGetter { get { return d => yScale.Map((d as Model.Bin).Count); } }
        /*
         * .Set3 .q0-10{fill:rgb(141,211,199)}
   .Set3 .q1-10{fill:rgb(255,255,179)}
   .Set3 .q2-10{fill:rgb(190,186,218)}
   .Set3 .q3-10{fill:rgb(251,128,114)}
   .Set3 .q4-10{fill:rgb(128,177,211)}
   .Set3 .q5-10{fill:rgb(253,180,98)}
   .Set3 .q6-10{fill:rgb(179,222,105)}
   .Set3 .q7-10{fill:rgb(252,205,229)}
   .Set3 .q8-10{fill:rgb(217,217,217)}
   .Set3 .q9-10{fill:rgb(188,128,189)}
         * 
         * */

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
         /*
        1f77b4 #1f77b4
   ff7f0e #ff7f0e
   2ca02c #2ca02c
   d62728 #d62728
   9467bd #9467bd
   8c564b #8c564b
   e377c2 #e377c2
   7f7f7f #7f7f7f
   bcbd22 #bcbd22
   17becf #17becf
       */
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
                return (_, index) => CategoricalColors[index % CategoricalColors.Count];
            }
        }

        public Boolean IsHistogramVisible { get; set; }

        public SummaryViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public d3.Selection.Data Data { get; set; }

        public void ShowSummary(Model.Column column)
        {
            Column = column;
            IsHistogramVisible = column.Type == Model.ColumnType.Categorical;

            if (IsHistogramVisible)
            {
                Double maxCount = column.Bins.Select(b => b.Count).Max();

                d3.Scale.Linear yScale = new d3.Scale.Linear()
                {
                    DomainStart = 0,
                    DomainEnd = Math.Round(maxCount * 1.2),
                    RangeStart = 400,
                    RangeEnd = 10
                };
                YScale = yScale;

                d3.Scale.Ordinal xScale = new d3.Scale.Ordinal()
                {
                    RangeStart = 70,
                    RangeEnd = 700
                };
                foreach (Model.Bin bin in column.Bins) { xScale.Domain.Add(bin.Name); }
                XScale = xScale;

                Int32 index = 0;
                foreach (Model.Bin bin in column.Bins)
                {
                    Rectangle rect = new Rectangle()
                    {
                        Width = 50,
                        Height = 400 - yScale.Map(bin.Count)
                    };
                    ++index;
                }

                Data = new d3.Selection.Data()
                {
                    Real = column.Bins.Select(b => b as Object).ToList()
                };

                OnPropertyChanged("Data");
            }
            else
            {

            }
            OnPropertyChanged("IsHistogramVisible");
        }
    }
}
