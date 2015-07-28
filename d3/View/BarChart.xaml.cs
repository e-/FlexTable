using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace d3.View
{
    public sealed partial class BarChart : UserControl
    {
        ViewModel.BarChartViewModel viewModel = new ViewModel.BarChartViewModel();

        public IEnumerable<Tuple<Object, Double>> Data
        {
            set
            {
                viewModel.Data = value;
            }
        }

        public BarChart()
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        public void Update()
        {
            RectangleElement.Update();
            IndicatorTextElement.Update();
            HorizontalAxis.Update();
            VerticalAxis.Update();

            LegendRectangleElement.Update();
            LegendTextElement.Update();
        }

        /*private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Double maxCount = columnViewModel.Bins.Select(b => b.Count).Max();

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
            foreach (Model.Bin bin in columnViewModel.Bins.Where(b => !b.IsFilteredOut)) { xScale.Domain.Add(bin.Name); }
            XScale = xScale;


            Data = new d3.Selection.Data()
            {
                Real = columnViewModel.Bins.Where(b => !b.IsFilteredOut).Select(b => b as Object).ToList()
            };

            LegendData = new d3.Selection.Data()
            {
                Real = columnViewModel.Bins.Select(b => b as Object).ToList()
            };
        }*/
    }
}
