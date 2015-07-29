using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class PageView : UserControl
    {
        public d3.View.BoxPlot BoxPlot { get { return BoxPlotElement; } }
        public d3.View.BarChart BarChart { get { return BarChartElement; } }
        public d3.View.GroupedBarChart GroupedBarChart { get { return GroupedBarChartElement; } }
        public PivotTableView PivotTableView { get { return PivotTableViewElement; } }

        public ViewModel.PageViewModel PageViewModel { get { return this.DataContext as ViewModel.PageViewModel; } }
        public Storyboard HideStoryboard { get { return HideStoryboardElement; } }
        
        public PageView()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Show.Begin();
        }

        private void Wrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PageViewModel.Tapped(this);
        }

        public void GoDown()
        {
            GoDownStoryboard.Begin();
        }

        public void GoUp()
        {
            GoUpStoryboard.Begin();
        }

        private void GoUpStoryboard_Completed(object sender, object e)
        {
            PageViewModel.Hide();
        }
    }
}
