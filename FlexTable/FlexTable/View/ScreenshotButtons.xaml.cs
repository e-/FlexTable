using FlexTable.ViewModel;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.View
{
    public sealed partial class ScreenshotButtons : UserControl
    {
        public MainPageViewModel ViewModel { get { return (MainPageViewModel)DataContext; } }

        public ScreenshotButtons()
        {
            this.InitializeComponent();
        }

        public void Show()
        {
            HideStoryboard.Pause();
            ShowStoryboard.Begin();
        }

        public void Hide()
        {
            ShowStoryboard.Pause();
            HideStoryboard.Begin();
        }

        private void CaptureChart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(ViewModel.ExplorationViewModel.SelectedPageViews.Count > 0)
            {
                ViewModel.ExplorationViewModel.SelectedPageViews.Last().Capture();
                Hide();
            }
        }
    }
}
