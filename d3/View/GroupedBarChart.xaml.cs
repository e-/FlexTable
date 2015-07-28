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
    public sealed partial class GroupedBarChart : UserControl
    {
        ViewModel.GroupedBarChartViewModel viewModel = new ViewModel.GroupedBarChartViewModel();

        public IEnumerable<Tuple<Object, Object, Double>> Data
        {
            set
            {
                viewModel.Data = value;
            }
        }

        public GroupedBarChart()
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
    }
}
