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

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class PageView : UserControl
    {
        public d3.View.BoxPlot BoxPlot { get { return BoxPlotElement; } }
        public d3.View.BarChart BarChart { get { return BarChartElement; } }
        public d3.View.GroupedBarChart GroupedBarChart { get { return GroupedBarChartElement; } }
        public PivotTableView PivotTableView { get { return PivotTableViewElement; } }

        public ViewModel.PageViewModel PageViewModel { get { return this.DataContext as ViewModel.PageViewModel; } }
        public Storyboard HideStoryboard { get { return HideStoryboardElement; } }
        private Int32 activatedParagraphIndex = 0;
        private List<Border> paragraphLabels = new List<Border>();

        public PageView()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Show.Begin();
        }

        private void Wrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PageViewModel.Tapped(this);
        }

        public void GoDown()
        {
            GoDownStoryboard.Begin();
        }

        public void GoUp()
        {
            GoUpStoryboard.Begin();
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
                    border.Tapped += delegate
                    {
                        GoToParagraph(copiedIndex);
                    };

                    border.Child = textBlock;

                    ParagraphLabelContainer.Children.Add(border);
                    paragraphLabels.Add(border);
                    index++;
                }
            }

            Carousel.UpdateLayout();
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
    }
}
