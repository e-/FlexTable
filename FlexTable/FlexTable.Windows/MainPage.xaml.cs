using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace FlexTable
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page, IMainPage
    {
        ViewModel.MainPageViewModel mainPageViewModel;
        Util.CsvLoader csvLoader = new Util.CsvLoader();

        public View.TableView TableView { get { return TableViewElement; } }

        public MainPage()
        {
            mainPageViewModel = new ViewModel.MainPageViewModel(this);
            this.DataContext = mainPageViewModel;
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Model.Sheet sheet = await csvLoader.Load();
            sheet.MeasureColumnWidth(DummyCell);
            sheet.UpdateColumnX();
            sheet.GuessColumnType();
            sheet.CreateColumnSummary();
            mainPageViewModel.Sheet = sheet;

            TableView.AddGuidelines(sheet.Rows.Count);
        }
       
        private void Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            mainPageViewModel.CancelGroupBy();
            mainPageViewModel.ChartViewModel.Hide();
        }
    }
}
