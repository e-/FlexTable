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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.View
{
    public sealed partial class GuidelinePresenter : UserControl
    {
        public Double VerticalOffset
        {
            set
            {
                GuidelineScrollViewer.ChangeView(null, value, null, true);
            }
        }

        public GuidelinePresenter()
        {
            this.InitializeComponent();
        }

        public void SetMaximumRowNumber(Int32 n)
        {
            Int32 maxN = n;

            if (maxN < 50) maxN = 50;
            Double rowHeight = Const.RowHeight;

            GuidelineElement.Children.Clear();

            for (Int32 i = 0; i < maxN; ++i)
            {
                Border border = new Border()
                {
                    Height = rowHeight,
                    Background = (SolidColorBrush)App.Current.Resources["RowGuidelineBrush" + i % 2]
                };

                GuidelineElement.Children.Add(border);
            }
        }
    }
}
