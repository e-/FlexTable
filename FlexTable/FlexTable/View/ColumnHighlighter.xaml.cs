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
using Windows.UI.Xaml.Media.Animation;
using FlexTable.Model;
using FlexTable.Util;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class ColumnHighlighter : UserControl
    {
        public ColumnViewModel ColumnViewModel { get; private set; }

        public ColumnHighlighter()
        {
            this.InitializeComponent();
        }
        
        public void Update(ColumnViewModel columnViewModel)
        {
            ColumnViewModel = columnViewModel;

            TableViewModel tvm = (this.DataContext as TableViewModel);

            Canvas.SetTop(LowerColumnHeaderWrapperElement, tvm.Height - Const.ColumnHeaderHeight);

            if (columnViewModel != null)
            {
                Int32 columnIndex = columnViewModel.Index;
                Int32 index = 0;

                foreach (RowViewModel rowViewModel in tvm.ActivatedRowViewModels)
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
                            Style = Const.CellStyle
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

                    Canvas.SetLeft(UpperPopupElement, 60);
                    Canvas.SetLeft(LowerPopupElement, 60);
                }
                else if (left + columnViewModel.Width * 3 / 2 >= tvm.SheetViewWidth)
                {
                    UpperColumn.RenderTransformOrigin = new Point(1, 0);
                    LowerColumnHeaderWrapperElement.RenderTransformOrigin = new Point(1, 1);

                    Canvas.SetLeft(UpperPopupElement, columnViewModel.Width - 60);
                    Canvas.SetLeft(LowerPopupElement, columnViewModel.Width - 60);
                }
                else
                {
                    UpperColumn.RenderTransformOrigin = new Point(0.5, 0);
                    LowerColumnHeaderWrapperElement.RenderTransformOrigin = new Point(0.5, 1);

                    Canvas.SetLeft(UpperPopupElement, columnViewModel.Width / 2);
                    Canvas.SetLeft(LowerPopupElement, columnViewModel.Width / 2);
                }

                Canvas.SetLeft(MagnifiedColumn, left);

                UpperColumnHeaderWrapperElement.Width = LowerColumnHeaderWrapperElement.Width = MagnifiedColumn.Width = columnViewModel.Width;
                UpperColumnHeaderWrapperElement.DataContext = LowerColumnHeaderWrapperElement.DataContext = columnViewModel;

                Canvas.SetTop(UpperPopupElement, Const.ColumnHeaderHeight * 0.2);
                Canvas.SetTop(LowerPopupElement, columnViewModel.MainPageViewModel.Bounds.Height - Const.ColumnHeaderHeight * 1.7);

                Wrapper.Visibility = Visibility.Visible;

                TableScrollViewer.Height = tvm.SheetViewHeight;
                TableScrollViewer.ChangeView(null, tvm.ScrollTop, null, true);

                if (Const.PopupMenuEnabled)
                {
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
                }

                Brighten.Stop();
                if(Darken.GetCurrentState() != ClockState.Active)
                    Darken.Begin();
            }
            else
            {
                TableScrollViewer.Height = tvm.SheetViewHeight;
                if (Brighten.GetCurrentState() != ClockState.Active)
                    Brighten.Begin();
                Darken.Stop();

                if (Const.PopupMenuEnabled)
                {
                    UpperDownMenuElement.Hide();
                    UpperLeftMenuElement.Hide();
                    UpperRightMenuElement.Hide();

                    LowerUpMenuElement.Hide();
                    LowerLeftMenuElement.Hide();
                    LowerRightMenuElement.Hide();
                }
            }
        }
        
        private void Darken_Completed(object sender, object e)
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);
            //TableScrollViewer.Height = tvm.SheetViewHeight / (Double)this.Resources["ZoomScale"] - Const.ColumnHeaderHeight / 2 / (Double)this.Resources["ZoomScale"];
        }
        
        enum Command { Left, Right, Up, Down, None };

        private void UpperColumnHeaderWrapperElement_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Touch) return;
            UpperColumnHeaderWrapperElement.CapturePointer(e.Pointer);
            if (!Const.PopupMenuEnabled) return;
            UpperDownMenuElement.Show();
            UpperLeftMenuElement.Show();
            UpperRightMenuElement.Show();
        }
        
        Command upperSelected = Command.None;

        private void UpperColumnHeaderWrapperElement_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (ColumnViewModel == null) return;

            Double x = e.GetCurrentPoint(UpperColumnHeaderWrapperElement).Position.X,
                   y = e.GetCurrentPoint(UpperColumnHeaderWrapperElement).Position.Y,
                   height = Const.ColumnHeaderHeight;

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

            if (upperSelected != newSelected)
            {
                UnhighlightUpperMenu(upperSelected);
                HighlightUpperMenu(newSelected);

                upperSelected = newSelected;
            }
        }
    
        private void UpperColumnHeaderWrapperElement_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);

            if (ColumnViewModel != null)
            {
                Boolean processed = ProcessUpperCommand();
                if(!processed)
                {
                    SelectAndCancelHighlight();
                }
            }

            if (!tvm.IsIndexing)
            {
                tvm.CancelIndexing(true);
            }
        }

        public Boolean ProcessUpperCommand()
        {
            if (!Const.PopupMenuEnabled) return false;

            TableViewModel tvm = (this.DataContext as TableViewModel);
            Boolean processed = false;

            switch (upperSelected)
            {
                case(Command.Left):
                    tvm.MainPageViewModel.SheetViewModel.BringFront(ColumnViewModel);
                    tvm.MainPageViewModel.ReflectAll(ReflectReason.ColumnShown);
                    UpperLeftMenuElement.Unhighlight();
                    tvm.CancelIndexing(true);
                    processed = true;
                    break;
                case(Command.Right):
                    tvm.MainPageViewModel.SheetViewModel.SetAside(ColumnViewModel);
                    tvm.MainPageViewModel.ReflectAll(ReflectReason.ColumnHidden);
                    UpperRightMenuElement.Unhighlight();
                    tvm.CancelIndexing(true);
                    processed = true;
                    break;
                case(Command.Down):
                    tvm.MainPageViewModel.SheetViewModel.Sort(ColumnViewModel, SortOption.Ascending);
                    tvm.MainPageViewModel.ReflectAll(ReflectReason.ColumnSorted);
                    UpperDownMenuElement.Unhighlight();
                    tvm.CancelIndexing(true);
                    processed = true;
                    break;
            }

            upperSelected = Command.None;
            UpperDownMenuElement.Hide();
            UpperLeftMenuElement.Hide();
            UpperRightMenuElement.Hide();
            return processed;
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
            LowerColumnHeaderWrapperElement.CapturePointer(e.Pointer);
            if (!Const.PopupMenuEnabled) return;
            LowerUpMenuElement.Show();
            LowerRightMenuElement.Show();
            LowerLeftMenuElement.Show();
        }

        Command lowerSelected = Command.None;

        private void LowerColumnHeaderWrapperElement_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (ColumnViewModel == null) return;
            
            Double x = e.GetCurrentPoint(LowerColumnHeaderWrapperElement).Position.X,
                   y = e.GetCurrentPoint(LowerColumnHeaderWrapperElement).Position.Y,
                   height = Const.ColumnHeaderHeight;

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

        private void LowerColumnHeaderWrapperElement_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);

            if (ColumnViewModel != null)
            {
                Boolean processed = ProcessLowerCommand();

                if (!processed)
                {
                    SelectAndCancelHighlight();
                }
            }

            if (!tvm.IsIndexing)
            {
                tvm.CancelIndexing(true);
            }
        }

        public Boolean ProcessLowerCommand()
        {
            if (!Const.PopupMenuEnabled) return false;

            TableViewModel tvm = (this.DataContext as TableViewModel);
            Boolean processed = false;

            switch (lowerSelected)
            {
                case (Command.Left):
                    tvm.MainPageViewModel.SheetViewModel.BringFront(ColumnViewModel);
                    tvm.MainPageViewModel.ReflectAll(ReflectReason.ColumnShown);// 2.SelectionChanged);
                    LowerLeftMenuElement.Unhighlight();
                    tvm.CancelIndexing(true);
                    processed = true;
                    break;
                case (Command.Right):
                    tvm.MainPageViewModel.SheetViewModel.SetAside(ColumnViewModel);
                    tvm.MainPageViewModel.ReflectAll(ReflectReason.ColumnHidden);// 2.SelectionChanged);
                    LowerRightMenuElement.Unhighlight();
                    tvm.CancelIndexing(true);
                    processed = true;
                    break;
                case (Command.Up):
                    tvm.MainPageViewModel.SheetViewModel.Sort(ColumnViewModel, SortOption.Descending);
                    tvm.MainPageViewModel.ReflectAll(ReflectReason.ColumnSorted); // 2.SelectionChanged);
                    LowerUpMenuElement.Unhighlight();
                    tvm.CancelIndexing(true);
                    processed = true;
                    break;
            }

            lowerSelected = Command.None;
            LowerUpMenuElement.Hide();
            LowerLeftMenuElement.Hide();
            LowerRightMenuElement.Hide();
            return processed;
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

        void SelectAndCancelHighlight() // 아무 메뉴도 선택하지 않고 바로 떼는 경우 
        {
            TableViewModel tvm = (this.DataContext as TableViewModel);
            var viewModel = tvm.MainPageViewModel.ExplorationViewModel.TopPageView.ViewModel;

            Logger.Instance.Log("column header tapped in ColumnHighlighter");
            if (viewModel.IsEmpty)
            {
                tvm.CancelIndexing(true);
                return;
            }

            if (viewModel.IsNoPossibleVisualizationWarningVisible)
            {
                tvm.CancelIndexing(true);
                return;
            }

            viewModel.State = PageViewModel.PageViewState.Selected;
            viewModel.StateChanged(tvm.MainPageViewModel.ExplorationViewModel.TopPageView);

            if(!tvm.IsIndexing)
                tvm.CancelIndexing(true);
        }
    }
}
