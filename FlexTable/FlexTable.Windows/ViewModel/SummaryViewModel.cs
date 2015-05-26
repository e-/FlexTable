using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3;
using Windows.UI.Xaml.Shapes;

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

        public SummaryViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public d3.Selection.Data Data { get; set; }

        public void ShowSummary(Model.Column column)
        {
            Column = column;
            Double maxCount = column.Bins.Select(b => b.Count).Max();

            d3.Scale.Linear yScale = new d3.Scale.Linear(){
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
    }
}
