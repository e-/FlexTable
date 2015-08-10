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

using FlexTable.ViewModel;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class ColumnHighlighter : UserControl
    {
        public static readonly DependencyProperty ColumnViewModelProperty =
            DependencyProperty.Register("ColumnViewModel", typeof(ColumnViewModel), typeof(ColumnHighlighter), new PropertyMetadata(null, new PropertyChangedCallback(ColumnViewModelChanged)));

        public ColumnViewModel ColumnViewModel
        {
            get { return (ColumnViewModel)GetValue(ColumnViewModelProperty); }
            set { SetValue(ColumnViewModelProperty, value); }
        }

        public ColumnHighlighter()
        {
            this.InitializeComponent();
        }

        private static void ColumnViewModelChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ColumnHighlighter ch = source as ColumnHighlighter;
            ch.Update();
        }

        public void Update()
        {
            ColumnViewModel columnViewModel = ColumnViewModel;
            TableViewModel tvm = (this.DataContext as TableViewModel);

            Canvas.SetTop(LowerColumn, tvm.Height - (Double)App.Current.Resources["ColumnHeaderHeight"]);

            if (columnViewModel != null)
            {
                Int32 columnIndex = columnViewModel.Index;

                TableCanvas.Children.Clear();

                foreach (ViewModel.RowViewModel rowViewModel in tvm.RowViewModels)
                {
                    TextBlock cell = new TextBlock()
                    {
                        Text = rowViewModel.Cells[columnIndex].Content.ToString(),
                        Style = App.Current.Resources["CellStyle"] as Style,
                        Width = columnViewModel.Width
                    };

                    Canvas.SetTop(cell, rowViewModel.Y);
                    TableCanvas.Children.Add(cell);
                }

                
                Double left = columnViewModel.X - tvm.ScrollLeft;

                if (left - columnViewModel.Width / 2 <= 0)
                {
                    UpperColumn.RenderTransformOrigin = new Point(0, 0);
                    LowerColumn.RenderTransformOrigin = new Point(0, 1);
                }
                else if(left + columnViewModel.Width * 3 / 2 >= tvm.SheetViewWidth)
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

                UpperColumnHeader.Width = LowerColumnHeader.Width = MagnifiedColumn.Width = columnViewModel.Width;
                UpperColumnHeader.Text = LowerColumnHeader.Text = columnViewModel.Column.Name;

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
            }
        }


        private void Darken_Completed(object sender, object e)
        {
            ViewModel.TableViewModel tvm = (this.DataContext as ViewModel.TableViewModel);
            TableScrollViewer.Height = tvm.SheetViewHeight / 2 - (Double)App.Current.Resources["ColumnHeaderHeight"];
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
