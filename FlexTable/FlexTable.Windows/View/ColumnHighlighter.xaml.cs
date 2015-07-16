using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            /*Model.Column column = Column;
            ViewModel.TableViewModel tvm = (this.DataContext as ViewModel.TableViewModel);

            Canvas.SetTop(LowerColumn, tvm.Height - (Double)App.Current.Resources["ColumnHeaderHeight"]);

            if (column != null)
            {                
                Int32 columnIndex = tvm.MainPageViewModel.Sheet.Columns.IndexOf(column);

                TableCanvas.Children.Clear();

                foreach (ViewModel.RowViewModel rowViewModel in tvm.MainPageViewModel.SheetViewModel.RowViewModels)
                {
                    TextBlock cell = new TextBlock()
                    {
                        Text = rowViewModel.Row.Cells[columnIndex].Content.ToString(),
                        Style = App.Current.Resources["CellStyle"] as Style,
                        Width = column.Width
                    };

                    Canvas.SetTop(cell, rowViewModel.Y);
                    TableCanvas.Children.Add(cell);
                }

                
                Double left = column.X - (this.DataContext as ViewModel.MainPageViewModel).TableViewModel.ScrollLeft;
                //Debug.WriteLine("left {0}", left);

                if (left - column.Width / 2 <= 0)
                {
                    UpperColumn.RenderTransformOrigin = new Point(0, 0);
                    LowerColumn.RenderTransformOrigin = new Point(0, 1);
                }
                else if(left + column.Width * 3 / 2 >= tvm.SheetViewWidth)
                {
                    UpperColumn.RenderTransformOrigin = new Point(1, 0);
                    LowerColumn.RenderTransformOrigin = new Point(1, 1);
                }
                else
                {
                    UpperColumn.RenderTransformOrigin = new Point(0.5, 0);
                    LowerColumn.RenderTransformOrigin = new Point(0.5, 1);
                }

                Canvas.SetLeft(MagnifiedColumn, left);

                UpperColumnHeader.Width = LowerColumnHeader.Width = MagnifiedColumn.Width = column.Width;
                //UpperColumnHeader.Text = LowerColumnHeader.Text = mpvm.HighlightedColumn.Name;

                Wrapper.Visibility = Visibility.Visible;

                TableScrollViewer.Height = tvm.SheetViewHeight;
                TableScrollViewer.UpdateLayout();
                TableScrollViewer.ChangeView(null, tvm.ScrollTop, null, true);

                Brighten.Pause();
                Darken.Pause();
                Darken.Begin();
            }
            else
            {
                Darken.Pause();
                Brighten.Pause();
                TableScrollViewer.Height = tvm.SheetViewHeight;
                TableScrollViewer.UpdateLayout();
                Brighten.Begin();
            }*/
        }


        private void Darken_Completed(object sender, object e)
        {
            ViewModel.TableViewModel mpvm = (this.DataContext as ViewModel.TableViewModel);
            TableScrollViewer.Height = mpvm.SheetViewHeight / 2 - (Double)App.Current.Resources["ColumnHeaderHeight"];
        }

        private void Brighten_Completed(object sender, object e)
        {
            Wrapper.Visibility = Visibility.Collapsed;
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // grouping 시작되어야 함
        }
    }
}
