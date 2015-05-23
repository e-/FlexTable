using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Column : INotifyPropertyChanged
    {
        private String name;
        public String Name { get { return name; } set { name = value; } }

        private Int32 index;
        public Int32 Index { get { return index; } set { index = value; } }

        private Double width;
        public Double Width { get { return width; } set { width = value; } }

        private Double x;
        public Double X { get { return x; } set { x = value; OnPropertyChanged("X"); } }

        private Boolean enabled = true;
        public Boolean Enabled { get { return enabled; } set { enabled = value; OnPropertyChanged("Enabled"); } }

        private Boolean highlighted = false;
        public Boolean Highlighted { get { return highlighted; } set { highlighted = value; OnPropertyChanged("Highlighted"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public ColumnType Type { get; set; }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ColumnType GuessColumnType(IEnumerable<String> cellValues)
        {
            Boolean allDouble = true;
            Double result;

            foreach (String value in cellValues)
            {
                if (!Double.TryParse(value, out result))
                {
                    allDouble = false;
                    break;
                }
            }

            if (allDouble) return ColumnType.Double;
            return ColumnType.String;
        }
    }
}
