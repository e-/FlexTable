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
        private Model.Row row;
        private List<CellPresenter> cellPresenters = new List<CellPresenter>();

        public RowPresenter(Model.Row row)
        {
            this.row = row;
            this.DataContext = row;
            this.InitializeComponent();

            foreach (Model.Cell cell in row.Cells)
            {
                CellPresenter cellPresenter = new CellPresenter(cell);
                CellCanvas.Children.Add(cellPresenter);
                cellPresenters.Add(cellPresenter);
                cellPresenter.Update();
            }
        }

        public void Update()
        {
            Int32 index = (this.DataContext as Model.Row).Index;
            //YAnimation.BeginTime = TimeSpan.FromMilliseconds(index * 20);
            YAnimation.Begin();
        }

        public void UpdateCells()
        {
            foreach (CellPresenter cellPresenter in cellPresenters)
            {
                cellPresenter.Update();
            }
        }
    }
}
