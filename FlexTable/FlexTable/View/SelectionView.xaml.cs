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
        public IEnumerable<Row> SelectedRows = new List<Row>();
        public SelectionViewModel ViewModel { get { return (SelectionViewModel)DataContext; } }

        public SelectionView()
        {
            this.InitializeComponent();
        }

        public void ChangeSelecion(IEnumerable<Row> rows, SelectionChangedType selectionChangedType, Boolean reflectAll)
        {
            if (selectionChangedType == SelectionChangedType.Add)
                SelectedRows = SelectedRows.Concat(rows).Distinct().ToList();
            else if (selectionChangedType == SelectionChangedType.Remove)
                SelectedRows = SelectedRows.Except(rows).Distinct().ToList();
            else if (selectionChangedType == SelectionChangedType.Replace)
                SelectedRows = rows.ToList();
            else
                SelectedRows = new List<Row>();

            Int32 count = SelectedRows.Count();

            if (count == ViewModel.MainPageViewModel.SheetViewModel.FilteredRows.Count())
            {
                SelectedRows = new List<Row>();
                count = 0;
            }

            if (count == 0)
            {
                Hide();
                ViewModel.MainPageViewModel.TableViewModel.CancelPreviewRows();
            }
            else
            {
                Show();
                SelectedRowCountIndicator.Text = count.ToString();
                //SelectionMessage.Text = count == 1 ? Const.Loader.GetString("SelectionMessage1") : Const.Loader.GetString("SelectionMessage2");
                ViewModel.MainPageViewModel.TableViewModel.PreviewRows(SelectedRows);
            }

            if(reflectAll)
                ViewModel.MainPageViewModel.ReflectAll(ReflectReason.RowSelectionChanged);
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
            ChangeSelecion(null, SelectionChangedType.Clear, true);
        }
    }
}
