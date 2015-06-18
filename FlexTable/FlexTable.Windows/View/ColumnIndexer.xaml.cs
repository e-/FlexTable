using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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
            Point point = e.GetCurrentPoint(this).Position;
            (DataContext as ViewModel.MainPageViewModel).IndexColumn(point.Y);
        }

        private void Border_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            (DataContext as ViewModel.MainPageViewModel).CancelIndexing();
        }

        private void Border_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            (DataContext as ViewModel.MainPageViewModel).CancelIndexing();
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            (DataContext as ViewModel.MainPageViewModel).CancelIndexing();
        }

        private void Border_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Pressed");
            Point point = e.GetCurrentPoint(this).Position;
            (DataContext as ViewModel.MainPageViewModel).IndexColumn(point.Y);
        }
    }
}
