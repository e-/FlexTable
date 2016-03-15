using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using FlexTable.ViewModel;
using Series = System.Tuple<System.String, System.Collections.Generic.List<System.Tuple<System.Object, System.Double>>>;
using FlexTable.Model;
using Windows.Devices.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using FlexTable.Crayon.Chart;
using Windows.UI.Input;
using FlexTable.Crayon;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class PageView : UserControl
    {
        public BarChart BarChart => BarChartElement;
        public LineChart LineChart => LineChartElement;
        public DescriptiveStatisticsView DescriptiveStatisticsView => DescriptiveStatisticsViewElement;
        public CorrelationStatisticsView CorrelationStatisticsView => CorrelationStatisticsViewElement;
        public DistributionView DistributionView => DistributionViewElement;
        public GroupedBarChart GroupedBarChart => GroupedBarChartElement;
        public Scatterplot Scatterplot => ScatterplotElement;
        public PivotTableView PivotTableView => PivotTableViewElement;
        //public BreadcrumbView BreadcrumbView => BreadcrumbViewElement;

        public StackPanel BarChartTitle => BarChartTitleElement;
        public StackPanel LineChartTitle => LineChartTitleElement;
        public StackPanel DistributionViewTitle => DistributionViewTitleElement;
        public StackPanel DescriptiveStatisticsTitle => DescriptiveStatisticsTitleElement;
        public StackPanel GroupedBarChartTitle => GroupedBarChartTitleElement;
        public StackPanel ScatterplotTitle => ScatterplotTitleElement;
        public StackPanel PivotTableTitle => PivotTableTitleElement;
        public StackPanel CorrelationStatisticsTitle => CorrelationStatisticsTitleElement;
        
        public PageViewModel ViewModel => (PageViewModel)DataContext;

        /// <summary>
        /// cannot be null
        /// </summary>
        public IEnumerable<Row> SelectedRows { get; set; } = new List<Row>();

        
        public PageView()
        {
            this.InitializeComponent();

            BarChartElement.SelectionChanged += SelectionChanged;
            BarChartElement.FilterOut += FilterOut;

            GroupedBarChartElement.SelectionChanged += SelectionChanged;
            GroupedBarChartElement.FilterOut += FilterOut;
         
            LineChartElement.SelectionChanged += SelectionChanged;
            LineChartElement.FilterOut += FilterOut;

            ScatterplotElement.SelectionChanged += SelectionChanged;
            ScatterplotElement.FilterOut += FilterOut;

            DistributionView.Histogram.SelectionChanged += SelectionChanged;
            DistributionView.Histogram.FilterOut += FilterOut;

            PageLabelViewElement.LabelTapped += PageLabelViewElement_LabelTapped;
        }

        public void Initialize()
        {
            Double width = ViewModel.MainPageViewModel.ParagraphWidth, height = ViewModel.MainPageViewModel.ParagraphChartHeight;

            // do not use binding

            BarChartElement.Width = width;
            BarChartElement.Height = height;
            GroupedBarChartElement.Width = width;
            GroupedBarChartElement.Height = height;
            LineChartElement.Width = width;
            LineChartElement.Height = height;
            ScatterplotElement.Width = width;
            ScatterplotElement.Height = height;
            PivotTableViewElement.Width = width;
            PivotTableViewElement.Height = height;
            DistributionViewElement.Width = width;
            DistributionViewElement.Height = height;
            DescriptiveStatisticsViewElement.Width = width;
            DescriptiveStatisticsViewElement.Height = height;
            CorrelationStatisticsViewElement.Width = width;
            CorrelationStatisticsViewElement.Height = height;

            ShowStoryboard.Begin();
        }

        public void SelectionChanged(object sender, IEnumerable<Row> rows, SelectionChangedType selectionChangedType)
        {
            if (selectionChangedType == SelectionChangedType.Add)
                SelectedRows = SelectedRows.Concat(rows).Distinct().ToList();
            else if (selectionChangedType == SelectionChangedType.Remove)
                SelectedRows = SelectedRows.Except(rows).Distinct().ToList();
            else if (selectionChangedType == SelectionChangedType.Replace)
                SelectedRows = rows.ToList();
            else
                SelectedRows = new List<Row>();

            Int32 count = SelectedRows.Count();

            if (count == ViewModel.MainPageViewModel.SheetViewModel.FilteredRows.Count())
            {
                SelectedRows = new List<Row>();
                count = 0;
            }

            if (count == 0)
            {
                HideSelectionIndicator();
                ViewModel.MainPageViewModel.TableViewModel.CancelPreviewRows();
            }
            else
            {
                HideSelectionIndicatorStoryboard.Pause();
                ShowSelectionIndicatorStoryboard.Begin();
                SelectedRowCountIndicator.Text = count.ToString();
                SelectionMessage.Text = count == 1 ? Const.Loader.GetString("SelectionMessage1") : Const.Loader.GetString("SelectionMessage2");
                ViewModel.MainPageViewModel.TableViewModel.PreviewRows(SelectedRows);
            }

            ViewModel.Reflect(ReflectReason.RowSelectionChanged);// 2.OnSelectionChanged | ReflectType2.TrackPreviousParagraph, reason);
        }        

        private void FilterOut(object sender, String name, IEnumerable<Row> filteredRows)
        {
            FilterViewModel fvm = new FilterViewModel(ViewModel.MainPageViewModel)
            {
                Name = name,
                Predicate = r => !filteredRows.Any(rr => rr == r)
            };

            /*SelectedRows = new List<Row>();
            HideSelectionIndicator();
            ViewModel.MainPageViewModel.TableViewModel.SelectedRows = null;*/
            ViewModel.MainPageViewModel.ExplorationViewModel.FilterOut(fvm);
        }

        public void HideSelectionIndicator()
        {
            ShowSelectionIndicatorStoryboard.Pause();
            HideSelectionIndicatorStoryboard.Begin();
        }
        
        public void ReflectState()
        {
            PageViewModel.PageViewState state = ViewModel.State,
                oldState = ViewModel.OldState;

            ///
            /// SelectedStateStoryboard: 차트가 보이도록 애니메이션
            /// EmptyStateStoryboard: 차트가 사라지도록 애니메이션
            /// EnlargeStoryboard: 차트가 꽉 차서 보이도록 애니메이션
            /// DefaultStoryboard: 차트가 줄어들어 보이도록 애니메이션
            /// BrightenStoryboard: 차트 바깥 쪽을 흰색으로
            /// DarkenStoryboard: 차트 바깥쪽을 회색으로
            
            // 현재 뷰가 위에 있다
            if(state == PageViewModel.PageViewState.Selected)
            {
                MoveToSelectedPositionStoryboard.Begin();
                SelectedStateStoryboard.Begin();

                /*EnlargeStoryboard.Begin();
                BrightenStoryboard.Begin();
                ChartVisibleStateStoryboard.Begin();

                HideBottomToolBar.Pause();
                ShowBottomToolBar.Begin();

                ShowTopToolBar.Pause();
                HideTopToolBar.Begin();*/
            }
            else if(state == PageViewModel.PageViewState.Undoing)
            {
                MoveToDefaultPositionStoryboard.Begin();
                UndoStateStoryboard.Begin();
                
                /*EmptyStateStoryboard.Begin();
                BrightenStoryboard.Begin();
                DefaultStoryboard.Begin();

                ShowTopToolBar.Pause();
                HideTopToolBar.Begin();

                ShowBottomToolBar.Pause();
                HideBottomToolBar.Begin();*/
            }
            else if (state == PageViewModel.PageViewState.Empty)
            {
                MoveToDefaultPositionStoryboard.Begin();
                EmptyStateStoryboard.Begin();
                
                /*BrightenStoryboard.Begin();

                ShowTopToolBar.Pause();
                HideTopToolBar.Begin();

                ShowBottomToolBar.Pause();
                HideBottomToolBar.Begin();*/
            }
            else if (state == PageViewModel.PageViewState.Previewing)
            {
                MoveToDefaultPositionStoryboard.Begin();
                SelectedStateStoryboard.Begin();
                /*DarkenStoryboard.Begin();

                HideTopToolBar.Pause();
                ShowTopToolBar.Begin();

                ShowBottomToolBar.Pause();
                HideBottomToolBar.Begin();*/
            }

            switch(state)
            {
                case PageViewModel.PageViewState.Selected:
                    BarChartElement.IsHitTestVisible = true;
                    LineChartElement.IsHitTestVisible = true;
                    DistributionViewElement.IsHitTestVisible = true;
                    GroupedBarChartElement.IsHitTestVisible = true;
                    ScatterplotElement.IsHitTestVisible = true;
                    PivotTableView.IsScrollable = true;
                    break;
                case PageViewModel.PageViewState.Undoing:
                case PageViewModel.PageViewState.Previewing:
                case PageViewModel.PageViewState.Empty:
                    ViewModel.IsPrimaryUndoMessageVisible = true;
                    BarChartElement.IsHitTestVisible = false;
                    LineChartElement.IsHitTestVisible = false;
                    DistributionViewElement.IsHitTestVisible = false;
                    GroupedBarChartElement.IsHitTestVisible = false;
                    ScatterplotElement.IsHitTestVisible = false;
                    PivotTableView.IsScrollable = false;
                    break;
            }
        }

        public List<StackPanel> paragraphs = new List<StackPanel>();

        public void UpdateCarousel(Boolean trackPreviousParagraph, String firstChartTag)
        {
            paragraphs.Clear();

            if (ViewModel.IsBarChartVisible && BarChartElement.Tag?.ToString() == firstChartTag) paragraphs.Add(BarChartWrapperElement);
            if (ViewModel.IsGroupedBarChartVisible && GroupedBarChartElement.Tag?.ToString() == firstChartTag) paragraphs.Add(GroupedBarChartWrapperElement);
            if (ViewModel.IsLineChartVisible && LineChartElement.Tag?.ToString() == firstChartTag) paragraphs.Add(LineChartWrapperElement);
            if (ViewModel.IsDistributionVisible && DistributionViewElement.Tag?.ToString() == firstChartTag) paragraphs.Add(DistributionWrapperElement);
            if (ViewModel.IsDescriptiveStatisticsVisible && DescriptiveStatisticsViewElement.Tag?.ToString() == firstChartTag) paragraphs.Add(DescriptiveStatisticsWrapperElement);
            if (ViewModel.IsScatterplotVisible && ScatterplotElement.Tag?.ToString() == firstChartTag) paragraphs.Add(ScatterplotWrapperElement);
            if (ViewModel.IsPivotTableVisible && PivotTableViewElement.Tag?.ToString() == firstChartTag) paragraphs.Add(PivotTableWrapperElement);
            if (ViewModel.IsCorrelationStatisticsVisible && CorrelationStatisticsViewElement.Tag?.ToString() == firstChartTag) paragraphs.Add(CorrelationStatisticsWrapperElement);

            if (ViewModel.IsBarChartVisible && BarChartElement.Tag?.ToString() != firstChartTag) paragraphs.Add(BarChartWrapperElement);
            if (ViewModel.IsGroupedBarChartVisible && GroupedBarChartElement.Tag?.ToString() != firstChartTag) paragraphs.Add(GroupedBarChartWrapperElement);
            if (ViewModel.IsLineChartVisible && LineChartElement.Tag?.ToString() != firstChartTag) paragraphs.Add(LineChartWrapperElement);
            if (ViewModel.IsDistributionVisible && DistributionViewElement.Tag?.ToString() != firstChartTag) paragraphs.Add(DistributionWrapperElement);
            if (ViewModel.IsDescriptiveStatisticsVisible && DescriptiveStatisticsViewElement.Tag?.ToString() != firstChartTag) paragraphs.Add(DescriptiveStatisticsWrapperElement);
            if (ViewModel.IsScatterplotVisible && ScatterplotElement.Tag?.ToString() != firstChartTag) paragraphs.Add(ScatterplotWrapperElement);
            if (ViewModel.IsPivotTableVisible && PivotTableViewElement.Tag?.ToString() != firstChartTag) paragraphs.Add(PivotTableWrapperElement);
            if (ViewModel.IsCorrelationStatisticsVisible && CorrelationStatisticsViewElement.Tag?.ToString() != firstChartTag) paragraphs.Add(CorrelationStatisticsWrapperElement);
            if (ViewModel.IsNoPossibleVisualizationWarningVisible) paragraphs.Add(NoVisualizationWarningWrapperElement);

            Int32 index = 0;
            foreach (StackPanel paragraph in paragraphs)
            {
                Canvas.SetLeft(paragraph, ViewModel.MainPageViewModel.ParagraphWidth * index);
                index++;
            }
            ParagraphContainerCanvasElement.Width = index * ViewModel.MainPageViewModel.ParagraphWidth;

            // 먼저 전에 보이던 시각화와 같은 시각화가 있는지 본다. 있으면 그걸 먼저 보여줌 (바, 산점도)에서 산점도를 보다 바가 없어지면 그대로 산점도를 보여주고 싶음

            index = 0;
            Int32 sameIndex = 0;
            StackPanel sameParagraph = null;
            foreach (StackPanel paragraph in paragraphs)
            {
                if (paragraph.Tag?.ToString() == PageLabelViewElement.ActivatedParagraphTag)
                {
                    sameParagraph = paragraph;
                    sameIndex = index;
                }
                index++;
            }

            if (sameParagraph != null && trackPreviousParagraph)
            {
                PageLabelViewElement.ActivatedParagraphIndex = sameIndex;
                Carousel.ChangeView(sameIndex * ViewModel.MainPageViewModel.ParagraphWidth, null, null, true);
            }
            else
            {
                // 아니면 현재 위치를 유지해보자
                // 0으로 초기화

                Double offset = Carousel.HorizontalOffset;
                Double width = Carousel.ActualWidth;

                Carousel.ChangeView(0, null, null, true);
                PageLabelViewElement.ActivatedParagraphIndex = 0;
            }

            ViewModel.ViewStatus.ActivatedChart = paragraphs[PageLabelViewElement.ActivatedParagraphIndex].Children.Last();
            PageLabelViewElement.Update(paragraphs);
        }


        private void Wrapper_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch) return;
            if (ViewModel.IsEmpty) { }
            else if (ViewModel.IsNoPossibleVisualizationWarningVisible) { }
            else if (ViewModel.IsUndoing)
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta >= 0) delta = 0;
                if (delta < -ViewModel.MainPageViewModel.PageOffset) { delta = -ViewModel.MainPageViewModel.PageOffset; }
                Canvas.SetTop(this, ViewModel.MainPageViewModel.PageOffset + delta);
                ChartWrapper.Opacity = 0.2 - 0.8 * delta / ViewModel.MainPageViewModel.PageOffset;
                ViewModel.IsPrimaryUndoMessageVisible = delta >= -Const.PageViewToggleThreshold;
            }
            else if (ViewModel.IsSelected) // 선택 된 경우 내릴수만 있음
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta <= 0) delta = 0;
                if (delta > ViewModel.MainPageViewModel.PageHeight) { delta = ViewModel.MainPageViewModel.PageHeight; }
                Canvas.SetTop(this, delta);
                ChartWrapper.Opacity = 0.2 + 0.8 * (1 - delta / ViewModel.MainPageViewModel.PageHeight);
            }
            else // 선택되지 않은 경우 올릴수만 있음
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta >= 0) delta = 0;
                if (delta < -ViewModel.MainPageViewModel.PageOffset) { delta = -ViewModel.MainPageViewModel.PageOffset; }
                Canvas.SetTop(this, ViewModel.MainPageViewModel.PageOffset + delta);
            }

            Canvas.SetZIndex(this, 10);
        }

        private void Wrapper_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch) return;
            if (ViewModel.IsEmpty) { }
            else if (ViewModel.IsNoPossibleVisualizationWarningVisible) { }
            else if (ViewModel.IsUndoing)
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta < -Const.PageViewToggleThreshold) // 많이 올라간 경우
                {
                    ViewModel.State = PageViewModel.PageViewState.Selected; // undo하는 경우는 조건 체크할 필요 없이 무조건 select해도 됨
                    ViewModel.StateChanged(this);
                }
                else
                {
                    MoveToDefaultPositionStoryboard.Begin();
                    ViewModel.IsPrimaryUndoMessageVisible = true;
                }
            }
            else if (ViewModel.IsSelected) // 선택 된 경우 내릴수만 있음
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta > Const.PageViewToggleThreshold) // 많이 내려간 경우
                {
                    ViewModel.State = PageViewModel.PageViewState.Undoing;
                    SelectionChanged(null, null, SelectionChangedType.Clear);//, ReflectReason2.SelectionChanged);
                    ViewModel.StateChanged(this);
                }
                else
                {
                    MoveToSelectedPositionStoryboard.Begin();
                    SelectedStateStoryboard.Begin();
                }
            }
            else // 선택되지 않은 경우 올릴수만 있음
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta < -Const.PageViewToggleThreshold) // 많이 올라간 경우
                {
                    if (this != ViewModel.MainPageViewModel.ExplorationViewModel.TopPageView) return; // 이게 맨 위에있는게 아니면 reject
                    if (!ViewModel.IsPreviewing) return; // 아무것도 프리뷰잉하고 있지 않으면 reject
                    if (ViewModel.MainPageViewModel.ExplorationViewModel.ViewStatus.SelectedColumnViewModels.IndexOf(
                        ViewModel.ViewStatus.SelectedColumnViewModels.Last()
                        ) >= 0)
                        return; // 선택된 컬럼이 이미 선택되어잇으면

                    ViewModel.State = PageViewModel.PageViewState.Selected;
                    ViewModel.StateChanged(this);
                }
                else
                {
                    MoveToDefaultPositionStoryboard.Begin();
                    SelectedStateStoryboard.Begin();
                }
            }
        }

        private void Carousel_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                Double offset = Carousel.HorizontalOffset;
                Double width = Carousel.ActualWidth;
                Int32 newIndex = (Int32)Math.Ceiling((offset - width / 2) / width);
                Int32 oldIndex = PageLabelViewElement.ActivatedParagraphIndex;

                PageLabelViewElement.ActivatedParagraphIndex = newIndex;

                Double newOffset = PageLabelViewElement.ActivatedParagraphIndex * width;

                Carousel.ChangeView(newOffset, null, null);

                PageLabelViewElement.UpdateHighlight();

                if(oldIndex != newIndex)
                    ParagraphScrolled();               
            }
        }

        private void PageLabelViewElement_LabelTapped(object sender, TappedRoutedEventArgs e)
        {
            Double width = Carousel.ActualWidth;
            Double newOffset = PageLabelViewElement.ActivatedParagraphIndex * width;

            Carousel.ChangeView(newOffset, null, null);
            e.Handled = true;
            ParagraphScrolled();
        }

        private void ParagraphScrolled()
        {
            Int32 index = PageLabelViewElement.ActivatedParagraphIndex;
            ViewModel.ViewStatus.ActivatedChart = paragraphs[index].Children.Last();

            if (ViewModel.IsSelected)
                ViewModel.MainPageViewModel.TableViewModel.Reflect(ViewModel.ViewStatus, ReflectReason.PageScrolled);// 2.PageScrolled);
        }
        
        private void Select_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 이 페이지 뷰가 선택될수 있는지 먼저 확인하자
            if (this != ViewModel.MainPageViewModel.ExplorationViewModel.TopPageView) return; // 이게 맨 위에있는게 아니면 reject
            if (!ViewModel.IsPreviewing) return; // 아무것도 프리뷰잉하고 있지 않으면 reject
            if (ViewModel.MainPageViewModel.ExplorationViewModel.ViewStatus.SelectedColumnViewModels.IndexOf(
                ViewModel.ViewStatus.SelectedColumnViewModels.Last()
                ) >= 0) return; // 선택된 컬럼이 이미 선택되어잇으면
            
            ViewModel.State = PageViewModel.PageViewState.Selected; 
            ViewModel.StateChanged(this);
        }

        private void Unselect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.State = PageViewModel.PageViewState.Undoing;
            SelectionChanged(null, null, SelectionChangedType.Clear);//, ReflectReason2.SelectionChanged);
            ViewModel.StateChanged(this);
        }

        private void Undo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ViewModel.State = PageViewModel.PageViewState.Selected; // undo하는 경우는 조건 체크할 필요 없이 무조건 select해도 됨
            ViewModel.StateChanged(this);
        }

        private async void Clipboard_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlashStoryboard.Begin();

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
            uint width = (uint)Carousel.ActualWidth + 1;
            uint height = (uint)Carousel.ActualHeight;
            Carousel.Measure(new Size(width, height));
            //Carousel.Arrange(new Rect(0, 0, width, height));

            await renderBitmap.RenderAsync(Carousel, (int)width, (int)height);
            var pixels = await renderBitmap.GetPixelsAsync();

            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream()
            {
                Size = width * height * 4
            };
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderBitmap.PixelWidth, (uint)renderBitmap.PixelHeight, 96d, 96d, pixels.ToArray());
            await encoder.FlushAsync();
            stream.Seek(0);

            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));

            Clipboard.SetContent(dataPackage);
        }

        private void SelectionIndicator_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Double delta = e.Cumulative.Translation.X;
            if (delta < 0) delta = 0;
            if (delta > Const.SelectionDismissThreshold) delta = Const.SelectionDismissThreshold;
            SelectionIndicatorTemporaryTransform.X = delta;
            SelectionIndicator.Opacity = (Const.SelectionDismissThreshold - delta) / Const.SelectionDismissThreshold;
        }

        private void SelectionIndicator_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Double delta = e.Cumulative.Translation.X;
            if (delta > Const.SelectionDismissThreshold)
            {
                SelectionChanged(this, null, SelectionChangedType.Clear);
            }
            else
            {
                ResetSelectionIndicatorPositionStoryboard.Begin();
            }
        }

        private void SelectionFilterButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Int32 count = SelectedRows.Count();
            if (count > 0)
            {
                FilterOut(this,
                    String.Format(
                        Const.Loader.GetString("FilterOutMessage"),
                        count
                    ), SelectedRows.ToList());
                FilterSelectionIndicatorStoryboard.Begin();
            }
        }

        private void SelectionIndicator_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //SelectionChanged(this, null, SelectionChangedType.Clear);//, ReflectReason2.SelectionChanged);
        }
    }
}
