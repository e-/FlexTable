﻿using System;
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

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class PageView : UserControl
    {
        const Double SelectionDismissThreshold = 50;

        public BarChart BarChart => BarChartElement;
        public LineChart LineChart => LineChartElement;
        public DescriptiveStatisticsView DescriptiveStatisticsView => DescriptiveStatisticsViewElement;
        public CorrelationStatisticsView CorrelationStatisticsView => CorrelationStatisticsViewElement;
        public DistributionView DistributionView => DistributionViewElement;
        public GroupedBarChart GroupedBarChart => GroupedBarChartElement;
        public Scatterplot Scatterplot => ScatterplotElement;
        public PivotTableView PivotTableView => PivotTableViewElement;

        public StackPanel BarChartTitle => BarChartTitleElement;
        public StackPanel LineChartTitle => LineChartTitleElement;
        public StackPanel DistributionViewTitle => DistributionViewTitleElement;
        public StackPanel DescriptiveStatisticsTitle => DescriptiveStatisticsTitleElement;
        public StackPanel GroupedBarChartTitle => GroupedBarChartTitleElement;
        public StackPanel ScatterplotTitle => ScatterplotTitleElement;
        public StackPanel PivotTableTitle => PivotTableTitleElement;
        public StackPanel CorrelationStatisticsTitle => CorrelationStatisticsTitleElement;

        public PageViewModel ViewModel => (PageViewModel)DataContext;
        private IEnumerable<Row> SelectedRows { get; set; }

        
        public PageView()
        {
            this.InitializeComponent();

            BarChartElement.SelectionChanged += ChartElement_SelectionChanged;
            BarChartElement.FilterOut += ChartElement_FilterOut;

            GroupedBarChartElement.SelectionChanged += ChartElement_SelectionChanged;
            GroupedBarChartElement.FilterOut += ChartElement_FilterOut;
         
            LineChartElement.SelectionChanged += LineChartElement_SelectionChanged; ;
            LineChartElement.FilterOut += LineChartElement_FilterOut;

            ScatterplotElement.SelectionChanged += ChartElement_SelectionChanged;
            ScatterplotElement.FilterOut += ChartElement_FilterOut;

            DistributionView.Histogram.SelectionChanged += Histogram_SelectionChanged;
            DistributionView.Histogram.FilterOut += Histogram_FilterOut;

            PageLabelViewElement.LabelTapped += PageLabelViewElement_LabelTapped;
        }

        
        private void SelectionChanged(object sender, IEnumerable<Row> selectedRows)
        {
            Int32 count = selectedRows.Count();
            SelectedRows = selectedRows;
            if (count == 0)
            {
                ShowSelectionIndicatorStoryboard.Pause();
                HideSelectionIndicatorStoryboard.Begin();
                ViewModel.MainPageViewModel.TableViewModel.CancelPreviewRows();
            }
            else
            {
                Int32 rowCount = selectedRows.Count();

                HideSelectionIndicatorStoryboard.Pause();
                ShowSelectionIndicatorStoryboard.Begin();
                SelectedRowCountIndicator.Text = rowCount.ToString();
                SelectionMessage.Text = rowCount == 1 ? "row selected" : "rows selected";
                ViewModel.MainPageViewModel.TableViewModel.PreviewRows(selectedRows);
            }
            BroadcastSelection(sender);
        }

        private void FilterOut(IEnumerable<Row> filteredRows, String name, IEnumerable<String> values)
        {
            FilterOut(filteredRows, (values.Count() == 1) ?
                $"{name} = {values.First()}" :
                $"{name} in {String.Join(", ", values)}");
        }

        private void FilterOut(IEnumerable<Row> filteredRows, String name)
        {
            FilterViewModel fvm = new FilterViewModel(ViewModel.MainPageViewModel)
            {
                Name = name,
                Predicate = r => !filteredRows.Any(rr => rr == r)
            };
            if (ViewModel.FilterOut(fvm))
            {
                SelectionChanged(null, new List<Row>() { });
                ClearSelection(false);
            }
        }           

        #region Visualization Event Handlers

        private void ChartElement_SelectionChanged(object sender, object e, object datum, int index)
        {
            IEnumerable<Row> selectedRows = datum as List<Row>;
            SelectionChanged(sender, selectedRows);
        }

        private void LineChartElement_SelectionChanged(object sender, object e, object datum, int index)
        {
            IEnumerable<LineChartDatum> selection = datum as IEnumerable<LineChartDatum>;
            SelectionChanged(sender, selection.SelectMany(s => s.Rows));
        }

        private void Histogram_SelectionChanged(object sender, object e, object datum, int index)
        {
            IEnumerable<BarChartDatum> selection = datum as IEnumerable<BarChartDatum>;
            SelectionChanged(sender, selection.SelectMany(s => s.Rows));
        }
        
        private void ChartElement_FilterOut(object sender, object e, object datum, int index)
        {
            IEnumerable<Row> filteredRows = datum as List<Row>;
            if (filteredRows == null || filteredRows.Count() == 0) return;
            //ColumnViewModel columnViewModel = BarChartElement.Data.First().ColumnViewModel;
            FilterOut(filteredRows, e.ToString());// $"Filtered by {columnViewModel.Name}");
        }

        private void LineChartElement_FilterOut(object sender, object e, object datum, int index)
        {
            IEnumerable<LineChartDatum> filteredData = datum as IEnumerable<LineChartDatum>;
            FilterOut(filteredData.SelectMany(lcd => lcd.Rows).ToList(), filteredData.First().ColumnViewModel.Name, filteredData.Select(d => d.Key.ToString()));
        }       

        private void Histogram_FilterOut(object sender, object e, object datum, int index)
        {
            IEnumerable<BarChartDatum> filteredData = datum as IEnumerable<BarChartDatum>;
            FilterOut(filteredData.SelectMany(bcd => bcd.Rows).ToList(), filteredData.First().ColumnViewModel.Name, filteredData.Select(d => d.Key.ToString()));
        }


        private void SelectionFilterButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(SelectedRows != null && SelectedRows.Count() > 0)
            {
                Int32 count = SelectedRows.Count();
                FilterOut(SelectedRows.ToList(), $"Filtered {count} row" + (count == 1 ? String.Empty : "s"));
                SelectionChanged(null, new List<Row>() { });
                ClearSelection(false);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowStoryboardElement.Begin();
        }
        #endregion
        
        public void ReflectState()
        {
            PageViewModel.PageViewState state = ViewModel.State,
                oldState = ViewModel.OldState;

            // 현재 뷰가 위에 있다
            if(state == PageViewModel.PageViewState.Selected)
            {
                MoveToSelectedPositionStoryboard.Begin();
                CancelDimmingboard.Begin();

                HideBottomToolBar.Pause();
                ShowBottomToolBar.Begin();

                ShowTopToolBar.Pause();
                HideTopToolBar.Begin();
            }
            else if(state == PageViewModel.PageViewState.Undoing)
            {
                UndoDimmingStoryboard.Begin();
                MoveToDefaultPositionStoryboard.Begin();

                ShowTopToolBar.Pause();
                HideTopToolBar.Begin();

                ShowBottomToolBar.Pause();
                HideBottomToolBar.Begin();
            }
            else if (state == PageViewModel.PageViewState.Empty)
            {
                EmptyDimmingStoryboard.Begin();

                ShowTopToolBar.Pause();
                HideTopToolBar.Begin();

                ShowBottomToolBar.Pause();
                HideBottomToolBar.Begin();
            }
            else if (state == PageViewModel.PageViewState.Previewing)
            {
                CancelDimmingboard.Begin();

                HideTopToolBar.Pause();
                ShowTopToolBar.Begin();

                ShowBottomToolBar.Pause();
                HideBottomToolBar.Begin();
            }

            switch(state)
            {
                case PageViewModel.PageViewState.Selected:
                    BarChartElement.IsHitTestVisible = true;
                    LineChartElement.IsHitTestVisible = true;
                    DistributionViewElement.IsHitTestVisible = true;
                    GroupedBarChartElement.IsHitTestVisible = true;
                    ScatterplotElement.IsHitTestVisible = true;
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
                    break;
            }
        }

        public List<StackPanel> paragraphs = new List<StackPanel>();

        public void UpdateCarousel(Boolean trackPreviousParagraph, String firstChartTag)
        {
            paragraphs.Clear();

            //first chart tag에 해당하는 차트 먼저 추가
            foreach (UIElement child in ParagraphContainerCanvasElement.Children)
            {
                child.UpdateLayout();
                if (child.Visibility == Visibility.Visible && (child as StackPanel).Tag?.ToString() == firstChartTag)
                {
                    paragraphs.Add(child as StackPanel);
                }
            }

            //그 다음 나머지 보이는 차트 추가
            foreach (UIElement child in ParagraphContainerCanvasElement.Children)
            {
                if (child.Visibility == Visibility.Visible && (child as StackPanel).Tag?.ToString() != firstChartTag)
                {
                    paragraphs.Add(child as StackPanel);
                }
            }

            Int32 index = 0;
            foreach (StackPanel paragraph in paragraphs)
            {
                Canvas.SetLeft(paragraph, (Double)App.Current.Resources["ParagraphWidth"] * index);
                index++;
            }
            ParagraphContainerCanvasElement.Width = index * (Double)App.Current.Resources["ParagraphWidth"];

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
                Carousel.ChangeView(sameIndex * (Double)App.Current.Resources["ParagraphWidth"], null, null, true);
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

            if(ViewModel.IsSelected)
                ViewModel.MainPageViewModel.TableViewModel.Reflect(ViewModel.ViewStatus);
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
            ViewModel.StateChanged(this);
            ClearSelection(false);
        }

        private void Undo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ViewModel.State = PageViewModel.PageViewState.Selected; // undo하는 경우는 조건 체크할 필요 없이 무조건 select해도 됨
            ViewModel.StateChanged(this);
        }


        private async void Clipboard_Tapped(object sender, TappedRoutedEventArgs e)
        {
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
            if (delta > SelectionDismissThreshold) delta = SelectionDismissThreshold;
            SelectionIndicatorTemporaryTransform.X = delta;
            SelectionIndicator.Opacity = (SelectionDismissThreshold - delta) / SelectionDismissThreshold;
        }

        private void SelectionIndicator_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Double delta = e.Cumulative.Translation.X;
            if (delta > SelectionDismissThreshold)
            {
                ClearSelection(true);
            }
            else
            {
                ResetSelectionIndicatorPositionStoryboard.Begin();
            }
        }

        private void Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Started)
            {
                FlashStoryboard.Begin();
                Clipboard_Tapped(sender, null);
            }
        }


        private void BroadcastSelection(object sender)
        {
            if (sender != BarChartElement && ViewModel.IsBarChartVisible) BarChartElement.ImportSelection(SelectedRows);
            if (sender != GroupedBarChartElement && ViewModel.IsGroupedBarChartVisible) GroupedBarChartElement.ImportSelection(SelectedRows);
            if (sender != LineChartElement && ViewModel.IsLineChartVisible) LineChart.ImportSelection(SelectedRows);
            //if (sender != DistributionViewElement && ViewModel.IsDistributionVisible) DistributionView.Histogram.ClearSelection(withHandler);
            if (sender != ScatterplotElement && ViewModel.IsScatterplotVisible) ScatterplotElement.ImportSelection(SelectedRows);
        }


        private void ClearSelection(Boolean withHandler)
        {
            if (ViewModel.IsBarChartVisible) BarChartElement.ClearSelection(withHandler);
            if (ViewModel.IsGroupedBarChartVisible) GroupedBarChartElement.ClearSelection(withHandler);
            if (ViewModel.IsLineChartVisible) LineChart.ClearSelection(withHandler);
            if (ViewModel.IsDistributionVisible) DistributionView.Histogram.ClearSelection(withHandler);
            if (ViewModel.IsScatterplotVisible) ScatterplotElement.ClearSelection(withHandler);
        }
    }
}
