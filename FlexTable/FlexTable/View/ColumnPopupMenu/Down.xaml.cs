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

namespace FlexTable.View.ColumnPopupMenu
{
    public sealed partial class Down : UserControl
    {
        public Down()
        {
            this.InitializeComponent();
        }

        public void Show()
        {
            ShowStoryboard.Begin();
        }

        public void Hide()
        {
            HideStoryboard.Begin();
        }

        public void Highlight()
        {
            HighlightStoryboard.Begin();
        }

        public void Unhighlight()
        {
            UnHighlightStoryboard.Begin();
        }
    }
}
