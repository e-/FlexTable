using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FlexTable.ViewModel;
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
    public sealed partial class RowPresenter : UserControl
    {
        public RowViewModel RowViewModel { get; set; }

        private List<TextBlock> cellPresenters = new List<TextBlock>();

        public RowPresenter()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            if(cellPresenters.Count == 0)
            {
                Style style = App.Current.Resources["CellStyle"] as Style;

                foreach (Model.Cell cell in RowViewModel.Cells)
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
            foreach (Model.Cell cell in RowViewModel.Cells)
            {
                cellPresenters[index].Text = cell.Content.ToString();
                cellPresenters[index].Width = cell.ColumnViewModel.Width;
                Canvas.SetLeft(cellPresenters[index], cell.ColumnViewModel.X);
                index++;
            }
        }
    }
}

