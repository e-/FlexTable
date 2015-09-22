using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class ColumnHeaderCellPresenter : UserControl
    {
        ViewModel.ColumnViewModel columnViewModel;

        public ColumnHeaderCellPresenter()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            XAnimation.Begin();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            columnViewModel = this.DataContext as ViewModel.ColumnViewModel;
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
            {
                columnViewModel.MainPageViewModel.TableViewModel.IndexedColumnViewModel = columnViewModel;
                columnViewModel.MainPageViewModel.ExplorationViewModel.ShowSummary(columnViewModel);
            }
        }
    }
}
