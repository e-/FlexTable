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
using FlexTable.Model;
using Windows.UI;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class TableView : UserControl
    {
        public static ScrollViewer GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            {
                return o as ScrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }

        public TableViewModel ViewModel { get { return (TableViewModel)DataContext;} }
        
        public ColumnHeaderPresenter TopColumnHeader { get { return TopColumnHeaderElement; } }
        public ColumnHeaderPresenter BottomColumnHeader { get { return BottomColumnHeaderElement; } }
        public RowHeaderPresenter RowHeaderPresenter { get { return RowHeaderPresenterElement; } }
        public GuidelinePresenter GuidelinePresenter { get { return GuidlineElement; } }
        public ColumnIndexer ColumnIndexer { get { return ColumnIndexerElement; } }
        public ColumnHighlighter ColumnHighlighter { get { return ColumnHighlighterElement; } }
        public ScrollViewer AllRowScrollViewer { get; set; }
        public ScrollViewer GroupedRowScrollViewer { get; set; }
        public ScrollViewer SelectedRowScrollViewer { get; set; }
        public ScrollViewer ActivatedScrollViewer { get; set; }
        public Canvas AnimatingRowCanvas => AnimatingRowCanvasElement;

        Drawable drawable = new Drawable()
        {
            IgnoreSmallStrokes = false
        };

        DispatcherTimer timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        InkManager inkManager;

        private List<RowPresenter> allRowPresenters = new List<RowPresenter>();
        private List<RowPresenter> groupedRowPresenters = new List<RowPresenter>();
        private List<RowPresenter> selectedRowPresenters = new List<RowPresenter>();

        public TableView()
        {
            this.InitializeComponent();
            
            drawable.Attach(SheetView, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += RecognizeStrokes;
            timer.Tick += timer_Tick;            
        }

        public void Initialize()
        {
            ActivatedScrollViewer = AllRowScrollViewer = GetScrollViewer(AllRowViewer);
            GroupedRowScrollViewer = GetScrollViewer(GroupedRowViewer);
            SelectedRowScrollViewer = GetScrollViewer(SelectedRowViewer);

            AllRowScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            GroupedRowScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            SelectedRowScrollViewer.ViewChanged += ScrollViewer_ViewChanged;

            foreach (RowViewModel rowViewModel in ViewModel.AllRowViewModels)
            {
                RowPresenter rowPresenter = new RowPresenter()
                {
                    RowViewModel = rowViewModel
                };

                rowPresenter.Update();

                AllRowCanvas.Children.Add(rowPresenter);
                allRowPresenters.Add(rowPresenter);
            }

            foreach (RowViewModel rowViewModel in ViewModel.AllRowViewModels)
            {
                RowPresenter rowPresenter = new RowPresenter()
                {
                    RowViewModel = rowViewModel
                };

                rowPresenter.Update();

                GroupedRowCanvas.Children.Add(rowPresenter);
                groupedRowPresenters.Add(rowPresenter);

            }
            foreach (RowViewModel rowViewModel in ViewModel.AllRowViewModels)
            {
                RowPresenter rowPresenter = new RowPresenter()
                {
                    RowViewModel = rowViewModel
                };

                rowPresenter.Update();

                SelectedRowCanvas.Children.Add(rowPresenter);
                selectedRowPresenters.Add(rowPresenter);
            }
        }

        public void ReflectState()
        {
            TableViewModel.TableViewState state = ViewModel.State,
                oldState = ViewModel.OldState;

            if (state == TableViewModel.TableViewState.AllRow)
            {
                HideGroupedRowViewerStoryboard.Begin();
                HideSelectedRowViewerStoryboard.Begin();
                HideAnimatingRowViewerStoryboard.Begin();
                ShowAllRowViewerStoryboard.Begin();
                
                ActivatedScrollViewer = AllRowScrollViewer;

                var sr = ViewModel.MainPageViewModel.SheetViewModel.FilteredRows.ToList();

                IEnumerable<RowPresenter> selected = allRowPresenters.Where(rp => sr.IndexOf(rp.RowViewModel.Row) >= 0).OrderBy(rp => rp.RowViewModel.Index),
                    unselected = allRowPresenters.Where(rp => sr.IndexOf(rp.RowViewModel.Row) < 0).OrderBy(rp => rp.RowViewModel.Index);

                Int32 index = 0;
                Double height = (Double)App.Current.Resources["RowHeight"];
                List<Color> colors = new List<Color>();

                foreach (RowPresenter rowPresenter in selected)
                {
                    Canvas.SetTop(rowPresenter, /*rowPresenter.RowViewModel.Y =*/ index * height);
                    rowPresenter.Opacity = 1;
                    rowPresenter.Update();
                    colors.Add(rowPresenter.RowViewModel.Color);
                    index++;
                }

                // y 도 힌트에 저장하는게 안낫나?
                foreach (RowPresenter rowPresenter in unselected)
                {
                    Canvas.SetTop(rowPresenter, /*rowPresenter.RowViewModel.Y =*/ index * height);
                    rowPresenter.Opacity = 0.2;
                    rowPresenter.Update();
                    colors.Add(rowPresenter.RowViewModel.Color);
                    index++;
                }

                RowHeaderPresenter.SetRowNumber(colors, selected.Count(), colors.Count);
            }
            else if (state == TableViewModel.TableViewState.GroupedRow)
            {
                HideAllRowViewerStoryboard.Begin();
                HideSelectedRowViewerStoryboard.Begin();
                HideAnimatingRowViewerStoryboard.Begin();
                ShowGroupedRowViewerStoryboard.Begin();

                ActivatedScrollViewer = GroupedRowScrollViewer;

                Int32 index = 0;
                Double height = (Double)App.Current.Resources["RowHeight"];
                Int32 i = 0;
                List<Color> colors = new List<Color>();

                for (i = 0; i < ViewModel.GroupedRowViewModels.Count; ++i)
                {
                    groupedRowPresenters[i].RowViewModel = ViewModel.GroupedRowViewModels[i];
                    Canvas.SetTop(groupedRowPresenters[i], index * height);
                    groupedRowPresenters[i].Visibility = Visibility.Visible;
                    groupedRowPresenters[i].Update();
                    colors.Add(groupedRowPresenters[i].RowViewModel.Color);
                    index++;
                }

                for (; i < ViewModel.AllRowViewModels.Count; ++i)
                {
                    groupedRowPresenters[i].Visibility = Visibility.Collapsed;
                }

                RowHeaderPresenter.SetRowNumber(colors, ViewModel.GroupedRowViewModels.Count, ViewModel.GroupedRowViewModels.Count);
            }
            else if(state == TableViewModel.TableViewState.SelectedRow)
            {
                HideAllRowViewerStoryboard.Begin();
                HideGroupedRowViewerStoryboard.Begin();
                HideAnimatingRowViewerStoryboard.Begin();
                ShowSelectedRowViewerStoryboard.Begin();

                ActivatedScrollViewer = SelectedRowScrollViewer;

                var sr = ViewModel.SelectedRows.ToList();

                IEnumerable<RowPresenter> selected = selectedRowPresenters.Where(rp => sr.IndexOf(rp.RowViewModel.Row) >= 0).OrderBy(rp => rp.RowViewModel.Index),
                    unselected = selectedRowPresenters.Where(rp => sr.IndexOf(rp.RowViewModel.Row) < 0).OrderBy(rp => rp.RowViewModel.Index);

                Int32 index = 0;
                Double height = (Double)App.Current.Resources["RowHeight"];
                List<Color> colors = new List<Color>();

                foreach (RowPresenter rowPresenter in selected)
                {
                    Canvas.SetTop(rowPresenter, index * height);
                    rowPresenter.Opacity = 1;
                    rowPresenter.Update();
                    colors.Add(rowPresenter.RowViewModel.Color);
                    index++;
                }

                foreach (RowPresenter rowPresenter in unselected)
                {
                    Canvas.SetTop(rowPresenter, index * height);
                    rowPresenter.Opacity = 0.2;
                    rowPresenter.Update();
                    colors.Add(rowPresenter.RowViewModel.Color);
                    index++;
                }

                RowHeaderPresenter.SetRowNumber(colors, selected.Count(), colors.Count);
            }
            else if(state == TableViewModel.TableViewState.Animation)
            {
                HideAllRowViewerStoryboard.Begin();
                HideGroupedRowViewerStoryboard.Begin();
                HideSelectedRowViewerStoryboard.Begin();
                ShowAnimatingRowViewerStoryboard.Begin();
            }

            UpdateScrollViews();
        }

        const Double VerticalStrokeMaxWidth = 30;
        const Double VerticalStrokeMinHeight = 50;
        const Double VerticalStrokeHeightDifferenceThreshold = 20;
        
        async void timer_Tick(object sender, object e)
        {
            timer.Stop();
            if (this.inkManager == null)
                return;
            
            IReadOnlyList<InkStroke> strokes = this.inkManager.GetStrokes();
            
            Double centerX = strokes[0].BoundingRect.X + strokes[0].BoundingRect.Width / 2 -
                (Double)App.Current.Resources["RowHeaderWidth"] + ActivatedScrollViewer.HorizontalOffset;

            Double screenHeight = this.ViewModel.MainPageViewModel.Bounds.Height;
            Double columnHeaderHeight = (Double)App.Current.Resources["ColumnHeaderHeight"];

            ColumnViewModel selectedColumnViewModel = null;


            foreach (ColumnViewModel columnViewModel in ViewModel.SheetViewModel.ColumnViewModels) // 현재 그 아래에 있는 컬럼을 찾고
            {
                if (columnViewModel.X <= centerX && centerX < columnViewModel.X + columnViewModel.Width)
                {
                    selectedColumnViewModel = columnViewModel;
                    break;
                }
            }

            if(selectedColumnViewModel == null)
            {
                drawable.RemoveAllStrokes();
                return;
            }

            if (strokes[0].BoundingRect.Y < columnHeaderHeight 
                || strokes[0].BoundingRect.Y + strokes[0].BoundingRect.Height > screenHeight - columnHeaderHeight) // 컬럼 헤더에서 스트로크가 쓰여졌을때에는 
            {
                // 스트로크를 인식해서 적절한 행동을 한다.

                IReadOnlyList<InkRecognitionResult> results = await this.inkManager.RecognizeAsync(InkRecognitionTarget.Recent);

                foreach (InkRecognitionResult result in results)
                {
                    foreach (String candidate in result.GetTextCandidates())
                    {
                        String upperCandidate = candidate.ToUpper();

                        if (selectedColumnViewModel != null && selectedColumnViewModel.Type == Model.ColumnType.Numerical
                            && ViewModel.MainPageViewModel.ExplorationViewModel.ViewStatus.SelectedColumnViewModels.Count > 0)
                        {
                            switch (upperCandidate)
                            {
                                case "MIN":
                                    selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.MinAggregation();
                                    ViewModel.MainPageViewModel.ReflectAll();
                                    Debug.WriteLine("Min selected");
                                    drawable.RemoveAllStrokes();
                                    timer.Stop();
                                    return;
                                case "MAX":
                                    selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.MaxAggregation();
                                    ViewModel.MainPageViewModel.ReflectAll();
                                    Debug.WriteLine("Max selected");
                                    drawable.RemoveAllStrokes();
                                    timer.Stop();
                                    return;
                                case "AVG":
                                case "MEAN":
                                    selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.AverageAggregation();
                                    ViewModel.MainPageViewModel.ReflectAll();
                                    Debug.WriteLine("Mean selected");
                                    drawable.RemoveAllStrokes();
                                    timer.Stop();
                                    return;
                                case "SUM":
                                    selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.SumAggregation();
                                    ViewModel.MainPageViewModel.ReflectAll();
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
        }
        
        void RecognizeStrokes(InkManager inkManager)
        {
            IReadOnlyList<InkStroke> strokes = inkManager.GetStrokes();
            this.inkManager = inkManager;

            Double centerX = strokes[0].BoundingRect.X + strokes[0].BoundingRect.Width / 2 -
                (Double)App.Current.Resources["RowHeaderWidth"] + ActivatedScrollViewer.HorizontalOffset;

            Double screenHeight = this.ViewModel.MainPageViewModel.Bounds.Height;
            Double columnHeaderHeight = (Double)App.Current.Resources["ColumnHeaderHeight"];

            ColumnViewModel selectedColumnViewModel = null;

            foreach (ColumnViewModel columnViewModel in ViewModel.SheetViewModel.ColumnViewModels) // 현재 그 아래에 있는 컬럼을 찾고
            {
                if (columnViewModel.X <= centerX && centerX < columnViewModel.X + columnViewModel.Width)
                {
                    selectedColumnViewModel = columnViewModel;
                    break;
                }
            }

            if (selectedColumnViewModel == null)
            {
                drawable.RemoveAllStrokes();
                return;
            }

            if (strokes[0].BoundingRect.Y < columnHeaderHeight
                || strokes[0].BoundingRect.Y + strokes[0].BoundingRect.Height > screenHeight - columnHeaderHeight) // 컬럼 헤더에서 스트로크가 쓰여졌을때에는 다음 글자가 쓰여질때까지 기다린다
            {
                if(timer.IsEnabled)timer.Stop();
                timer.Start();
            }
            else // 아니면 바로 실행해버린다.
            {
                if (strokes[0].BoundingRect.Width < VerticalStrokeMaxWidth && strokes[0].BoundingRect.Height > VerticalStrokeMinHeight) // 세로 스트로크면 소팅하라는 것임 
                {
                    var points = strokes[0].GetInkPoints();
                    var firstPoint = points.First();
                    var lastPoint = points.Last();

                    if (firstPoint.Position.Y < lastPoint.Position.Y - VerticalStrokeHeightDifferenceThreshold)
                    {
                        // 내림차순 정렬
                        this.ViewModel.SheetViewModel.Sort(selectedColumnViewModel, SortOption.Descending);
                        this.ViewModel.MainPageViewModel.ReflectAll();
                    }
                    else if (lastPoint.Position.Y < firstPoint.Position.Y + VerticalStrokeHeightDifferenceThreshold)
                    {
                        // 오름차순 정렬
                        this.ViewModel.SheetViewModel.Sort(selectedColumnViewModel, SortOption.Ascending);
                        this.ViewModel.MainPageViewModel.ReflectAll();
                    }
                }
                drawable.RemoveAllStrokes();
            }
        }

        public void ScrollToColumnViewModel(ColumnViewModel columnViewModel)
        {
            Double offset = ActivatedScrollViewer.HorizontalOffset,
                   width = ViewModel.SheetViewWidth,
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

            ActivatedScrollViewer.ChangeView(to, null, null);

            if (to == null)
            {
                ViewModel.ScrollLeft = ActivatedScrollViewer.HorizontalOffset;
            }
            else
            {
                ViewModel.ScrollLeft = (Double)to;
            }
        }


        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateScrollViews();
        }

        void UpdateScrollViews()
        {
            ScrollViewer sv = ActivatedScrollViewer;
            RowHeaderPresenterElement.VerticalOffset = sv.VerticalOffset;
            GuidlineElement.VerticalOffset = sv.VerticalOffset;
            TopColumnHeader.HorizontalOffset = sv.HorizontalOffset;
            BottomColumnHeader.HorizontalOffset = sv.HorizontalOffset;

            ViewModel.ScrollTop = sv.VerticalOffset;
            ViewModel.ScrollLeft = sv.HorizontalOffset;
        }


    }
}
