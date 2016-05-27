using FlexTable.Model;
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
    public sealed partial class SelectionView : UserControl
    {
        public IEnumerable<Row> SelectedRows;
        public SelectionViewModel ViewModel { get { return (SelectionViewModel)DataContext; } }

        public SelectionView()
        {
            this.InitializeComponent();
        }

        public void SetSelection(IEnumerable<Row> selectedRows)
        {
            SelectedRows = selectedRows;

            Int32 count = selectedRows.Count();

            if (count == 0)
            {
                Hide();
            }
            else
            {
                SelectedRowCountIndicator.Text = count.ToString();
                SelectionMessage.Text = count == 1 ? Const.Loader.GetString("SelectionMessage1") : Const.Loader.GetString("SelectionMessage2");
            }
        }

        public void Show()
        {
            HideSelectionIndicatorStoryboard.Pause();
            ShowSelectionIndicatorStoryboard.Begin();
        }

        public void Hide()
        {
            ShowSelectionIndicatorStoryboard.Pause();
            HideSelectionIndicatorStoryboard.Begin();
        }

        private void SelectionIndicatorElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.MainPageViewModel.ReflectAll(ReflectReason.RowSelectionChanged);
        }
    }
}
