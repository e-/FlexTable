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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.View
{
    public sealed partial class RowPresenter : UserControl
    {
        public RowViewModel RowViewModel { get; set; }

        private List<TextBlock> cellPresenters = new List<TextBlock>();

        public RowPresenter()
        {
            this.InitializeComponent();
        }

        public void Update(ColumnViewModel coloredColumnViewModel, Boolean updateText) {
            Update(coloredColumnViewModel, null, null, null, updateText);
        }

        public void Update(ColumnViewModel coloredColumnViewModel, ColumnViewModel firstColoredViewModel, Boolean updateText) {
            Update(coloredColumnViewModel, firstColoredViewModel, null, null, updateText);
        }

        public void Update(ColumnViewModel coloredColumnViewModel, ColumnViewModel firstColoredViewModel, ColumnViewModel secondColoredViewModel, Boolean updateText)
        {
            Update(coloredColumnViewModel, firstColoredViewModel, secondColoredViewModel, null, updateText);
        }

        public void Update(ColumnViewModel coloredColumnViewModel,
            ColumnViewModel firstColoredColumnViewModel,
            ColumnViewModel secondColoredColumnViewModel,
            ColumnViewModel thirdColoredColumnViewModel,
            Boolean updateText)
        {
            if(cellPresenters.Count == 0)
            {
                Style style = Const.CellStyle;

                foreach (Cell cell in RowViewModel.Cells)
                {
                    TextBlock cellPresenter = new TextBlock()
                    {
                        Text = cell.RawContent,
                        Width = cell.ColumnViewModel.Width,
                        Style = style
                    };

                    CellCanvas.Children.Add(cellPresenter);
                    cellPresenters.Add(cellPresenter);
                }
            }

            Int32 index = 0;
            foreach (Cell cell in RowViewModel.Cells)
            {
                if(updateText)
                    cellPresenters[index].Text = cell.Content.ToString();
                //cellPresenters[index].Width = cell.ColumnViewModel.Width;

                if (cell.ColumnViewModel == coloredColumnViewModel)
                {
                    cellPresenters[index].Foreground = new SolidColorBrush(RowViewModel.Color);
                }
                else if (cell.ColumnViewModel == firstColoredColumnViewModel)
                {
                    cellPresenters[index].Foreground = ViewStatus.Category10FirstSolidColorBrush;
                }
                else if (cell.ColumnViewModel == secondColoredColumnViewModel)
                {
                    cellPresenters[index].Foreground = ViewStatus.Category10SecondSolidColorBrush;
                }
                else if (cell.ColumnViewModel == thirdColoredColumnViewModel)
                {
                    cellPresenters[index].Foreground = ViewStatus.Category10ThirdSolidColorBrush;
                }
                else
                {
                    cellPresenters[index].Foreground = ViewStatus.DefaultRowHeaderSolidColorBrush;
                }

                Canvas.SetLeft(cellPresenters[index], cell.ColumnViewModel.X);
                index++;
            }
        }
    }
}

