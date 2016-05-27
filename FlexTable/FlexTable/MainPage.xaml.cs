using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace FlexTable
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page, IMainPage
    {
        ViewModel.MainPageViewModel mainPageViewModel;

        public View.TableView TableView { get { return TableViewElement; } }
        public View.ExplorationView ExplorationView { get { return ExplorationViewElement; } }
        public TextBlock DummyTextBlock { get { return DummyCell; } }
        public ViewModel.MainPageViewModel ViewModel => mainPageViewModel;
        
        public MainPage()
        {
            mainPageViewModel = new ViewModel.MainPageViewModel(this);
            this.DataContext = mainPageViewModel;
            this.InitializeComponent();
            
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.VisibilityChanged += Current_VisibilityChanged;
        }

        private async void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible) return;
            Logger.Instance.Log("Visibility Changed");
            await Logger.Instance.Flush();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            view.TryEnterFullScreenMode();

            await mainPageViewModel.Initialize();
        }

        /*Boolean isFilterViewVisible = true;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isFilterViewVisible = !isFilterViewVisible;
            if(isFilterViewVisible)
            {
                TableViewColumnDefinition.Width = new GridLength(4, GridUnitType.Star);
                FilterViewColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                TableViewColumnDefinition.Width = new GridLength(5, GridUnitType.Star);
                FilterViewColumnDefinition.Width = new GridLength(0, GridUnitType.Star);
            }
        }
        */

        private void ToggleFilterButtonElement_Checked(object sender, RoutedEventArgs e)
        {
            TableViewColumnDefinition.Width = new GridLength(4, GridUnitType.Star);
            FilterViewColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
        }

        private void ToggleFilterButtonElement_Unchecked(object sender, RoutedEventArgs e)
        {
            TableViewColumnDefinition.Width = new GridLength(5, GridUnitType.Star);
            FilterViewColumnDefinition.Width = new GridLength(0, GridUnitType.Star);
        }
    }
}
