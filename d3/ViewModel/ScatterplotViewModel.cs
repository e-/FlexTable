﻿using System;
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

        private Linear xScale = new Linear();
        public Linear XScale { get { return xScale; } set { xScale = value; OnPropertyChanged("XScale"); } }

        private Linear yScale = new Linear();
        public Linear YScale { get { return yScale; } set { yScale = value; OnPropertyChanged("YScale"); } }

        private Dictionary<Object, Int32> colorScale = new Dictionary<Object, Int32>();
        public Dictionary<Object, Int32> ColorScale { get { return colorScale; } set { colorScale = value; OnPropertyChanged("ColorScale"); } }

        private Double height = 350;
        public Double Height { get { return height; } set { height = value; OnPropertyChanged("Height"); } }

        private Double width = 580;
        public Double Width { get { return width; } set { width = value; OnPropertyChanged("Width"); } }

        private Double chartAreaEndX;
        public Double ChartAreaEndX { get { return chartAreaEndX; } set { chartAreaEndX = value; OnPropertyChanged("ChartAreaEndX"); } }

        private Double chartAreaEndY = 300;
        public Double ChartAreaEndY { get { return chartAreaEndY; } set { chartAreaEndY = value; OnPropertyChanged("ChartAreaEndY"); } }

        public Func<Object, Int32, Double> XGetter { get { return (d, index) => xScale.Map((d as Tuple<Object, Double, Double>).Item2); } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => yScale.Map((d as Tuple<Object, Double, Double>).Item3); } }
        public Func<Object, Int32, Double> RadiusGetter { get { return (d, index) => 5; } }
        public Func<Object, Int32, Double> OpacityGetter { get { return (d, index) => 0.5; } }

        private d3.Data chartData;
        public d3.Data ChartData { get { return chartData; } }

        private IEnumerable<Tuple<Object, Double, Double>> data;
        public IEnumerable<Tuple<Object, Double, Double>> Data { get { return data; } set { data = value; } }

        private d3.Data legendData;
        public d3.Data LegendData { get { return legendData; } }

        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => 20; } }
        public Func<Object, Int32, Double> LegendPatchXGetter { get { return (d, index) => 0; } }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - colorScale.Count() * LegendPatchHeight - (colorScale.Count() - 1) * LegendPatchSpace) / 2 + index * (LegendPatchHeight + LegendPatchSpace);
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => 25; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as Tuple<Object, Int32>).Item1.ToString(); } }
        public Func<Object, Int32, Color> LegendTextColorGetter { get { return (d, index) => /*(d as Model.Bin).IsFilteredOut ? Colors.LightGray :*/ Colors.Black; } }


        public Func<Object, Int32, Color> ColorGetter
        {
            get
            {
                return (bin, index) => ColorScheme.Category10.Colors[colorScale[(bin as Tuple<Object, Double, Double>).Item1] % 10];
            }
        }

        public Func<Object, Int32, Color> LegendColorGetter
        {
            get
            {
                return (bin, index) => ColorScheme.Category10.Colors[colorScale[(bin as Tuple<Object, Int32>).Item1] % 10];
            }
        }

        private Visibility horizontalAxisVisibility;
        public Visibility HorizontalAxisVisibility { get { return horizontalAxisVisibility; } set { horizontalAxisVisibility = value; OnPropertyChanged("HorizontalAxisVisibility"); } }
        public Boolean IsHorizontalAxisVisible { get { return horizontalAxisVisibility == Visibility.Visible; } }


        private Visibility legendVisibility = Visibility.Visible;
        public Visibility LegendVisibility { get { return legendVisibility; } set { legendVisibility = value;  OnPropertyChanged("LegendVisibility"); } }
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

        public void UpdateLegendData()
        {
            colorScale = new Dictionary<Object, Int32>();
            Int32 index = 0;
            foreach (Tuple<Object, Double, Double> tuple in data)
            {
                if (!colorScale.ContainsKey(tuple.Item1))
                {
                    colorScale[tuple.Item1] = index++;
                }
            }
            ColorScale = colorScale;

            legendData = new d3.Data()
            {
                List = colorScale.Select(kv => new Tuple<Object, Int32>(kv.Key, kv.Value) as Object).ToList()
            };

            OnPropertyChanged("LegendData");
        }

        public void Update()
        {
            chartData = new d3.Data()
            {
                List = data.Select(d => d as Object).ToList()
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

            xScale = new Linear()
            {
                DomainStart = data.Select(d => d.Item2).Min(),
                DomainEnd = data.Select(d => d.Item2).Max(),
                RangeStart = VerticalAxisCanvasLeft,
                RangeEnd = ChartAreaEndX + PaddingLeft
            };
            xScale.Nice();

            XScale = xScale;

            yScale = new Linear()
            {
                DomainStart = data.Select(d => d.Item3).Min(),
                DomainEnd = data.Select(d => d.Item3).Max(),
                RangeStart = ChartAreaEndY,
                RangeEnd = PaddingTop
            };
            yScale.Nice();

            YScale = yScale;

            UpdateLegendData();
            OnPropertyChanged("ChartData");
        }
    }
}