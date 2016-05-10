using FlexTable.Model;
using FlexTable.Util;
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
    public sealed partial class FilterView : UserControl
    {
        public FilterViewModel ViewModel { get { return (FilterViewModel)DataContext; } }
        
        public FilterView()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ColumnViewModel cvm = (sender as Button).DataContext as ColumnViewModel;

            cvm.IsCategoryVisible = !cvm.IsCategoryVisible;
        }
        
        private void CategoryCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Category category = (sender as CheckBox).DataContext as Category;
            ColumnViewModel cvm = category.ColumnViewModel;

            if (cvm.Categories.All(c => c.IsKept)) cvm.IsKept = true;
            else if (cvm.Categories.All(c => !c.IsKept)) cvm.IsKept = false;
            else cvm.IsKept = null;

            Logger.Instance.Log($"filter out,filterview,touch");            

            ViewModel.MainPageViewModel.SheetViewModel.UpdateFilter();
            ViewModel.MainPageViewModel.ReflectAll(ReflectReason.RowFiltered);
        }

        private void CategoryCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            Category c = cb.DataContext as Category;

            if (c != null)
                c.IsKept = true;
        }

        private void CategoryCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            Category c = cb.DataContext as Category;

            if (c != null)
            {
                ColumnViewModel cvm = c.ColumnViewModel;

                if (cvm.Categories.Count(sb => sb.IsKept) > 1)
                {
                    c.IsKept = false;
                    cb.IsChecked = false;
                }
                else
                {
                    c.IsKept = true;
                    cb.IsChecked = true;
                }                
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            // 무조건 checked -> checked 나 partial chcked -> checked 임
            CheckBox cb = sender as CheckBox;
            ColumnViewModel cvm = cb.DataContext as ColumnViewModel;

            cb.IsChecked = true;

            if (cvm != null)
            {
                foreach(Category c in cvm.Categories)
                {
                    c.IsKept = true;
                }
            }

            Logger.Instance.Log($"filter out,filterview,touch");

            ViewModel.MainPageViewModel.SheetViewModel.UpdateFilter();
            ViewModel.MainPageViewModel.ReflectAll(ReflectReason.RowFiltered);
        }
    }
}
