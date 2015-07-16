using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class RowViewModel : NotifyViewModel
    {
        ViewModel.MainPageViewModel mainPageViewModel;

        private Model.Row row;
        public Model.Row Row { get { return row; } set { row = value; } }

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

        public RowViewModel(MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }
    }
}
