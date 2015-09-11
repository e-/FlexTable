using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3.Scale;
using Windows.UI;
using Windows.UI.Xaml;

namespace d3.ViewModel
{
    class ScatterplotViewModel : Notifiable
    {
        private Linear xScale = new Linear();
        public Linear XScale { get { return xScale; } set { xScale = value; OnPropertyChanged("XScale"); } }

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
                if (IsLegendVisible)
                {
                    LegendAreaWidth = 140;
                }
                else
                {
                    LegendAreaWidth = 0;
                }

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

        public Func<Object, Int32, Double> XGetter { get { return (d, index) => xScale.Map((d as Tuple<Object, Double, Double>).Item2); } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Double, Double>).Item3); } }
        public Func<Object, Int32, Double> RadiusGetter { get { return (d, index) => 5; } }
        public Func<Object, Int32, Double> OpacityGetter { get { return (d, index) => 0.5; } }

        private d3.Data chartData;
        public d3.Data ChartData { get { return chartData; } }

        private IEnumerable<Tuple<Object, Double, Double>> data;
        public IEnumerable<Tuple<Object, Double, Double>> Data { get { return data; } set { data = value; } }

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
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as Tuple<Object, Double, Double>).Item1.ToString(); } }
        public Func<Object, Int32, Color> LegendTextColorGetter { get { return (d, index) => /*(d as Model.Bin).IsFilteredOut ? Colors.LightGray :*/ Colors.Black; } }


        public Func<Object, Int32, Color> ColorGetter
        {
            get
            {
                return (bin, index) => ColorScheme.Category10.Colors.First();
            }
        }

        private Visibility legendVisibility;
        public Visibility LegendVisibility { get { return legendVisibility; } set { legendVisibility = value; OnPropertyChanged("LegendVisibility"); } }
        public Boolean IsLegendVisible { get { return legendVisibility == Visibility.Visible; } }

        public void Update()
        {
            chartData = new d3.Data()
            {
                List = data.Select(d => d as Object).ToList()
            };

            xScale = new Linear()
            {
                DomainStart = data.Select(d => d.Item2).Min(),
                DomainEnd = data.Select(d => d.Item2).Max(),
                RangeStart = 50,
                RangeEnd = chartAreaWidth
            };
            xScale.Nice();

            XScale = xScale;

            yScale = new Linear()
            {
                DomainStart = data.Select(d => d.Item3).Min(),
                DomainEnd = data.Select(d => d.Item3).Max(),
                RangeStart = HorizontalAxisCanvasTop,
                RangeEnd = 50
            };
            yScale.Nice();

            YScale = yScale;            

            OnPropertyChanged("ColorGetter");
            OnPropertyChanged("ChartData");
        }
    }
}
