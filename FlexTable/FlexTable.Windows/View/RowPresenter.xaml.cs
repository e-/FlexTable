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

        private List<CellPresenter> cellPresenters = new List<CellPresenter>();

        public Double Y
        {
            set { CompositeTransform.TranslateY = value; }
        }

        public RowPresenter(ViewModel.RowViewModel rowViewModel)
        {
            this.rowViewModel = rowViewModel;
            this.DataContext = rowViewModel;
            this.InitializeComponent();

            foreach (Model.Cell cell in rowViewModel.Cells)
            {
                CellPresenter cellPresenter = new CellPresenter(cell);
                CellCanvas.Children.Add(cellPresenter);
                cellPresenters.Add(cellPresenter);
                cellPresenter.Update();
            }
        }

        public void Update()
        {
            UpdateStoryboard.Begin();
        }

        public void UpdateCells()
        {
            foreach (CellPresenter cellPresenter in cellPresenters)
            {
                cellPresenter.Update();
            }
        }

        public void UpdateCellsWithoutAnimation()
        {
            foreach (CellPresenter cellPresenter in cellPresenters)
            {
                cellPresenter.UpdateWithoutAnimation();
            }
        }
    }
}
