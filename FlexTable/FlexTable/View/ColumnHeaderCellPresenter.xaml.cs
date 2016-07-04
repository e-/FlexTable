using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FlexTable.ViewModel;
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
        public ColumnViewModel ViewModel => (ColumnViewModel)this.DataContext;
        public ColumnHeaderCellPresenter()
        {
            this.InitializeComponent();
        }

        public VerticalAlignment ContentVerticalAlignment
        {
            set { ColumnStackPanel.VerticalAlignment = value; }
        }

        public void Update(Double delayBeforeAnimation)
        {
            XAnimation.BeginTime = TimeSpan.FromMilliseconds(delayBeforeAnimation);
            XAnimation.Begin();
        }

        private void Wrapper_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Wrapper.CapturePointer(e.Pointer);
            ViewModel.MainPageViewModel.View.TableView.ColumnHighlighter.Update(ViewModel);
            ViewModel.MainPageViewModel.ExplorationViewModel.PreviewColumn(ViewModel);
        }

        public void AlignContent(VerticalAlignment va)
        {
            Wrapper.VerticalAlignment = va;
        }

        public void HideTopTypeIndicators()
        {
            TopIndicators.Visibility = Visibility.Collapsed;
        }

        public void HideBottomTypeIndicators()
        {
            BottomIndicators.Visibility = Visibility.Collapsed;
        }
    }
}
