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
        private Int32 activatedParagraphIndex = 0;
        private List<Border> paragraphLabels = new List<Border>();

        public PageView()
        {
            this.InitializeComponent();

            BarChartElement.BarPointerPressed += BarChartElement_BarPointerPressed;
            BarChartElement.BarPointerReleased += BarChartElement_BarPointerReleased;

            GroupedBarChartElement.BarPointerPressed += GroupedBarChartElement_BarPointerPressed;
            GroupedBarChartElement.BarPointerReleased += BarChartElement_BarPointerReleased;

            LineChartElement.LinePointerPressed += LineChartElement_LinePointerPressed;
            LineChartElement.LinePointerReleased += LineChartElement_LinePointerReleased;
        }

        private void LineChartElement_LinePointerPressed(object sender, object datum, int index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;

            Series series = datum as Series;
            pvm.MainPageViewModel.TableViewModel.PreviewRows(pvm.LineChartRowSelector(series));
        }


        private void LineChartElement_LinePointerReleased(object sender, object datum, int index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;
            pvm.MainPageViewModel.TableViewModel.CancelPreviewRows();
        }


        private void GroupedBarChartElement_BarPointerPressed(object sender, object d, Int32 index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;

            Tuple<Object, Object, Double> datum = d as Tuple<Object, Object, Double>;
            pvm.MainPageViewModel.TableViewModel.PreviewRows(pvm.GroupedBarChartRowSelecter(datum.Item1 as Model.Category, datum.Item2 as Model.Category));
        }

        void BarChartElement_BarPointerPressed(object sender, object d, Int32 index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;
            
            Tuple<Object, Double> datum = d as Tuple<Object, Double>;
            pvm.MainPageViewModel.TableViewModel.PreviewRows(pvm.BarChartRowSelecter(datum.Item1 as Model.Category));
        }

        void BarChartElement_BarPointerReleased(object sender, object d, Int32 index)
        {
            PageViewModel pvm = this.DataContext as PageViewModel;
            pvm.MainPageViewModel.TableViewModel.CancelPreviewRows();
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Show.Begin();
        }

        private void Wrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PageViewModel.Tapped(this);
        }

        private void TitleWrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ParagraphLabelContainer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public void GoDown()
        {
            GoDownStoryboard.Begin();
            BarChartElement.IsHitTestVisible = true;
            LineChartElement.IsHitTestVisible = true;
            DistributionViewElement.IsHitTestVisible = true;
            GroupedBarChartElement.IsHitTestVisible = true;
            ScatterplotElement.IsHitTestVisible = true;
        }

        public void GoUp()
        {
            GoUpStoryboard.Begin();
            BarChartElement.IsHitTestVisible = false;
            LineChartElement.IsHitTestVisible = false;
            DistributionViewElement.IsHitTestVisible = false;
            GroupedBarChartElement.IsHitTestVisible = false;
            ScatterplotElement.IsHitTestVisible = false;
        }

        private void GoUpStoryboard_Completed(object sender, object e)
        {
            ChartWrapper.Opacity = 1;
            PageViewModel.Hide();
        }

        public void UpdateCarousel()
        {
            ParagraphLabelContainer.Children.Clear();
            paragraphLabels.Clear();
            Carousel.UpdateLayout();

            Int32 index = 0;
            foreach (UIElement child in ParagraphContainer.Children)
            {
                if (child.Visibility == Visibility.Visible)
                {
                    Border border = new Border()
                    {
                        Style = this.Resources["ParagraphLabelBorderStyle"] as Style,
                        Background = new SolidColorBrush((Color)this.Resources["LabelBackgroundColor"])
                    };

                    TextBlock textBlock = new TextBlock()
                    {
                        Text = (child as FrameworkElement).Tag.ToString(),
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
            }

            Double offset = Carousel.HorizontalOffset;
            Double width = Carousel.ActualWidth;

            activatedParagraphIndex = (Int32)Math.Ceiling((offset - width / 2) / width);
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
                Double newOffset = activatedParagraphIndex * width;

                Carousel.ChangeView(newOffset, null, null);
            }
        }

        private void Carousel_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            Double offset = Carousel.HorizontalOffset;
            Double width = Carousel.ActualWidth;

            Int32 paragraphIndex = (Int32)Math.Ceiling((offset - width / 2) / width);

            if (highlightedParagraphIndex != paragraphIndex)
                HighlightParagraphLabel(paragraphIndex);
        }

        public void GoToParagraph(Int32 index)
        {
            activatedParagraphIndex = index;
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

        public void BarPointerPressed(Object sender, Object datum)
        {

        }
    }
}
