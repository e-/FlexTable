using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class ColumnHighlighter : UserControl
    {
        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register("Column", typeof(Model.Column), typeof(ColumnHighlighter), new PropertyMetadata(null, new PropertyChangedCallback(ColumnChanged)));

        public Model.Column Column
        {
            get { return (Model.Column)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }

        public ColumnHighlighter()
        {
            this.InitializeComponent();
        }

        private static void ColumnChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ColumnHighlighter ch = source as ColumnHighlighter;
            ch.Update();
        }

        public void Update()
        {
            Model.Column column = Column;
            ViewModel.MainPageViewModel mpvm = (this.DataContext as ViewModel.MainPageViewModel);

            Canvas.SetTop(LowerHeader, mpvm.Height - (Double)App.Current.Resources["ColumnHeaderHeight"]);

            if (column != null)
            {                
                Int32 columnIndex = mpvm.Sheet.Columns.IndexOf(column);

                TableCanvas.Children.Clear();

                foreach (Model.Row row in mpvm.Sheet.Rows)
                {
                    TextBlock cell = new TextBlock()
                    {
                        Text = row.Cells[columnIndex].Content.ToString(),
                        Style = App.Current.Resources["CellStyle"] as Style,
                        Width = column.Width
                    };

                    Canvas.SetTop(cell, row.Y);
                    TableCanvas.Children.Add(cell);
                }

                Wrapper.Visibility = Visibility.Visible;

                Brighten.Pause();
                Darken.Pause();
                Darken.Begin();

                
                Canvas.SetLeft(MagnifiedColumn, column.X - (this.DataContext as ViewModel.MainPageViewModel).ScrollLeft);

                UpperColumnHeader.Width = LowerColumnHeader.Width = MagnifiedColumn.Width = column.Width;
                UpperColumnHeader.Text = LowerColumnHeader.Text = mpvm.HighlightedColumn.Name;
            }
            else
            {
                Darken.Pause();
                Brighten.Pause();
                TableScrollViewer.Height = mpvm.Height;
                Brighten.Begin();
            }
        }


        private void Darken_Completed(object sender, object e)
        {
            ViewModel.MainPageViewModel mpvm = (this.DataContext as ViewModel.MainPageViewModel);
            TableScrollViewer.Height = mpvm.SheetViewHeight / 2 - (Double)App.Current.Resources["ColumnHeaderHeight"];
        }

        private void Brighten_Completed(object sender, object e)
        {
            Wrapper.Visibility = Visibility.Collapsed;
        }

    }
}
