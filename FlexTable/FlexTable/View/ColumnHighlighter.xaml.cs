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
using Windows.Devices.Input;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class ColumnHighlighter : UserControl
    {
        public ColumnViewModel ColumnViewModel { get; set; }

        public ColumnHighlighter()
        {
            this.InitializeComponent();
        }

        public void Update()
        {
            ColumnViewModel columnViewModel = ColumnViewModel;
            TableViewModel tvm = (this.DataContext as TableViewModel);

            Canvas.SetTop(LowerColumnHeaderWrapperElement, tvm.Height - (Double)App.Current.Resources["ColumnHeaderHeight"]);

            if (columnViewModel != null)
            {
                Int32 columnIndex = columnViewModel.Index;
                Int32 index = 0;

                foreach (RowViewModel rowViewModel in tvm.RowViewModels)
                {
                    TextBlock cell;
                    if (index < TableCanvas.Children.Count)
                    {
                        cell = TableCanvas.Children[index] as TextBlock;
                        cell.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        cell = new TextBlock()
                        {
                            Style = App.Current.Resources["CellStyle"] as Style
                        };
                        TableCanvas.Children.Add(cell);
                    }

                    String text = rowViewModel.Cells[columnIndex].Content.ToString();                    
                    cell.Text = text;
                    cell.Width = columnViewModel.Width;
                    Canvas.SetTop(cell, rowViewModel.Y);
                    index++;
                }

                for (Int32 i = index; i < TableCanvas.Children.Count; ++i)
                {
                    TableCanvas.Children[i].Visibility = Visibility.Collapsed;
                }
                
                Double left = columnViewModel.X - tvm.ScrollLeft;

                if (left - columnViewModel.Width / 2 <= 0)
                {
                    UpperColumn.RenderTransformOrigin = new Point(0, 0);
                    LowerColumnHeaderWrapperElement.RenderTransformOrigin = new Point(0, 1);
                }
                else if(left + columnViewModel.Width * 3 / 2 >= tvm.SheetViewWidth)
                {
                    UpperColumn.RenderTransformOrigin = new Point(1, 0);
                    LowerColumnHeaderWrapperElement.RenderTransformOrigin = new Point(1, 1);
                }
                else
                {
                    UpperColumn.RenderTransformOrigin = new Point(0.5, 0);
                    LowerColumnHeaderWrapperElement.RenderTransformOrigin = new Point(0.5, 1);
                }

                Canvas.SetLeft(MagnifiedColumn, left);

                UpperColumnHeaderWrapperElement.Width = LowerColumnHeaderWrapperElement.Width = MagnifiedColumn.Width = columnViewModel.Width;
                UpperColumnHeaderWrapperElement.DataContext = LowerColumnHeaderWrapperElement.DataContext = columnViewModel;

                Canvas.SetLeft(UpperPopupElement, columnViewModel.Width / 2);
                Canvas.SetLeft(LowerPopupElement, columnViewModel.Width / 2);

                Canvas.SetTop(LowerPopupElement, columnViewModel.MainPageViewModel.Bounds.Height - (Double)App.Current.Resources["ColumnHeaderHeight"] * 2);

                Wrapper.Visibility = Visibility.Visible;

                TableScrollViewer.Height = tvm.SheetViewHeight;
                TableScrollViewer.UpdateLayout();
                TableScrollViewer.ChangeView(null, tvm.ScrollTop, null, true);


                if (columnViewModel.IsHidden)
                {
                    UpperRightMenuElement.Visibility = Visibility.Collapsed;
                    UpperLeftMenuElement.Visibility = Visibility.Visible;
                    LowerRightMenuElement.Visibility = Visibility.Collapsed;
                    LowerLeftMenuElement.Visibility = Visibility.Visible;
                }
                else
                {
                    UpperRightMenuElement.Visibility = Visibility.Visible;
                    UpperLeftMenuElement.Visibility = Visibility.Collapsed;
                    LowerRightMenuElement.Visibility = Visibility.Visible;
                    LowerLeftMenuElement.Visibility = Visibility.Collapsed;
                }

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
            TableViewModel tvm = (this.DataContext as TableViewModel);
            TableScrollViewer.Height = tvm.SheetViewHeight / 2 - (Double)App.Current.Resources["ColumnHeaderHeight"];
        }

        private void Brighten_Completed(object sender, object e)
        {
            Wrapper.Visibility = Visibility.Collapsed;
        }

        enum Command { Left, Right, Up, Down, None };

        private void UpperColumnHeaderWrapperElement_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Touch) return;
            UpperDownMenuElement.Show();
            UpperLeftMenuElement.Show();
            UpperRightMenuElement.Show();
        }
        
        Command upperSelected = Command.None;

        private void UpperColumnHeaderWrapperElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (ColumnViewModel == null) return;

            Double x = e.Position.X,
                   y = e.Position.Y,
                   height = (Double)App.Current.Resources["ColumnHeaderHeight"];

            Command newSelected = Command.None;

            if(x < 10 && 0 < y && y < height) {
                newSelected = Command.Left;
            }
            else if (x > ColumnViewModel.Width - 10 && 0 < y && y < height)
            {
                newSelected = Command.Right;
            }
            else if (0 <= x && x < ColumnViewModel.Width && y < 10)
            {
                newSelected = Command.Up;
            }
            else if (0 <= x && x < ColumnViewModel.Width && y > height - 10)
            {
                newSelected = Command.Down;
            }
            else
            {
                newSelected = Command.None;
            }

            if (upperSelected != newSelected)
            {
                UnhighlightUpperMenu(upperSelected);
                HighlightUpperMenu(newSelected);

                upperSelected = newSelected;
            }
        }

        private void UpperColumnHeaderWrapperElement_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);

            if (ColumnViewModel != null)
                ProcessUpperCommand();

            if (!tvm.IsIndexing)
            {
                tvm.MainPageViewModel.View.TableView.ColumnHighlighter.ColumnViewModel = null;
                tvm.MainPageViewModel.View.TableView.ColumnHighlighter.Update();
                tvm.MainPageViewModel.View.ExplorationView.TopPageViewModel.Hide();
            }
        }

        private void UpperColumnHeaderWrapperElement_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);

            if (ColumnViewModel != null)
                ProcessUpperCommand();

            if (!tvm.IsIndexing)
            {
                tvm.MainPageViewModel.View.TableView.ColumnHighlighter.ColumnViewModel = null;
                tvm.MainPageViewModel.View.TableView.ColumnHighlighter.Update();
                tvm.MainPageViewModel.View.ExplorationView.TopPageViewModel.Hide();
            }
        }

        public void ProcessUpperCommand()
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);
            switch (upperSelected)
            {
                case(Command.Left):
                    tvm.MainPageViewModel.SheetViewModel.BringFront(ColumnViewModel);
                    UpperLeftMenuElement.Unhighlight();
                    tvm.CancelIndexing();
                    break;
                case(Command.Right):
                    tvm.MainPageViewModel.SheetViewModel.SetAside(ColumnViewModel);
                    UpperRightMenuElement.Unhighlight();
                    tvm.CancelIndexing();
                    break;
                case(Command.Down):
                    tvm.MainPageViewModel.TableViewModel.Sort(ColumnViewModel, Model.SortOption.Descending);
                    UpperDownMenuElement.Unhighlight();
                    tvm.CancelIndexing();
                    break;
            }

            upperSelected = Command.None;
            UpperDownMenuElement.Hide();
            UpperLeftMenuElement.Hide();
            UpperRightMenuElement.Hide();
        }
        
        void HighlightUpperMenu(Command command)
        {
            switch (command)
            {
                case Command.Down:
                    UpperDownMenuElement.Highlight();
                    break;
                case Command.Left:
                    UpperLeftMenuElement.Highlight();
                    break;
                case Command.Right:
                    UpperRightMenuElement.Highlight();
                    break;
            }
        }

        void UnhighlightUpperMenu(Command command)
        {
            switch (command)
            {
                case Command.Down:
                    UpperDownMenuElement.Unhighlight();
                    break;
                case Command.Left:
                    UpperLeftMenuElement.Unhighlight();
                    break;
                case Command.Right:
                    UpperRightMenuElement.Unhighlight();
                    break;
                case Command.None:
                    UpperDownMenuElement.Unhighlight();
                    UpperLeftMenuElement.Unhighlight();
                    UpperRightMenuElement.Unhighlight();
                    break;
            }
        }

        private void LowerColumnHeaderWrapperElement_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Touch) return;
            LowerUpMenuElement.Show();
            LowerRightMenuElement.Show();
            LowerLeftMenuElement.Show();
        }

        Command lowerSelected = Command.None;

        private void LowerColumnHeaderWrapperElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (ColumnViewModel == null) return;

            Double x = e.Position.X,
                   y = e.Position.Y,
                   height = (Double)App.Current.Resources["ColumnHeaderHeight"];

            Command newSelected = Command.None;

            if (x < 10 && 0 < y && y < height)
            {
                newSelected = Command.Left;
            }
            else if (x > ColumnViewModel.Width - 10 && 0 < y && y < height)
            {
                newSelected = Command.Right;
            }
            else if (0 <= x && x < ColumnViewModel.Width && y < 10)
            {
                newSelected = Command.Up;
            }
            else if (0 <= x && x < ColumnViewModel.Width && y > height - 10)
            {
                newSelected = Command.Down;
            }
            else
            {
                newSelected = Command.None;
            }

            if (lowerSelected != newSelected)
            {
                UnhighlightLowerMenu(lowerSelected);
                HighlightLowerMenu(newSelected);

                lowerSelected = newSelected;
            }
        }

        private void LowerColumnHeaderWrapperElement_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);

            if (ColumnViewModel != null)
                ProcessLowerCommand();

            if (!tvm.IsIndexing)
            {
                tvm.MainPageViewModel.View.TableView.ColumnHighlighter.ColumnViewModel = null;
                tvm.MainPageViewModel.View.TableView.ColumnHighlighter.Update();
                tvm.MainPageViewModel.View.ExplorationView.TopPageViewModel.Hide();
            }
        }

        private void LowerColumnHeaderWrapperElement_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);

            if (ColumnViewModel != null)
                ProcessLowerCommand();

            if (!tvm.IsIndexing)
            {
                tvm.MainPageViewModel.View.TableView.ColumnHighlighter.ColumnViewModel = null;
                tvm.MainPageViewModel.View.TableView.ColumnHighlighter.Update();
                tvm.MainPageViewModel.View.ExplorationView.TopPageViewModel.Hide();
            }
        }

        public void ProcessLowerCommand()
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);
            switch (lowerSelected)
            {
                case (Command.Left):
                    tvm.MainPageViewModel.SheetViewModel.BringFront(ColumnViewModel);
                    LowerLeftMenuElement.Unhighlight();
                    tvm.CancelIndexing();
                    break;
                case (Command.Right):
                    tvm.MainPageViewModel.SheetViewModel.SetAside(ColumnViewModel);
                    LowerRightMenuElement.Unhighlight();
                    tvm.CancelIndexing();
                    break;
                case (Command.Up):
                    tvm.MainPageViewModel.TableViewModel.Sort(ColumnViewModel, Model.SortOption.Ascending);
                    LowerUpMenuElement.Unhighlight();
                    tvm.CancelIndexing();
                    break;
            }

            lowerSelected = Command.None;
            LowerUpMenuElement.Hide();
            LowerLeftMenuElement.Hide();
            LowerRightMenuElement.Hide();
        }

        void HighlightLowerMenu(Command command)
        {
            switch (command)
            {
                case Command.Up:
                    LowerUpMenuElement.Highlight();
                    break;
                case Command.Left:
                    LowerLeftMenuElement.Highlight();
                    break;
                case Command.Right:
                    LowerRightMenuElement.Highlight();
                    break;
            }
        }

        void UnhighlightLowerMenu(Command command)
        {
            switch (command)
            {
                case Command.Up:
                    LowerUpMenuElement.Unhighlight();
                    break;
                case Command.Left:
                    LowerLeftMenuElement.Unhighlight();
                    break;
                case Command.Right:
                    LowerRightMenuElement.Unhighlight();
                    break;
                case Command.None:
                    LowerUpMenuElement.Unhighlight();
                    LowerLeftMenuElement.Unhighlight();
                    LowerRightMenuElement.Unhighlight();
                    break;
            }
        }
    }
}
