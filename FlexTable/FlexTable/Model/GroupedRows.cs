using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class GroupedRows
    {
        private Dictionary<ViewModel.ColumnViewModel, Object> keys = new Dictionary<ViewModel.ColumnViewModel, Object>();
        public Dictionary<ViewModel.ColumnViewModel, Object> Keys { get { return keys; } }

        private List<Row> rows = new List<Row>();
        public List<Row> Rows { get { return rows; } set { rows = value; } }
   } 
}
