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

        Boolean holding = false;
        private void Wrapper_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Wrapper.CapturePointer(e.Pointer);
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen) return;
            
            ViewModel.MainPageViewModel.ExplorationViewModel.PreviewColumn(ViewModel);
            holding = false;
        }

        private void Wrapper_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen) return;
            if (holding) return;
            var viewModel = ViewModel.MainPageViewModel.ExplorationViewModel.TopPageView.ViewModel;

            viewModel.State = PageViewModel.PageViewState.Selected;
            viewModel.StateChanged(viewModel.MainPageViewModel.ExplorationViewModel.TopPageView);
        }

        private void Wrapper_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                holding = true;
                ViewModel.MainPageViewModel.View.TableView.ColumnHighlighter.Update(ViewModel);
            }
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
