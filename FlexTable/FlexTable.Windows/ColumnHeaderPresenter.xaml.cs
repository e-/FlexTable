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

namespace FlexTable
{
    public sealed partial class ColumnHeaderPresenter : UserControl
    {
        private Model.Sheet sheet;
        public Model.Sheet Sheet
        {
            get { return sheet; }
            set
            {
                sheet = value;
                Initialize();
            }
        }

        public Double HorizontalOffset
        {
            set
            {
                ColumnHeaderScrollViewer.ChangeView(value, null, null, true);
            }
        }
        private List<ColumnHeaderCellPresenter> cellPresenters = new List<ColumnHeaderCellPresenter>();

        public ColumnHeaderPresenter()
        {
            this.InitializeComponent();
        }

        public void Initialize()
        {
            foreach (Model.Column column in sheet.Columns)
            {
                ColumnHeaderCellPresenter cellPresenter = new ColumnHeaderCellPresenter(column);
                CellCanvas.Children.Add(cellPresenter);
                cellPresenters.Add(cellPresenter);
                cellPresenter.Update();
            }
        }

        public void UpdateCells()
        {
            foreach (ColumnHeaderCellPresenter cellPresenter in cellPresenters)
            {
                cellPresenter.Update();
            }
        }
    }
}
