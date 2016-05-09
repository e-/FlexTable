using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FlexTable.ViewModel;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.View
{
    public sealed partial class FilterPresenter : UserControl, INotifyPropertyChanged
    {
        public FilterListViewModel ViewModel { get { return (FilterListViewModel)DataContext; } }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FilterPresenter()
        {
            this.InitializeComponent();
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            OnPropertyChanged(nameof(ViewModel));
        }

        const Double CancelThreshold = 50;

        private void Border_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Double delta = e.Cumulative.Translation.X;
            if (delta < 0) delta = 0;
            if (delta > CancelThreshold) delta = CancelThreshold;

            WrapperRenderTransform.X = delta;
            Wrapper.Opacity = 1 - delta / CancelThreshold;
        }

        private void Border_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Double delta = e.Cumulative.Translation.X;
            if (delta > CancelThreshold)
            {
                ViewModel.MainPageViewModel.ExplorationViewModel.CancelFilter(ViewModel);
            }
            else
            {
                ResetWrapperStoryboard.Begin();
            }
        }
    }
}
