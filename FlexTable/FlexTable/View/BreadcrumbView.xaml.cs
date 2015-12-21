using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.View
{
    public sealed partial class BreadcrumbView : UserControl
    {
        public PageViewModel ViewModel => (PageViewModel)DataContext;

        public BreadcrumbView()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            StackPanelElement.Children.Clear();
            Int32 index = 0;

            foreach(ColumnViewModel columnViewModel in ViewModel.ViewStatus.SelectedColumnViewModels)
            {
                Boolean isLast = index == ViewModel.ViewStatus.SelectedColumnViewModels.Count() - 1;
                Boolean isFirst = index == 0;

                if (!isFirst)
                {
                    TextBlock caret = new TextBlock()
                    {
                        Text = "\xf0da",
                        Style = Resources["CaretStyle"] as Style
                    };
                    StackPanelElement.Children.Add(caret);
                }

                StackPanel stack = new StackPanel()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Orientation = Orientation.Horizontal
                };

                TextBlock textBlock = new TextBlock()
                {
                    Text = columnViewModel.Column.Name,
                    Style = Resources["ColumnStyle"] as Style
                };

                Border border = new Border()
                {
                    Child = new TextBlock()
                    {
                        Text = columnViewModel.Type.ToString()[0].ToString(),
                        Style = Resources["ColumnTypeTextBlockStyle"] as Style
                    },
                    Style = Resources["ColumnTypeStyle"] as Style,
                    Background = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)) // columnViewModel.Type == Model.ColumnType.Categorical ? Color.FromArgb(255, 95, 49, 109) : Color.FromArgb(255, 41, 161, 156))
                };

                stack.Children.Add(border);
                stack.Children.Add(textBlock);                

                StackPanelElement.Children.Add(stack);

                index++;
            }
        }
    }
}
