using FlexTable.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class RowViewModel : NotifyViewModel
    {
        MainPageViewModel mainPageViewModel;

        private Int32 index;
        public Int32 Index
        {
            get { return index; }
            set
            {
                index = value;
                OnPropertyChanged("Index");
                OnPropertyChanged("IndexFromOne");
            }
        }
        public Int32 IndexFromOne { get { return index + 1; } }

        public Double Y { get { return index * (Double)App.Current.Resources["RowHeight"]; } }
        public Boolean IsFilteredOut { get; set; }

        private ObservableCollection<Cell> cells = new ObservableCollection<Cell>();
        public ObservableCollection<Cell> Cells { get { return cells; } }

        public RowViewModel(MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }
    }
}
