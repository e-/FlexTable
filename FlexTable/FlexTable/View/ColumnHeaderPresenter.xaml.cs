using FlexTable.ViewModel;
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

        public TableViewModel ViewModel => (TableViewModel)DataContext;

        public ColumnHeaderPresenter()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            Update(0);
        }

        public void Update(Double delayBeforeAnimation)
        {
            if (Items.Children.Count == 0)
            {
                SheetViewModel sheetViewModel = (this.DataContext as TableViewModel).SheetViewModel;
                foreach (ColumnViewModel cvm in sheetViewModel.ColumnViewModels)
                {
                    ColumnHeaderCellPresenter chcp = new ColumnHeaderCellPresenter()
                    {
                        DataContext = cvm
                    };

                    if (this.Name == "BottomColumnHeaderElement")
                    {
                        chcp.ContentVerticalAlignment = VerticalAlignment.Top;
                    }
                    else
                    {
                        chcp.ContentVerticalAlignment = VerticalAlignment.Bottom;
                    }
                    Items.Children.Add(chcp);
                }
            }

            foreach (UIElement ele in Items.Children)
            {
                ColumnHeaderCellPresenter chcp = ele as ColumnHeaderCellPresenter;
                chcp.Update(delayBeforeAnimation);
            }
        }

        public void Reset()
        {
            Items.Children.Clear();
        }
    }
}
