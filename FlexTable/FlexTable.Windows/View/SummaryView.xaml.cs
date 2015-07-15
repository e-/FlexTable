using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class SummaryView : UserControl
    {
        public SummaryView()
        {
            this.InitializeComponent();
            Drawable drawable = new Drawable();
            drawable.Attach(Wrapper, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += RecognizeStrokes;
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.SummaryViewModel svm = this.DataContext as ViewModel.SummaryViewModel;
            
            //svm.IsSelected = true;
            svm.MainPageViewModel.GroupBy(svm.Column);
            svm.MainPageViewModel.CancelIndexing();
        }

        void RecognizeStrokes(InkManager inkManager)
        {
            IReadOnlyList<InkStroke> strokes = inkManager.GetStrokes();
            ViewModel.SummaryViewModel svm = this.DataContext as ViewModel.SummaryViewModel;

            if (strokes.Count == 0) return;
            if (strokes.Count >= 2) return;

            InkStroke stroke = strokes.First();

            if (stroke.BoundingRect.Height < 30 && stroke.BoundingRect.Width > 30)
            {
                svm.StrokeAdded(stroke);
            }
            return;
        }
    }
}
