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
    public sealed partial class CellPresenter : UserControl
    {
        private Model.Cell cell;

        public CellPresenter(Model.Cell cell)
        {
            this.cell = cell;
            this.DataContext = cell;
            this.InitializeComponent();
        }

        public void Update()
        {
            XAnimation.Begin();
        }

        public void UpdateWithoutAnimation()
        {
            CompositeTransform.TranslateX = (this.DataContext as Model.Cell).ColumnViewModel.X;
        }
    }
}
