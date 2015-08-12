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
        Drawable drawable = new Drawable();

        public Canvas AllRowsTableCanvas { get { return AllRowsCanvasElement; } }
        public Canvas TableCanvas { get { return TableCanvasElement; } }
        public Grid ScrollViewerContentWrapper { get { return ScrollViewerContentWrapperElement; } }
        public ColumnHeaderPresenter TopColumnHeader { get { return TopColumnHeaderElement; } }
        public ColumnHeaderPresenter BottomColumnHeader { get { return BottomColumnHeaderElement; } }
        public RowHeaderPresenter RowHeaderPresenter { get { return RowHeaderPresenterElement; } }
        public ScrollViewer TableScrollViewer { get { return TableScrollViewerElement; } }

        public TableView()
        {
            this.InitializeComponent();
            
            drawable.Attach(SheetView, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += RecognizeStrokes;
        }

        public void AddGuidelines(Int32 count)
        {
            if (count < 50) count = 50;

            for (Int32 i = 0; i < count - 1; ++i)
            {
                Rectangle rectangle = new Rectangle()
                {
                    Style = (Style)App.Current.Resources["RowGuidelineStyle" + (i % 2).ToString()]
                };
                GuildlineElement.Children.Add(rectangle);
            }
        }

        /*async*/ void RecognizeStrokes(InkManager inkManager)
        {
            /*try
            {
                IReadOnlyList<InkStroke> strokes = inkManager.GetStrokes();
                Double centerX = strokes[0].BoundingRect.X + strokes[0].BoundingRect.Width / 2 -
                    (Double)App.Current.Resources["RowHeaderWidth"] + TableScrollViewer.HorizontalOffset;
                ViewModel.MainPageViewModel mainPageViewModel = this.DataContext as ViewModel.MainPageViewModel;

                Int32 columnIndex = -1, index = 0;
                foreach (Model.Column column in mainPageViewModel.Sheet.Columns)
                {
                    if (column.X <= centerX && centerX < column.X + column.Width)
                    {
                        columnIndex = index;
                        break;
                    }
                    index++;
                }


                IReadOnlyList<InkRecognitionResult> results = await inkManager.RecognizeAsync(InkRecognitionTarget.Recent);

                foreach (InkRecognitionResult result in results)
                {
                    foreach (String candidate in result.GetTextCandidates())
                    {
                        Debug.WriteLine(candidate);

                        if (candidate == "a" || candidate == "A")
                        {
                            //mainPageViewModel.ChangeAggregationType(columnIndex, Model.AggregationType.Average);
                            drawable.RemoveAllStrokes();
                            return;
                        }

                        if (candidate == "m" || candidate == "M")
                        {
                            //mainPageViewModel.ChangeAggregationType(columnIndex, Model.AggregationType.Maximum);
                            drawable.RemoveAllStrokes();
                            return;
                        }

                        if (candidate == "v" || candidate == "V")
                        {
                            mainPageViewModel.DrawChart(columnIndex);
                            drawable.RemoveAllStrokes();
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            drawable.RemoveAllStrokes();*/
        }

        public void ScrollToColumnViewModel(ViewModel.ColumnViewModel columnViewModel)
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
            ViewModel.TableViewModel tableViewModel = this.DataContext as ViewModel.TableViewModel;
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

            ShowAllRowsCanvasStoryboard.Begin();
            HideTableCanvasStoryboard.Begin();
        }

        public void ShowTableCanvas()
        {
            TableCanvasElement.Visibility = Visibility.Visible;

            ShowTableCanvasStoryboard.Begin();
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
