using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace FlexTable.Model
{
    public class RowHeader : INotifyPropertyChanged
    {
        private Int32 index;
        public Int32 Index
        {
            get { return index; }
            set
            {
                index = value; 
                OnPropertyChanged("IndexString"); 
                OnPropertyChanged("Top");
            }
        }

        private Double opacity;
        public Double Opacity
        {
            get { return opacity; }
            set { opacity = value; OnPropertyChanged("Opacity"); }
        }

        public String IndexString { get { return index.ToString(); } }
        public Double Top { get { return (index - 1) * Const.RowHeight; } }
        public SolidColorBrush Background { get { return (SolidColorBrush)App.Current.Resources["RowGuidelineBrush" + (index+1) % 2]; } }
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
