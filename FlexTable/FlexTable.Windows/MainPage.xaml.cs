﻿using System;
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
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace FlexTable
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ViewModel.MainPageViewModel mainPageViewModel = new ViewModel.MainPageViewModel();
        Util.CsvLoader csvLoader = new Util.CsvLoader();

        public MainPage()
        {
            this.DataContext = mainPageViewModel;
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Model.Sheet sheet = await csvLoader.Load();
            sheet.MeasureColumnWidth(DummyCell);
            sheet.UpdateColumnX();

            mainPageViewModel.Sheet = sheet;
            ColumnHeaderPresenter.Sheet = sheet;

            for (Int32 i = 0; i < sheet.RowCount - 1; ++i)
            {
                RowDefinition rd = new RowDefinition()
                {
                    Height = new GridLength((Double)App.Current.Resources["RowHeight"])
                };
                GuidelineGrid.RowDefinitions.Add(rd);

                Rectangle rectangle = new Rectangle()
                {
                    Style = (Style)App.Current.Resources["RowGuidelineStyle"]
                };
                Grid.SetRow(rectangle, i);

                GuidelineGrid.Children.Add(rectangle);
            }

            foreach (Model.Row row in sheet.Rows)
            {
                RowPresenter rowPresenter = new RowPresenter(row);
                TableCanvas.Children.Add(rowPresenter);
                rowPresenter.Update();
                mainPageViewModel.RowPresenters.Add(rowPresenter);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mainPageViewModel.ShuffleRows();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            mainPageViewModel.ShuffleColumns();
            ColumnHeaderPresenter.UpdateCells();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            ColumnHeaderPresenter.HorizontalOffset = sv.HorizontalOffset;
        }
    }
}
