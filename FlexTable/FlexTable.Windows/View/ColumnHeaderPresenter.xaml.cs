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
            if (Items.Children.Count == 0)
            {
                ViewModel.SheetViewModel sheetViewModel = (this.DataContext as ViewModel.TableViewModel).SheetViewModel;
                foreach (ViewModel.ColumnViewModel cvm in sheetViewModel.ColumnViewModels)
                {
                    ColumnHeaderCellPresenter chcp = new ColumnHeaderCellPresenter()
                    {
                        DataContext = cvm
                    };
                    Items.Children.Add(chcp);
                }
            }

            foreach (UIElement ele in Items.Children)
            {
                ColumnHeaderCellPresenter chcp = ele as ColumnHeaderCellPresenter;
                chcp.Update();
            }

            /*Items.UpdateLayout();
            foreach (var item in Items.Items)
            {
                var uiElement = Items.ContainerFromItem(item);
                ColumnHeaderCellPresenter chcp = VisualTreeHelper.GetChild(uiElement, 0) as ColumnHeaderCellPresenter;
                chcp.Update();
            }*/
        }
    }
}
