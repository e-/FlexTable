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

        public EditableTitle BarChartTitle => BarChartTitleElement;
        public EditableTitle LineChartTitle => LineChartTitleElement;
        public EditableTitle DistributionViewTitle => DistributionViewTitleElement;
        public EditableTitle DescriptiveStatisticsTitle => DescriptiveStatisticsTitleElement;
        public EditableTitle GroupedBarChartTitle => GroupedBarChartTitleElement;
        public EditableTitle ScatterplotTitle => ScatterplotTitleElement;
        public EditableTitle PivotTableTitle => PivotTableTitleElement;
        public EditableTitle CorrelationStatisticsTitle => CorrelationStatisticsTitleElement;
        
        public PageViewModel ViewModel => (PageViewModel)DataContext;

        /// <summary>
        /// cannot be null
        /// </summary>
        public IEnumerable<Row> SelectedRows => ViewModel.MainPageViewModel.View.SelectionView.SelectedRows;

        public PageView()
        {
            this.InitializeComponent();

            BarChartElement.SelectionChanged += SelectionChanged;
            BarChartElement.FilterOut += FilterOut;

            GroupedBarChartElement.SelectionChanged += SelectionChanged;
            GroupedBarChartElement.FilterOut += FilterOut;
            GroupedBarChartElement.RemoveColumnViewModel += RemoveColumnViewModel;
         
            LineChartElement.SelectionChanged += SelectionChanged;
            LineChartElement.FilterOut += FilterOut;
            LineChartElement.RemoveColumnViewModel += RemoveColumnViewModel;

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
            ViewModel.MainPageViewModel.View.SelectionView.ChangeSelecion(rows, selectionChangedType, true);
        }

        private void FilterOut(object sender, IEnumerable<Category> categoriesRaw)
        {
            List<Category> categories = categoriesRaw.ToList();
            foreach(Category category in categories)
            {
                category.IsKept = false;
            }

            foreach(Category category in categories)
            {
                ColumnViewModel cvm = category.ColumnViewModel;

                if (cvm.Categories.All(c => c.IsKept)) cvm.IsKept = true;
                else if (cvm.Categories.All(c => !c.IsKept)) cvm.IsKept = false;
                else cvm.IsKept = null;
            }

            ViewModel.MainPageViewModel.SheetViewModel.UpdateFilter();
            ViewModel.MainPageViewModel.ReflectAll(ReflectReason.RowFiltered);            
        }

        private void RemoveColumnViewModel(object sender, string title)
        {
            ColumnViewModel columnViewModel = ViewModel.MainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Column.Name == title).FirstOrDefault();

            if(columnViewModel != null)
            {
                ViewModel.RemoveColumnViewModel(columnViewModel);
            }
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
                if(SelectedStateStoryboard.GetCurrentState() != ClockState.Active)
                    SelectedStateStoryboard.Begin();
            }
            else if(state == PageViewModel.PageViewState.Undoing)
            {
                MoveToDefaultPositionStoryboard.Begin();
                UndoStateStoryboard.Begin();
            }
            else if (state == PageViewModel.PageViewState.Empty)
            {
                MoveToDefaultPositionStoryboard.Begin();
                EmptyStateStoryboard.Begin();
            }
            else if (state == PageViewModel.PageViewState.Previewing)
            {
                MoveToDefaultPositionStoryboard.Begin();
                if (SelectedStateStoryboard.GetCurrentState() != ClockState.Active)
                    SelectedStateStoryboard.Begin();
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

                    BarChartTitleElement.IsHitTestVisible = true;
                    GroupedBarChartTitleElement.IsHitTestVisible = true;
                    LineChartTitleElement.IsHitTestVisible = true;
                    DistributionViewTitleElement.IsHitTestVisible = true;
                    DescriptiveStatisticsTitleElement.IsHitTestVisible = true;
                    ScatterplotTitleElement.IsHitTestVisible = true;
                    PivotTableTitleElement.IsHitTestVisible = true;
                    CorrelationStatisticsTitleElement.IsHitTestVisible = true;
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

                    BarChartTitleElement.IsHitTestVisible = false;
                    GroupedBarChartTitleElement.IsHitTestVisible = false;
                    LineChartTitleElement.IsHitTestVisible = false;
                    DistributionViewTitleElement.IsHitTestVisible = false;
                    DescriptiveStatisticsTitleElement.IsHitTestVisible = false;
                    ScatterplotTitleElement.IsHitTestVisible = false;
                    PivotTableTitleElement.IsHitTestVisible = false;
                    CorrelationStatisticsTitleElement.IsHitTestVisible = false;
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

        public async void Capture()
        {            
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
            uint width = (uint)Carousel.ActualWidth + 1;
            uint height = (uint)Carousel.ActualHeight;
            Carousel.Measure(new Size(width, height));
            Carousel.Background = new SolidColorBrush(Colors.White);
            //Carousel.Arrange(new Rect(0, 0, width, height));

            await renderBitmap.RenderAsync(Carousel, (int)width, (int)height);
            var pixels = await renderBitmap.GetPixelsAsync();

            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream()
            {
                Size = width * height * 4
            };
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)renderBitmap.PixelWidth, (uint)renderBitmap.PixelHeight, 96d, 96d, pixels.ToArray());
            await encoder.FlushAsync();
            stream.Seek(0);

            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));

            Clipboard.SetContent(dataPackage);
            FlashStoryboard.Begin();
            Carousel.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void Wrapper_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if(e.HoldingState == HoldingState.Started)
            {
                ViewModel.MainPageViewModel.View.ScreenshotButtons.Show();
            }
            else if(e.HoldingState == HoldingState.Completed)
            {
                ViewModel.MainPageViewModel.View.ScreenshotButtons.Hide();
            }
        }
    }
}
