using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class SummaryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Model.Column column;
        public Model.Column Column { get { return column; } set { column = value; OnPropertyChanged("Column"); } }

        ViewModel.MainPageViewModel mainPageViewModel;

        public SummaryViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void Summarize(Model.Column column)
        {
            Column = column;
        }
    }
}
