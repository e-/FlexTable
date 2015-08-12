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

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class RowPresenter : UserControl
    {
        private ViewModel.RowViewModel rowViewModel;
        public ViewModel.RowViewModel RowViewModel { get { return rowViewModel; } }

        private List<TextBlock> cellPresenters = new List<TextBlock>();
        public List<TextBlock> CellPresenters { get { return cellPresenters; } }

        public Double Y
        {
            set { CompositeTransform.TranslateY = value; }
        }

        public RowPresenter(ViewModel.RowViewModel rowViewModel)
        {
            this.rowViewModel = rowViewModel;
            this.DataContext = rowViewModel;
            this.InitializeComponent();

            Style style = App.Current.Resources["CellStyle"] as Style;

            foreach (Model.Cell cell in rowViewModel.Cells)
            {
                TextBlock cellPresenter = new TextBlock(){
                    Text = cell.RawContent,
                    Width = cell.ColumnViewModel.Width,
                    Style = style
                };

                CellCanvas.Children.Add(cellPresenter);
                cellPresenters.Add(cellPresenter);

                Canvas.SetLeft(cellPresenter, cell.ColumnViewModel.X);

                if (cell.ColumnViewModel.IsHidden)
                    cellPresenter.Opacity = 0.15;
            }
        }

        public void Update()
        {
            UpdateStoryboard.Begin();
        }

        public void UpdateCellsWithoutAnimation()
        {
            Int32 index = 0;
            foreach (Model.Cell cell in rowViewModel.Cells)
            {
                Canvas.SetLeft(cellPresenters[index], cell.ColumnViewModel.X);
                index++;
            }
        }
    }
}
