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
using Windows.UI.Input;

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

        private List<RowPresenter> activatedRowPresenters;
        private List<RowPresenter> allRowPresenters = new List<RowPresenter>();
        private List<RowPresenter> groupedRowPresenters = new List<RowPresenter>();
        private List<RowPresenter> selectedRowPresenters = new List<RowPresenter>();

        public TableView()
        {
            this.InitializeComponent();
            
            drawable.Attach(SheetView, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdding += StrokeAdding;
            drawable.StrokeAdded += StrokeAdded;
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

            AllRowCanvas.Children.Clear();
            allRowPresenters.Clear();
            GroupedRowCanvas.Children.Clear();
            groupedRowPresenters.Clear();
            SelectedRowCanvas.Children.Clear();
            selectedRowPresenters.Clear();

            foreach (RowViewModel rowViewModel in ViewModel.AllRowViewModels)
            {
                RowPresenter rowPresenter = new RowPresenter()
                {
                    RowViewModel = rowViewModel
                };

                rowPresenter.Update(null);

                AllRowCanvas.Children.Add(rowPresenter);
                allRowPresenters.Add(rowPresenter);
            }

            foreach (RowViewModel rowViewModel in ViewModel.AllRowViewModels)
            {
                RowPresenter rowPresenter = new RowPresenter()
                {
                    RowViewModel = rowViewModel
                };

                rowPresenter.Update(null);

                GroupedRowCanvas.Children.Add(rowPresenter);
                groupedRowPresenters.Add(rowPresenter);

            }
            foreach (RowViewModel rowViewModel in ViewModel.AllRowViewModels)
            {
                RowPresenter rowPresenter = new RowPresenter()
                {
                    RowViewModel = rowViewModel
                };

                rowPresenter.Update(null);

                SelectedRowCanvas.Children.Add(rowPresenter);
                selectedRowPresenters.Add(rowPresenter);
            }

            activatedRowPresenters = allRowPresenters;
        }
        

        public void ReflectState(ViewStatus viewStatus)
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

                ColumnViewModel coloredColumnViewModel = viewStatus?.GetColoredColumnViewModel();
                var sr = ViewModel.MainPageViewModel.SheetViewModel.FilteredRows.ToList();

                IEnumerable<RowPresenter> selected = allRowPresenters.Where(rp => sr.IndexOf(rp.RowViewModel.Row) >= 0).OrderBy(rp => rp.RowViewModel.Index),
                    unselected = allRowPresenters.Where(rp => sr.IndexOf(rp.RowViewModel.Row) < 0).OrderBy(rp => rp.RowViewModel.Index);

                Int32 index = 0;
                Double height = Const.RowHeight;
                List<Color> colors = new List<Color>();

                foreach (RowPresenter rowPresenter in selected)
                {
                    Canvas.SetTop(rowPresenter, index * height);
                    rowPresenter.Opacity = 1;
                    rowPresenter.Update(coloredColumnViewModel); 
                    index++;
                }

                // y 도 힌트에 저장하는게 안낫나?
                foreach (RowPresenter rowPresenter in unselected)
                {
                    Canvas.SetTop(rowPresenter, index * height);
                    rowPresenter.Opacity = 0.2;
                    rowPresenter.Update(coloredColumnViewModel);
                    index++;
                }

                RowHeaderPresenter.SetRowNumber(colors, selected.Count(), index);
                activatedRowPresenters = allRowPresenters;
            }
            else if (state == TableViewModel.TableViewState.GroupedRow)
            {
                HideAllRowViewerStoryboard.Begin();
                HideSelectedRowViewerStoryboard.Begin();
                HideAnimatingRowViewerStoryboard.Begin();
                ShowGroupedRowViewerStoryboard.Begin();

                ActivatedScrollViewer = GroupedRowScrollViewer;

                Int32 index = 0;
                Double height = Const.RowHeight;
                Int32 i = 0;
                List<Color> colors = new List<Color>();
                ColumnViewModel coloredColumnViewModel = viewStatus?.GetColoredColumnViewModel(),
                    firstColoredColumnViewModel = viewStatus?.GetFirstColoredColumnViewModel(),
                    secondColoredColumnViewModel = viewStatus?.GetSecondColoredColumnViewModel(),
                    thirdColoredColumnViewModel = viewStatus?.GetThirdColoredColumnViewModel();

                // pivotTable과 barChart가 색깔을 공유함에 주의
                if (viewStatus.IsCorrelationStatisticsVisible || viewStatus.IsDescriptiveStatisticsVisible)
                {
                    coloredColumnViewModel = firstColoredColumnViewModel = secondColoredColumnViewModel = thirdColoredColumnViewModel = null;
                }
                else if (viewStatus.IsCN)
                {
                    if (viewStatus.IsLineChartVisible)
                    {
                        coloredColumnViewModel = null;
                    }
                    else
                    {
                        firstColoredColumnViewModel = null;
                        secondColoredColumnViewModel = null;
                    }
                }
                else if (viewStatus.IsCCN)
                {
                    firstColoredColumnViewModel = null;
                    secondColoredColumnViewModel = null;
                }
                else if (viewStatus.IsCNN)
                {
                    if (viewStatus.IsPivotTableVisible)
                    {
                        firstColoredColumnViewModel = secondColoredColumnViewModel = null;
                    }
                    else
                    {
                        coloredColumnViewModel = null;
                    }
                }

                for (i = 0; i < ViewModel.GroupedRowViewModels.Count; ++i)
                {
                    groupedRowPresenters[i].RowViewModel = ViewModel.GroupedRowViewModels[i];
                    Canvas.SetTop(groupedRowPresenters[i], index * height);
                    groupedRowPresenters[i].Visibility = Visibility.Visible;
                    groupedRowPresenters[i].Update(coloredColumnViewModel, firstColoredColumnViewModel, secondColoredColumnViewModel, thirdColoredColumnViewModel);
                    index++;
                }

                for (; i < ViewModel.AllRowViewModels.Count; ++i)
                {
                    groupedRowPresenters[i].Visibility = Visibility.Collapsed;
                }

                RowHeaderPresenter.SetRowNumber(colors, ViewModel.GroupedRowViewModels.Count, ViewModel.GroupedRowViewModels.Count);
                activatedRowPresenters = groupedRowPresenters;
            }
            else if(state == TableViewModel.TableViewState.SelectedRow)
            {
                HideAllRowViewerStoryboard.Begin();
                HideGroupedRowViewerStoryboard.Begin();
                HideAnimatingRowViewerStoryboard.Begin();
                ShowSelectedRowViewerStoryboard.Begin();

                ActivatedScrollViewer = SelectedRowScrollViewer;
                ScrollToOrigin(ActivatedScrollViewer);

                IEnumerable<Row> filteredRows = ViewModel.SheetViewModel.FilteredRows;

                ColumnViewModel coloredColumnViewModel = viewStatus?.GetColoredColumnViewModel(),
                    firstColoredColumnViewModel = viewStatus?.GetFirstColoredColumnViewModel(),
                    secondColoredColumnViewModel = viewStatus?.GetSecondColoredColumnViewModel(),
                    thirdColoredColumnViewModel = viewStatus?.GetThirdColoredColumnViewModel();

                if(!viewStatus.IsDistributionViewVisible && !viewStatus.IsGroupedBarChartVisible)
                {
                    firstColoredColumnViewModel = secondColoredColumnViewModel = thirdColoredColumnViewModel = null;
                }
                if (viewStatus.IsCCN && viewStatus.IsGroupedBarChartVisible) firstColoredColumnViewModel = secondColoredColumnViewModel = thirdColoredColumnViewModel = null;
                if (viewStatus.IsCNN && viewStatus.IsGroupedBarChartVisible) coloredColumnViewModel = null;
                
                Int32 index = 0;
                Double height = Const.RowHeight;
                List<Color> colors = new List<Color>();

                // selectedRow는 scatterplot과 barchart가 공유함에 주의

                foreach (RowPresenter rowPresenter in selectedRowPresenters.OrderBy(rp => rp.RowViewModel.Index))
                {
                    Canvas.SetTop(rowPresenter, index * height);
                    rowPresenter.Update(coloredColumnViewModel, firstColoredColumnViewModel, secondColoredColumnViewModel, thirdColoredColumnViewModel);
                    if(filteredRows.Contains(rowPresenter.RowViewModel.Row) && (ViewModel.SelectedRows.Count() == 0 || ViewModel.SelectedRows.Contains(rowPresenter.RowViewModel.Row)))
                    {
                        rowPresenter.Opacity = 1;
                    }
                    else
                    {
                        rowPresenter.Opacity = 0.2;
                    }
                    index++;
                }
                RowHeaderPresenter.SetRowNumber(colors, ViewModel.SelectedRows.Count(), index);
                activatedRowPresenters = selectedRowPresenters;
            }
            else if(state == TableViewModel.TableViewState.Animation)
            {
                SlowlyHideAllRowViewerStoryboard.Begin();
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
                                    ViewModel.MainPageViewModel.ReflectAll(ReflectReason.AggregateFunctionChanged); // 2.ColumnViewModelChanged);
                                    Debug.WriteLine("Min selected");
                                    drawable.RemoveAllStrokes();
                                    timer.Stop();
                                    return;
                                case "MAX":
                                    selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.MaxAggregation();
                                    ViewModel.MainPageViewModel.ReflectAll(ReflectReason.AggregateFunctionChanged); // 2.ColumnViewModelChanged);
                                    Debug.WriteLine("Max selected");
                                    drawable.RemoveAllStrokes();
                                    timer.Stop();
                                    return;
                                case "AVG":
                                case "MEAN":
                                    selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.AverageAggregation();
                                    ViewModel.MainPageViewModel.ReflectAll(ReflectReason.AggregateFunctionChanged); // 2.ColumnViewModelChanged);
                                    Debug.WriteLine("Mean selected");
                                    drawable.RemoveAllStrokes();
                                    timer.Stop();
                                    return;
                                case "SUM":
                                    selectedColumnViewModel.AggregativeFunction = new AggregativeFunction.SumAggregation();
                                    ViewModel.MainPageViewModel.ReflectAll(ReflectReason.AggregateFunctionChanged); // 2.ColumnViewModelChanged);
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

        Boolean isSelectionPreviewing = false;

        void StrokeAdding(Rect boundingRect)
        {
            Double centerX = boundingRect.X + boundingRect.Width / 2 -
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
                return;
            }

            if (
                boundingRect.Y + boundingRect.Height < columnHeaderHeight * 1.2 ||
                screenHeight - columnHeaderHeight * 1.2 < boundingRect.Y
                ) // 컬럼 헤더에서 스트로크가 쓰여졌을때에는 다음 글자가 쓰여질때까지 기다린다
            {
            }
            else // 아니면 테이블 프리뷰
            {
                if (boundingRect.Height <= VerticalStrokeMinHeight) // 세로 스트로크인지 원인지 모르겠으면 대기
                {
                }
                else if (boundingRect.Width < VerticalStrokeMaxWidth && boundingRect.Height > VerticalStrokeMinHeight) // 세로 스트로크면 소팅하라는 것임 따라서 무시
                {
                    /*if(isSelectionPreviewing)
                    {
                        ReflectState(ViewModel.MainPageViewModel.ExplorationViewModel.ViewStatus);
                        isSelectionPreviewing = false;
                    }*/
                }
                else // 아니면 선택하라는 것임
                {
                    Double startY = boundingRect.Y + ActivatedScrollViewer.VerticalOffset - columnHeaderHeight,
                        endY = boundingRect.Y + boundingRect.Height + ActivatedScrollViewer.VerticalOffset - columnHeaderHeight;

                    PreviewSelectionByRange(startY, endY);
                    //isSelectionPreviewing = true;
                }
            }
        }

        void StrokeAdded(InkManager inkManager)
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

            if (
                strokes[0].BoundingRect.Y + strokes[0].BoundingRect.Height < columnHeaderHeight * 1.2 ||
                screenHeight - columnHeaderHeight * 1.2 < strokes[0].BoundingRect.Y
                /*(firstInkPoint.Position.Y < columnHeaderHeight
                || firstInkPoint.Position.Y > screenHeight - columnHeaderHeight) */

                ) // 컬럼 헤더에서 스트로크가 쓰여졌을때에는 다음 글자가 쓰여질때까지 기다린다
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
                        // 오름차순 정렬
                        this.ViewModel.SheetViewModel.Sort(selectedColumnViewModel, SortOption.Ascending);
                        this.ViewModel.MainPageViewModel.ReflectAll(ReflectReason.ColumnSorted);// 2.SelectionChanged);
                    }
                    else if (lastPoint.Position.Y < firstPoint.Position.Y + VerticalStrokeHeightDifferenceThreshold)
                    {
                        // 내림차순 정렬
                        this.ViewModel.SheetViewModel.Sort(selectedColumnViewModel, SortOption.Descending);
                        this.ViewModel.MainPageViewModel.ReflectAll(ReflectReason.ColumnSorted); // 2.SelectionChanged);
                    }
                }
                else // 아니면 선택하라는 것임
                {
                    Double startY = strokes[0].BoundingRect.Y + ActivatedScrollViewer.VerticalOffset - columnHeaderHeight,
                        endY = strokes[0].BoundingRect.Y + strokes[0].BoundingRect.Height + ActivatedScrollViewer.VerticalOffset - columnHeaderHeight;

                    ViewModel.SelectRowsByRange(startY, endY);
                }
                drawable.RemoveAllStrokes();
            }
        }

        public void PreviewSelectionByRange(Double startY, Double endY)
        {
            if (ViewModel.MainPageViewModel.ExplorationViewModel.SelectedPageViews.Count == 0) return;

            Double rowHeight = Const.RowHeight;

            startY -= rowHeight / 2; // row의 중앙을 기준으로 테스트 하기 위함임
            endY -= rowHeight / 2;

            foreach (RowPresenter rowPresenter in activatedRowPresenters)
            {
                if (startY <= rowPresenter.RowViewModel.Y && rowPresenter.RowViewModel.Y <= endY)
                {
                    rowPresenter.Opacity = 1;
                }
                else
                {
                    rowPresenter.Opacity = 0.2;
                }
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

        public void ScrollToOrigin(ScrollViewer sv)
        {
            sv.ChangeView(0, 0, null);
        }
    }
}
