using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class ColumnViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Model.Column column;
        public Model.Column Column { get { return column; } set { column = value; OnPropertyChanged("Column"); } }        

        private ViewModel.MainPageViewModel mainPageViewModel;       

        public ColumnViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void Highlight()
        {
            mainPageViewModel.HighlightColumn(column);
        }

        public void Unhighlight()
        {
            mainPageViewModel.UnhighlightColumn(column);
        }

        public void MarkEnabled()
        {
            mainPageViewModel.MarkColumnEnabled(column);
        }

        public void MarkDisabled()
        {
            mainPageViewModel.MarkColumnDisabled(column);
        }

        public void SortAscending()
        {
            mainPageViewModel.Sort(column, false);
        }

        public void SortDescending()
        {
            mainPageViewModel.Sort(column, true);
        }
    }
}
