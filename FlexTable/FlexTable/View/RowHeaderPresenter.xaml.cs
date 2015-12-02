using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FlexTable.Model;
using FlexTable.ViewModel;
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
    public sealed partial class RowHeaderPresenter : UserControl
    {
        public Double VerticalOffset
        {
            set
            {
                HeaderScrollViewer.ChangeView(null, value, null, true);
            }
        }

        public SheetViewModel SheetViewModel
        {
            get
            {
                return (this.DataContext as TableViewModel).SheetViewModel;
            }
        }

        StackPanel visibleNumberElement, hiddenNumberElement;
        Int32 maximumRowNumber;

        public RowHeaderPresenter()
        {
            this.InitializeComponent();
            visibleNumberElement = NumberElement0;
            hiddenNumberElement = NumberElement1;
        }

        Storyboard last = null;

        public void SetRowMaximumNumber(Int32 maximumRowNumber)
        {
            this.maximumRowNumber = maximumRowNumber;

            Style style = App.Current.Resources["RowHeaderStyle"] as Style;

            NumberElement0.Children.Clear();
            NumberElement1.Children.Clear();

            for (Int32 i = 0; i < maximumRowNumber; ++i)
            {
                TextBlock textBlock1 = new TextBlock()
                {
                    Text = (i + 1).ToString(),
                    Style = style
                };

                NumberElement0.Children.Add(textBlock1);

                TextBlock textBlock2 = new TextBlock()
                {
                    Text = (i + 1).ToString(),
                    Style = style
                };

                NumberElement1.Children.Add(textBlock2);
            }
        }        

        public void SetRowNumber(List<Color> colors, Int32 highlightRowNumber, Int32 visibleRowNumber)
        {
            if (last != null) last.Pause();

            // visible 은 세팅되어있으니까 흐리게, hidden은 세팅하고 밝게

            Int32 i;
            Int32 count = colors.Count;

            for (i = 0; i < highlightRowNumber; ++i)
            {
                hiddenNumberElement.Children[i].Visibility = Visibility.Visible;
                hiddenNumberElement.Children[i].Opacity = 1;

                if (i < count)
                    (hiddenNumberElement.Children[i] as TextBlock).Foreground = new SolidColorBrush(colors[i]);
                else
                    (hiddenNumberElement.Children[i] as TextBlock).Foreground = ViewStatus.DefaultRowHeaderSolidColorBrush;
            }

            for (; i < visibleRowNumber; ++i)
            {
                hiddenNumberElement.Children[i].Visibility = Visibility.Visible;
                hiddenNumberElement.Children[i].Opacity = 0.2;
                if (i < count)
                    (hiddenNumberElement.Children[i] as TextBlock).Foreground = new SolidColorBrush(colors[i]);
                else
                    (hiddenNumberElement.Children[i] as TextBlock).Foreground = ViewStatus.DefaultRowHeaderSolidColorBrush;
            }

            for (; i < maximumRowNumber; ++i)
            {
                hiddenNumberElement.Children[i].Visibility = Visibility.Collapsed;
            }

            Storyboard sb = new Storyboard();

            sb.Children.Add(Util.Animator.Generate(visibleNumberElement, "Opacity", 0));
            sb.Children.Add(Util.Animator.Generate(hiddenNumberElement, "Opacity", 1));

            StackPanel temp;
            temp = visibleNumberElement;
            visibleNumberElement = hiddenNumberElement;
            hiddenNumberElement = temp;

            sb.Begin();
            last = sb;
        }
    }
}
