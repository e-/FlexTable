using System;
using System.Collections.Generic;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.View
{
    public sealed partial class PageLabelView : UserControl
    {
        private Int32 activatedParagraphIndex;
        public Int32 ActivatedParagraphIndex { get { return activatedParagraphIndex; } set
            {
                activatedParagraphIndex = value;
                ActivatedParagraphTag = paragraphs?.ElementAtOrDefault(activatedParagraphIndex)?.Tag.ToString();
            }
        }
        public String ActivatedParagraphTag { get; private set; }

        private List<Border> paragraphLabels = new List<Border>();
        private List<StackPanel> paragraphs;

        public event TappedEventHandler LabelTapped;

        public PageLabelView()
        {
            this.InitializeComponent();
        }        

        public void Update(List<StackPanel> paragraphs)
        {
            this.paragraphs = paragraphs;

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
                    ActivatedParagraphIndex = copiedIndex;

                    if (LabelTapped != null)
                        LabelTapped(sender, e);
                    UpdateHighlight();
                };

                border.Child = textBlock;

                ParagraphLabelContainer.Children.Add(border);
                paragraphLabels.Add(border);
                index++;
            }

            paragraphLabels[ActivatedParagraphIndex].Background = new SolidColorBrush((Color)this.Resources["ActivatedLabelBackgroundColor"]);
            (paragraphLabels[ActivatedParagraphIndex].Child as TextBlock).Foreground = new SolidColorBrush((Color)this.Resources["ActivatedLabelForegroundColor"]);
            highlightedParagraphIndex = ActivatedParagraphIndex;
        }
        private Storyboard paragraphLabelStoryboard;
        private Int32 highlightedParagraphIndex;
        public void UpdateHighlight()
        {
            if (highlightedParagraphIndex == ActivatedParagraphIndex)
            {
                return;
            }

            if (paragraphLabelStoryboard != null)
                paragraphLabelStoryboard.Pause();

            highlightedParagraphIndex = ActivatedParagraphIndex;

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
