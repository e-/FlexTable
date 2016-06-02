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
using FlexTable.Model;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.View
{
    public sealed partial class AnimatingRowPresenter : UserControl
    {
        public AnimatingRowViewModel AnimatingRowViewModel { get; set; }

        public TextBlock CellPresenter { get; set; }

        public AnimatingRowPresenter()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            Style style = Const.CellStyle;
            Cell cell = AnimatingRowViewModel.RowViewModel.Cells[AnimatingRowViewModel.ColumnViewModel.Index];

            CellPresenter = new TextBlock()
            {
                Text = cell.RawContent,
                Width = AnimatingRowViewModel.ColumnViewModel.Width,
                Style = style
            };

            CellCanvas.Children.Add(CellPresenter);
                                    
            Canvas.SetLeft(CellPresenter, AnimatingRowViewModel.X);
        }
    }
}
