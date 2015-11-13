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

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class PageView : UserControl
    {
        public d3.View.BarChart BarChart => BarChartElement;
        public d3.View.LineChart LineChart => LineChartElement;
        public DescriptiveStatisticsView DescriptiveStatisticsView => DescriptiveStatisticsViewElement;
        public CorrelationStatisticsView CorrelationStatisticsView => CorrelationStatisticsViewElement;
        public DistributionView DistributionView => DistributionViewElement;
        public d3.View.GroupedBarChart GroupedBarChart => GroupedBarChartElement;
        public d3.View.Scatterplot Scatterplot => ScatterplotElement;
        public PivotTableView PivotTableView => PivotTableViewElement;

        public StackPanel BarChartTitle => BarChartTitleElement;
        public StackPanel LineChartTitle => LineChartTitleElement;
        public StackPanel DistributionViewTitle => DistributionViewTitleElement;
        public StackPanel DescriptiveStatisticsTitle => DescriptiveStatisticsTitleElement;
        public StackPanel GroupedBarChartTitle => GroupedBarChartTitleElement;
        public StackPanel ScatterplotTitle => ScatterplotTitleElement;
        public StackPanel PivotTableTitle => PivotTableTitleElement;
        public StackPanel CorrelationStatisticsTitle => CorrelationStatisticsTitleElement;

        public PageViewModel PageViewModel => (this.DataContext as PageViewModel);
        public Storyboard HideStoryboard => HideStoryboardElement;
        public Storyboard CancelUndoStoryboard => CancelUndoOpacityStoryboardElement;
        private Int32 activatedParagraphIndex = 0;
        public String ActivatedParagraphTag { get; set; }
        private List<Border> paragraphLabels = new List<Border>();
        private List<StackPanel> paragraphs = new List<StackPanel>();
        

        public PageView()
        {
            this.InitializeComponent();

            BarChartElement.BarPointerPressed += BarChartElement_BarPointerPressed;
            BarChartElement.BarPointerReleased += BarChartElement_BarPointerReleased;

            GroupedBarChartElement.BarPointerPressed += GroupedBarChartElement_BarPointerPressed;
            GroupedBarChartElement.BarPointerReleased += BarChartElement_BarPointerReleased;

            LineChartElement.LinePointerPressed += LineChartElement_LinePointerPressed;
            LineChartElement.LinePointerReleased += LineChartElement_LinePointerReleased;

            ScatterplotElement.CategoryPointerPressed += ScatterplotElement_CategoryPointerPressed;
            ScatterplotElement.CategoryPointerReleased += ScatterplotElement_CategoryPointerReleased;
            ScatterplotElement.LassoSelected += ScatterplotElement_LassoSelected;
            ScatterplotElement.LassoUnselected += ScatterplotElement_LassoUnselected;
        }

        #region Visualization Event Handlers
        private void ScatterplotElement_LassoSelected(object sender, object datum, int index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;

            List<Int32> indices = datum as List<Int32>;
            pvm.MainPageViewModel.TableViewModel.PreviewRows(
                r => indices.IndexOf(r.Index) >= 0
            );
        }

        private void ScatterplotElement_LassoUnselected(object sender, object datum, int index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;
            pvm.MainPageViewModel.TableViewModel.CancelPreviewRows();
        }

        private void ScatterplotElement_CategoryPointerPressed(object sender, object datum, int index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;

            Category category = (datum as Tuple<Object, Int32>).Item1 as Category;
            pvm.MainPageViewModel.TableViewModel.PreviewRows(pvm.ScatterplotRowSelecter(category));
        }

        private void ScatterplotElement_CategoryPointerReleased(object sender, object datum, int index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;
            pvm.MainPageViewModel.TableViewModel.CancelPreviewRows();
        }

        private void LineChartElement_LinePointerPressed(object sender, object datum, int index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;

            Series series = datum as Series;
            pvm.MainPageViewModel.TableViewModel.PreviewRows(pvm.LineChartRowSelecter(series));
        }

        private void LineChartElement_LinePointerReleased(object sender, object datum, int index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;
            pvm.MainPageViewModel.TableViewModel.CancelPreviewRows();
        }

        private void GroupedBarChartElement_BarPointerPressed(object sender, object d, Int32 index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;

            Tuple<Object, Object, Double, Object> datum = d as Tuple<Object, Object, Double, Object>;
            pvm.MainPageViewModel.TableViewModel.PreviewRows(pvm.GroupedBarChartRowSelecter(datum.Item1 as Category, datum.Item4 as Category));
        }

        void BarChartElement_BarPointerPressed(object sender, object d, Int32 index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;
            
            Tuple<Object, Double> datum = d as Tuple<Object, Double>;
            pvm.MainPageViewModel.TableViewModel.PreviewRows(pvm.BarChartRowSelecter(datum.Item1 as Category));
        }

        void BarChartElement_BarPointerReleased(object sender, object d, Int32 index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;
            pvm.MainPageViewModel.TableViewModel.CancelPreviewRows();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowStoryboardElement.Begin();
        }

        private void Wrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*if(e.PointerDeviceType == PointerDeviceType.Touch)
                PageViewModel.Tapped(this, false);
                */
        }
        
        #endregion
        
        public void MoveToSelectedPosition(Boolean isUndo)
        {
            if (isUndo) UndoSelectStoryboard.Begin();
            else SelectStoryboard.Begin();
        }
        
        public void MoveToUnselectedPosition()
        {
            UnselectStoryboard.Begin();
        }

        public void EnterSelectedMode()
        {
            BarChartElement.IsHitTestVisible = true;
            LineChartElement.IsHitTestVisible = true;
            DistributionViewElement.IsHitTestVisible = true;
            GroupedBarChartElement.IsHitTestVisible = true;
            ScatterplotElement.IsHitTestVisible = true;
            PageViewModel.IsUndoing = false;
            PageViewModel.IsEmpty = false;
        }

        public void EnterUndoMode()
        {
            UndoStoryboard.Begin();

            PageViewModel.IsPrimaryUndoMessageVisible = true;
            BarChartElement.IsHitTestVisible = false;
            LineChartElement.IsHitTestVisible = false;
            DistributionViewElement.IsHitTestVisible = false;
            GroupedBarChartElement.IsHitTestVisible = false;
            ScatterplotElement.IsHitTestVisible = false;
        }

        private void UndoStoryboard_Completed(object sender, object e)
        {
            PageViewModel.IsUndoing = true;
        }

        #region Carousel 
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
            foreach(StackPanel paragraph in paragraphs)
            {
                Canvas.SetLeft(paragraph, (Double)App.Current.Resources["ParagraphWidth"] * index);
                index++;
            }
            ParagraphContainerCanvasElement.Width = index * (Double)App.Current.Resources["ParagraphWidth"] * index;

            // 먼저 전에 보이던 시각화와 같은 시각화가 있는지 본다. 있으면 그걸 먼저 보여줌 (바, 산점도)에서 산점도를 보다 바가 없어지면 그대로 산점도를 보여주고 싶음

            index = 0;
            Int32 sameIndex = 0;
            StackPanel sameParagraph = null;
            foreach(StackPanel paragraph in paragraphs)
            {
                if (paragraph.Tag?.ToString() == ActivatedParagraphTag)
                {
                    sameParagraph = paragraph;
                    sameIndex = index;
                }
                index++;
            }

            if (sameParagraph != null && trackPreviousParagraph)
            {
                activatedParagraphIndex = sameIndex;
                Carousel.ChangeView(sameIndex * (Double)App.Current.Resources["ParagraphWidth"], null, null, true);
            }
            else
            {
                // 아니면 현재 위치를 유지해보자
                // 0으로 초기화

                Double offset = Carousel.HorizontalOffset;
                Double width = Carousel.ActualWidth;

                Carousel.ChangeView(0, null, null, true);
                activatedParagraphIndex = 0;
                ActivatedParagraphTag = paragraphs[activatedParagraphIndex].Tag.ToString();
                /*activatedParagraphIndex = (Int32)Math.Ceiling((offset - width / 2) / width);
                ActivatedParagraphTag = paragraphs[activatedParagraphIndex].Tag.ToString();*/
            }
        }

        public void UpdatePageLabelContainer()
        {
            ParagraphLabelContainer.Children.Clear();
            paragraphLabels.Clear();            

            Int32 index = 0;
            foreach (StackPanel paragraph in paragraphs)
            {
                Border border = new Border()
                {
                    Style = this.Resources["ParagraphLabelBorderStyle"] as Style,
                    Background = new SolidColorBrush((Color)this.Resources["LabelBackgroundColor"])
                };

                TextBlock textBlock = new TextBlock()
                {
                    Text = (paragraph as FrameworkElement).Tag.ToString(),
                    Style = this.Resources["ParagraphLabelTextBlockStyle"] as Style,
                    Foreground = new SolidColorBrush((Color)this.Resources["LabelForegroundColor"])
                };

                Int32 copiedIndex = index;
                border.Tapped += delegate (object sender, TappedRoutedEventArgs e)
                {
                    GoToParagraph(copiedIndex);
                    e.Handled = true;
                };

                border.Child = textBlock;

                ParagraphLabelContainer.Children.Add(border);
                paragraphLabels.Add(border);
                index++;
            }

            paragraphLabels[activatedParagraphIndex].Background = new SolidColorBrush((Color)this.Resources["ActivatedLabelBackgroundColor"]);
            (paragraphLabels[activatedParagraphIndex].Child as TextBlock).Foreground = new SolidColorBrush((Color)this.Resources["ActivatedLabelForegroundColor"]);
            highlightedParagraphIndex = activatedParagraphIndex;
        }

        private void Carousel_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                Double offset = Carousel.HorizontalOffset;
                Double width = Carousel.ActualWidth;

                activatedParagraphIndex = (Int32)Math.Ceiling((offset - width / 2) / width);
                ActivatedParagraphTag = paragraphs[activatedParagraphIndex].Tag.ToString();
                Double newOffset = activatedParagraphIndex * width;

                Carousel.ChangeView(newOffset, null, null);

                if (highlightedParagraphIndex != activatedParagraphIndex)
                    HighlightParagraphLabel(activatedParagraphIndex);
            }
        }

        private void Carousel_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            Double offset = Carousel.HorizontalOffset;
            Double width = Carousel.ActualWidth;

            Int32 paragraphIndex = (Int32)Math.Ceiling((offset - width / 2) / width);
        }

        public void GoToParagraph(Int32 index)
        {
            activatedParagraphIndex = index;
            ActivatedParagraphTag = paragraphs[activatedParagraphIndex].Tag.ToString();
            HighlightParagraphLabel(index);
            Double width = Carousel.ActualWidth;
            Double newOffset = activatedParagraphIndex * width;

            Carousel.ChangeView(newOffset, null, null);
        }

        private Storyboard paragraphLabelStoryboard;    
        private Int32 highlightedParagraphIndex;
        public void HighlightParagraphLabel(Int32 highlightedParagraphIndex)
        {
            this.highlightedParagraphIndex = highlightedParagraphIndex;

            if (paragraphLabelStoryboard != null)
                paragraphLabelStoryboard.Pause();

            paragraphLabelStoryboard = new Storyboard();
            Int32 index = 0;
            foreach (UIElement child in ParagraphLabelContainer.Children)
            {
                ColorAnimation background = new ColorAnimation()
                {
                    EasingFunction = App.Current.Resources["QuarticEaseInOut"] as QuarticEase,
                    Duration = TimeSpan.FromMilliseconds(300),
                    To = (Color)(index == highlightedParagraphIndex ? this.Resources["ActivatedLabelBackgroundColor"] : this.Resources["LabelBackgroundColor"])
                };

                Storyboard.SetTarget(background, child);
                Storyboard.SetTargetProperty(background, "(Border.Background).(SolidColorBrush.Color)");

                ColorAnimation foreground = new ColorAnimation()
                {
                    EasingFunction = App.Current.Resources["QuarticEaseInOut"] as QuarticEase,
                    Duration = TimeSpan.FromMilliseconds(300),
                    To = (Color)(index == highlightedParagraphIndex ? this.Resources["ActivatedLabelForegroundColor"] : this.Resources["LabelForegroundColor"])
                };

                Storyboard.SetTarget(foreground, (child as Border).Child);
                Storyboard.SetTargetProperty(foreground, "(TextBlock.Foreground).(SolidColorBrush.Color)");

                paragraphLabelStoryboard.Children.Add(background);
                paragraphLabelStoryboard.Children.Add(foreground);
                index++;
            }

            paragraphLabelStoryboard.Begin();
        }

        #endregion

        private void Undo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            PageViewModel.IsUndoing = false;
            PageViewModel.StatusChanged(this, true);
        }

        private void Wrapper_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch) return;
            if (PageViewModel.IsEmpty) { }
            else if(PageViewModel.IsUndoing)
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta >= 0) delta = 0;
                if (delta < -PageViewModel.MainPageViewModel.PageOffset) { delta = -PageViewModel.MainPageViewModel.PageOffset; }
                Canvas.SetTop(this, PageViewModel.MainPageViewModel.PageOffset + delta);
                ChartWrapper.Opacity = 0.2 - 0.8 * delta / PageViewModel.MainPageViewModel.PageOffset;
                PageViewModel.IsPrimaryUndoMessageVisible = delta > -50;
            }
            else if (PageViewModel.IsSelected) // 선택 된 경우 내릴수만 있음
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta <= 0) delta = 0;
                if (delta > PageViewModel.MainPageViewModel.PageHeight) { delta = PageViewModel.MainPageViewModel.PageHeight; }
                Canvas.SetTop(this, delta);
                ChartWrapper.Opacity = 0.2 + 0.8 * (1 - delta / PageViewModel.MainPageViewModel.PageHeight);
                Canvas.SetZIndex(this, 10);
            }
            else // 선택되지 않은 경우 올릴수만 있음
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta >= 0) delta = 0;
                if (delta < -PageViewModel.MainPageViewModel.PageOffset) { delta = -PageViewModel.MainPageViewModel.PageOffset; }
                Canvas.SetTop(this, PageViewModel.MainPageViewModel.PageOffset + delta);
            }            
        }

        private void Wrapper_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch) return;
            if (PageViewModel.IsEmpty) { }
            else if (PageViewModel.IsUndoing)
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta < -Const.PageViewToggleThreshold) // 많이 올라간 경우
                {
                    Undo_Tapped(sender, new TappedRoutedEventArgs());
                }
                else
                {
                    MoveToUnselectedPosition();
                    EnterUndoMode();
                }
            }
            else if (PageViewModel.IsSelected) // 선택 된 경우 내릴수만 있음
            {
                Double delta = e.Cumulative.Translation.Y;
                if (delta > Const.PageViewToggleThreshold) // 많이 내려간 경우
                {
                    MoveToUnselectedPosition();
                    EnterUndoMode();
                    PageViewModel.StatusChanged(this, false);
                }
                else
                {
                    MoveToSelectedPosition(false);
                }
                Canvas.SetZIndex(this, 0);
            }
            else // 선택되지 않은 경우 올릴수만 있음
            {
                Double delta = e.Cumulative.Translation.Y;
                if(delta < -Const.PageViewToggleThreshold) // 많이 올라간 경우
                {
                    MoveToSelectedPosition(false);
                    EnterSelectedMode();
                    PageViewModel.StatusChanged(this, false);
                }
                else
                {
                    MoveToUnselectedPosition();
                }
            }
        }

        private void Carousel_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Wrapper_ManipulationDelta(sender, e);
        }

        private void Carousel_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Wrapper_ManipulationCompleted(sender, e);
        }

    }
}
