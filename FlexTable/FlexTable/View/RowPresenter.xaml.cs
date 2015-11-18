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

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class RowPresenter : UserControl
    {
        public ViewModel.RowViewModel RowViewModel { get { return this.DataContext as RowViewModel; } }

        private List<TextBlock> cellPresenters = new List<TextBlock>();
        public List<TextBlock> CellPresenters { get { return cellPresenters; } }

        public Double Y
        {
            set { CompositeTransform.TranslateY = value; }
        }

        public RowPresenter()
        {
            this.InitializeComponent();            
        }

        public void Update()
        {
            //UpdateStoryboard.Begin();
            UpdateCellsWithoutAnimation();
        }

        public void UpdateCellsWithoutAnimation()
        {
            Int32 index = 0;
            foreach (Model.Cell cell in RowViewModel.Cells.OrderBy(c => c.ColumnViewModel.X))
            {
                cellPresenters[index].Text = cell.RawContent;
                cellPresenters[index].Width = cell.ColumnViewModel.Width;
                //Canvas.SetLeft(cellPresenters[index], cell.ColumnViewModel.X);
                if (cell.ColumnViewModel.IsHidden)
                    cellPresenters[index].Opacity = 0.15;
                else
                    cellPresenters[index].Opacity = 1;

                index++;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Style style = App.Current.Resources["CellStyle"] as Style;

            foreach (Model.Cell cell in RowViewModel.Cells.OrderBy(c => c.ColumnViewModel.X))
            {
                TextBlock cellPresenter = new TextBlock()
                {
                    Text = cell.RawContent,
                    Width = cell.ColumnViewModel.Width,
                    Style = style
                };

                // Wrapper.Children.Add(cellPresenter);
                cellPresenters.Add(cellPresenter);

                //Canvas.SetLeft(cellPresenter, cell.ColumnViewModel.X);

                if (cell.ColumnViewModel.IsHidden)
                    cellPresenter.Opacity = 0.15;
            }
        }
    }
}
