using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class ColumnHeaderPresenter : UserControl
    {
        public Double HorizontalOffset
        {
            set
            {
                HeaderScrollViewer.ChangeView(value, null, null, true);
            }
        }

        public ColumnHeaderPresenter()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            foreach (var item in Items.Items)
            {
                ContentPresenter uiElement = (ContentPresenter)Items.ContainerFromItem(item);
                ColumnHeaderCellPresenter chcp = VisualTreeHelper.GetChild(uiElement, 0) as ColumnHeaderCellPresenter;
                chcp.Update();
            }
        }
    }
}
