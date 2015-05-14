using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Row : INotifyPropertyChanged
    {
        private ObservableCollection<Cell> cells = new ObservableCollection<Cell>();
        public ObservableCollection<Cell> Cells { get { return cells; } }

        private Int32 index;
        public Int32 Index { 
            get { return index; } 
            set { 
                index = value; 
                OnPropertyChanged("Index");
            }
        }
        public Double Y { get { return index * 20; } }
        public Double Opacity { get { return index < 60 ? 1 : 0; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
