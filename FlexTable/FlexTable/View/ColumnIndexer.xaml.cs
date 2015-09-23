using System.Diagnostics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using FlexTable.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using System;
using Windows.UI;
using Windows.UI.Xaml.Media;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class ColumnIndexer : UserControl
    {
        public ColumnIndexer()
        {
            this.InitializeComponent();
        }

        private void Border_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if(e.GetCurrentPoint(this).PointerDevice.PointerDeviceType != PointerDeviceType.Touch) return; 
            Point point = e.GetCurrentPoint(this).Position;
            (DataContext as TableViewModel).IndexColumn(e.GetCurrentPoint(this).PointerId, point.Y);
        }

        private void Border_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).PointerDevice.PointerDeviceType != PointerDeviceType.Touch) return;
            (DataContext as TableViewModel).CancelIndexing();
            ShowHelperStoryboard.Pause();
            HideHelperStoryboard.Begin();
        }

        private void Border_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).PointerDevice.PointerDeviceType != PointerDeviceType.Touch) return;
            (DataContext as TableViewModel).CancelIndexing();
            ShowHelperStoryboard.Pause();
            HideHelperStoryboard.Begin();
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).PointerDevice.PointerDeviceType != PointerDeviceType.Touch) return;
            (DataContext as TableViewModel).CancelIndexing();
            ShowHelperStoryboard.Pause();
            HideHelperStoryboard.Begin();
        }
        
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).PointerDevice.PointerDeviceType != PointerDeviceType.Touch) return;
            Point point = e.GetCurrentPoint(this).Position;
            (DataContext as TableViewModel).IndexColumn(e.GetCurrentPoint(this).PointerId, point.Y);
            Update();
            HideHelperStoryboard.Pause();
            ShowHelperStoryboard.Begin();
        }

        public void HideHelper()
        {
            HideHelperStoryboard.Begin();
        }

        public void Update() // 처음에 초기화하거나 컬럼의 순서가 바뀌면 이게 호출되어야함
        {
            TableViewModel tvm = this.DataContext as TableViewModel;
            List<ColumnViewModel> sorted = tvm.SheetViewModel.ColumnViewModels.OrderBy(cvm => cvm.Order).ToList();
            Int32 index = 0;

            foreach (ColumnViewModel cvm in sorted)
            {
                Border border = null;
                TextBlock textBlock = null;

                if (IndexHelperWrapperElement.Children.Count > index)
                {
                    border = IndexHelperWrapperElement.Children[index] as Border;
                    textBlock = border.Child as TextBlock;
                }
                else
                {
                    border = new Border()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    textBlock = new TextBlock()
                    {
                        Style = this.Resources["LabelStyle"] as Style
                    };

                    border.Child = textBlock;
                    IndexHelperWrapperElement.Children.Add(border);
                }


                textBlock.Text = cvm.Column.Name;
                textBlock.Measure(new Size(Double.MaxValue, Double.MaxValue));
                Double perHeight = (tvm.SheetViewHeight + 10) / sorted.Count;
                if (textBlock.ActualWidth <= perHeight)
                {
                    textBlock.Width = (Double)App.Current.Resources["RowHeaderWidth"] - 1;
                    textBlock.TextAlignment = TextAlignment.Center;
                }
                else
                {
                    textBlock.Width = 1000;
                    textBlock.TextAlignment = TextAlignment.Left;
                }

                border.Height = perHeight;
                border.Width = (Double)App.Current.Resources["RowHeaderWidth"] - 1;
                border.Background = index % 2 == 0 ? (App.Current.Resources["GridLineBrush"] as SolidColorBrush) : (new SolidColorBrush(Colors.White)); 
                index++;
            }

            for (Int32 j = IndexHelperWrapperElement.Children.Count - 1; j >= index; --j)
            {
                IndexHelperWrapperElement.Children.RemoveAt(j);
            }

            //tvm.SheetViewModel.ColumnViewModels.Or
        }
    }
}
