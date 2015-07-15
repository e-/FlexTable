using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace FlexTable
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page, IMainPage
    {
        ViewModel.MainPageViewModel mainPageViewModel;
        Util.CsvLoader csvLoader = new Util.CsvLoader();
        Drawable drawable = new Drawable();

        public MainPage()
        {
            mainPageViewModel = new ViewModel.MainPageViewModel(this);
            this.DataContext = mainPageViewModel;
            this.InitializeComponent();
            
            drawable.Attach(SheetView, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += RecognizeStrokes;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Model.Sheet sheet = await csvLoader.Load();
            sheet.MeasureColumnWidth(DummyCell);
            sheet.UpdateColumnX();
            sheet.GuessColumnType();
            sheet.CreateColumnSummary();
            mainPageViewModel.Sheet = sheet;

            for (Int32 i = 0; i < sheet.RowCount - 1; ++i)
            {
                RowDefinition rd = new RowDefinition()
                {
                    Height = new GridLength((Double)App.Current.Resources["RowHeight"])
                };
                GuidelineGrid.RowDefinitions.Add(rd);

                Rectangle rectangle = new Rectangle()
                {
                    Style = (Style)App.Current.Resources["RowGuidelineStyle" + (i%2).ToString()]
                };
                Grid.SetRow(rectangle, i);

                GuidelineGrid.Children.Add(rectangle);
            }
        }

        public void UpdateColumnHeaders()
        {
            TopColumnHeader.Update();
            BottomColumnHeader.Update();
        }

        public void AddRow(View.RowPresenter rowPresenter)
        {
            TableCanvas.Children.Add(rowPresenter);
        }
        
        public void RemoveRow(View.RowPresenter rowPresenter)
        {
            TableCanvas.Children.Remove(rowPresenter);
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            RowHeader.VerticalOffset = sv.VerticalOffset;
            TopColumnHeader.HorizontalOffset = sv.HorizontalOffset;
            BottomColumnHeader.HorizontalOffset = sv.HorizontalOffset;

            mainPageViewModel.ScrollTop = sv.VerticalOffset;
            mainPageViewModel.ScrollLeft = sv.HorizontalOffset;
        }

        async void RecognizeStrokes(InkManager inkManager)
        {
            try
            {
                IReadOnlyList<InkStroke> strokes = inkManager.GetStrokes();
                Double centerX = strokes[0].BoundingRect.X + strokes[0].BoundingRect.Width / 2 - 
                    (Double)App.Current.Resources["RowHeaderWidth"] + TableScrollViewer.HorizontalOffset;

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

                        if(candidate == "a" || candidate == "A")
                        {
                            mainPageViewModel.ChangeAggregationType(columnIndex, Model.AggregationType.Average);
                            drawable.RemoveAllStrokes();
                            return;
                        }

                        if (candidate == "m" || candidate == "M")
                        {
                            mainPageViewModel.ChangeAggregationType(columnIndex, Model.AggregationType.Maximum);
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

            drawable.RemoveAllStrokes();
        }

        public void ScrollToColumn(Model.Column column)
        {
            Double offset = TableScrollViewer.HorizontalOffset,
                   width = mainPageViewModel.SheetViewWidth,
                   x1 = column.X,
                   x2 = column.X + column.Width;

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

            TableScrollViewer.ChangeView(to, null, null);

            if (to == null)
            {
                mainPageViewModel.ScrollLeft = TableScrollViewer.HorizontalOffset;
            }
            else
            {
                mainPageViewModel.ScrollLeft = (Double)to;
            }
        }

        private void Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            mainPageViewModel.CancelGroupBy();
            mainPageViewModel.ChartViewModel.Hide();
        }
    }
}
