using FlexTable.Util;
using FlexTable.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Shapes;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class TableView : UserControl
    {
        Drawable drawable = new Drawable()
        {
            IgnoreSmallStrokes = false
        };

        public Canvas AllRowsTableCanvas => AllRowsCanvasElement;
        public Canvas TableCanvas { get { return TableCanvasElement; } }
        public Grid ScrollViewerContentWrapper { get { return ScrollViewerContentWrapperElement; } }
        public ColumnHeaderPresenter TopColumnHeader { get { return TopColumnHeaderElement; } }
        public ColumnHeaderPresenter BottomColumnHeader { get { return BottomColumnHeaderElement; } }
        public RowHeaderPresenter RowHeaderPresenter { get { return RowHeaderPresenterElement; } }
        public ScrollViewer TableScrollViewer { get { return TableScrollViewerElement; } }
        public ColumnIndexer ColumnIndexer { get { return ColumnIndexerElement; } }
        public ColumnHighlighter ColumnHighlighter { get { return ColumnHighlighterElement; } }

        DispatcherTimer timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        InkManager inkManager;

        public TableView()
        {
            this.InitializeComponent();
            
            drawable.Attach(SheetView, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += RecognizeStrokes;
            timer.Tick += timer_Tick;
        }

        async void timer_Tick(object sender, object e)
        {
            timer.Stop();
            if (this.inkManager == null)
                return;

            IReadOnlyList<InkStroke> strokes = this.inkManager.GetStrokes();
            Double centerX = strokes[0].BoundingRect.X + strokes[0].BoundingRect.Width / 2 -
                (Double)App.Current.Resources["RowHeaderWidth"] + TableScrollViewer.HorizontalOffset;
            TableViewModel tableViewModel = this.DataContext as TableViewModel;
            ColumnViewModel selectedColumnViewModel = null;

            if (strokes[0].BoundingRect.Y < 100)
            {
                foreach (ColumnViewModel columnViewModel in tableViewModel.SheetViewModel.ColumnViewModels)
                {
                    if (columnViewModel.X <= centerX && centerX < columnViewModel.X + columnViewModel.Width)
                    {
                        selectedColumnViewModel = columnViewModel;
                        break;
                    }
                }
            }

            IReadOnlyList<InkRecognitionResult> results = await this.inkManager.RecognizeAsync(InkRecognitionTarget.Recent);

            foreach (InkRecognitionResult result in results)
            {
                foreach (String candidate in result.GetTextCandidates())
                {
                    String upperCandidate = candidate.ToUpper();

                    if (selectedColumnViewModel != null && selectedColumnViewModel.Type == Model.ColumnType.Numerical
                        && tableViewModel.MainPageViewModel.ExplorationViewModel.ViewStatus.SelectedColumnViewModels.Count > 0)
                    {
                        switch (upperCandidate)
                        {
                            case "MIN":
                                selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.MinAggregation();
                                tableViewModel.OnAggregativeFunctionChanged(selectedColumnViewModel);
                                Debug.WriteLine("Min selected");
                                drawable.RemoveAllStrokes();
                                timer.Stop();
                                return;
                            case "MAX":
                                selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.MaxAggregation();
                                tableViewModel.OnAggregativeFunctionChanged(selectedColumnViewModel);
                                Debug.WriteLine("Max selected");
                                drawable.RemoveAllStrokes();
                                timer.Stop();
                                return;
                            case "AVG":
                            case "MEAN":
                                selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.AverageAggregation();
                                tableViewModel.OnAggregativeFunctionChanged(selectedColumnViewModel);
                                Debug.WriteLine("Mean selected");
                                drawable.RemoveAllStrokes();
                                timer.Stop();
                                return;
                            case "SUM":
                                selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.SumAggregation();
                                tableViewModel.OnAggregativeFunctionChanged(selectedColumnViewModel);
                                Debug.WriteLine("Sum selected");
                                drawable.RemoveAllStrokes();
                                timer.Stop();
                                return;
                        }
                    }
                }
            }
            drawable.RemoveAllStrokes();
        }

        public void AddGuidelines(Int32 count)
        {
            GuidelineElement.Children.Clear();
            if (count < 50) count = 50;

            for (Int32 i = 0; i < count - 1; ++i)
            {
                Rectangle rectangle = new Rectangle()
                {
                    Style = (Style)App.Current.Resources["RowGuidelineStyle" + (i % 2).ToString()]
                };
                GuidelineElement.Children.Add(rectangle);
            }
        }

        void RecognizeStrokes(InkManager inkManager)
        {
            timer.Start();
            this.inkManager = inkManager;
        }

        public void ScrollToColumnViewModel(ColumnViewModel columnViewModel)
        {
            TableViewModel tableViewModel = this.DataContext as TableViewModel;
            Double offset = TableScrollViewerElement.HorizontalOffset,
                   width = tableViewModel.SheetViewWidth,
                   x1 = columnViewModel.X,
                   x2 = columnViewModel.X + columnViewModel.Width;

            Double? to = null;

            if (x1 < offset)
            {
                to = x1 - 20;
            }
            else if (offset + width < x2)
            {
                to = x2 - width + 20;
            }

            if (to < 0) to = 0;

            TableScrollViewerElement.ChangeView(to, null, null);

            if (to == null)
            {
                tableViewModel.ScrollLeft = TableScrollViewerElement.HorizontalOffset;
            }
            else
            {
                tableViewModel.ScrollLeft = (Double)to;
            }
        }


        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            TableViewModel tableViewModel = this.DataContext as TableViewModel;
            ScrollViewer sv = sender as ScrollViewer;

            RowHeaderPresenterElement.VerticalOffset = sv.VerticalOffset;
            TopColumnHeader.HorizontalOffset = sv.HorizontalOffset;
            BottomColumnHeader.HorizontalOffset = sv.HorizontalOffset;

            tableViewModel.ScrollTop = sv.VerticalOffset;
            tableViewModel.ScrollLeft = sv.HorizontalOffset;
        }

        
        public void ShowAllRowsCanvas()
        {
            AllRowsCanvasElement.Visibility = Visibility.Visible;

            HideAllRowsCanvasStoryboard.Pause();
            ShowAllRowsCanvasStoryboard.Begin();
            ShowTableCanvasStoryboard.Pause();
            HideTableCanvasStoryboard.Begin();
        }

        public void ShowTableCanvas()
        {
            TableCanvasElement.Visibility = Visibility.Visible;

            HideTableCanvasStoryboard.Pause();
            ShowTableCanvasStoryboard.Begin();
            ShowAllRowsCanvasStoryboard.Pause();
            HideAllRowsCanvasStoryboard.Begin();            
        }

        private void HideAllRowsCanvasStoryboard_Completed(object sender, object e)
        {
            AllRowsCanvasElement.Visibility = Visibility.Collapsed;
        }

        private void HideTableCanvasStoryboard_Completed(object sender, object e)
        {
            TableCanvas.Visibility = Visibility.Collapsed;
        }
    }
}
