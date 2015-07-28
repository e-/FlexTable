using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class GroupedRows
    {
        private Dictionary<ViewModel.ColumnViewModel, Category> keys = new Dictionary<ViewModel.ColumnViewModel, Category>();
        public Dictionary<ViewModel.ColumnViewModel, Category> Keys { get { return keys; } }

        private List<Row> rows = new List<Row>();
        public List<Row> Rows { get { return rows; } set { rows = value; } }
    }
}
