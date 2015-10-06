using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace FlexTable.Model
{
    public class Sheet : NotifyModel
    {
        private String name;
        public String Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }

        private List<Column> columns = new List<Column>();
        public List<Column> Columns { get { return columns; } }

        private List<Row> rows = new List<Row>();
        public List<Row> Rows { get { return rows; } private set { rows = value; OnPropertyChanged("Rows"); } }
    }
}
