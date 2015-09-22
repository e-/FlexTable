using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        public ViewModel.SheetViewModel SheetViewModel
        {
            get
            {
                return (this.DataContext as ViewModel.TableViewModel).SheetViewModel;
            }
        }

        StackPanel visibleNumberElement, hiddenNumberElement;

        public RowHeaderPresenter()
        {
            this.InitializeComponent();
            visibleNumberElement = NumberElement0;
            hiddenNumberElement = NumberElement1;
        }
        
        public void SetMaximumRowNumber(Int32 n)
        {
            Int32 maxN = n;

            if (maxN < 50) maxN = 50;
            Double rowHeight = (Double)App.Current.Resources["RowHeight"];

            for (Int32 i = 0; i < maxN; ++i)
            {
                Border border = new Border(){
                    Height = rowHeight,
                    Background = (SolidColorBrush)App.Current.Resources["RowGuidelineBrush" + i % 2]
                };

                GuidelineElement.Children.Add(border);
            }
        }

        public void SetRowNumber(Int32 n)
        {
            hiddenNumberElement.Children.Clear();

            Style style = App.Current.Resources["RowHeaderStyle"] as Style;

            for (Int32 i = 0; i < n; ++i)
            {
                TextBlock textBlock = new TextBlock()
                {
                    Text = (i + 1).ToString(),
                    Style = style
                };

                hiddenNumberElement.Children.Add(textBlock);
            }

            hiddenNumberElement.Opacity = 0;
            hiddenNumberElement.Visibility = Visibility.Visible;

            Storyboard sb = new Storyboard();

            sb.Children.Add(Util.Animator.Generate(visibleNumberElement, "Opacity", 0));
            sb.Children.Add(Util.Animator.Generate(hiddenNumberElement, "Opacity", 1));

            sb.Completed += sb_Completed;
            sb.Begin();
        }

        void sb_Completed(object sender, object e)
        {
            visibleNumberElement.Visibility = Visibility.Collapsed;
            StackPanel temp;
            temp = visibleNumberElement;
            visibleNumberElement = hiddenNumberElement;
            hiddenNumberElement = temp;
        }
    }
}
