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
    public sealed partial class RowPresenter : UserControl
    {
        private Model.Row row;

        public RowPresenter(Model.Row row)
        {
            this.row = row;
            this.InitializeComponent();
        }

        public void Update()
        {
            Int32 index = (this.DataContext as Model.Row).Index;
            if (index < 60)
                Wrapper.Opacity = .2;
            else
                Wrapper.Opacity = 0;
            YAnimation.BeginTime = TimeSpan.FromMilliseconds(index * 20);
            YAnimation.Begin();
        }
    }
}
