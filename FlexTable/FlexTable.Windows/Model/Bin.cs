using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Bin
    {
        public String Name { get; set; }
        public Int32 Count { get; set; }
        public Boolean IsFilteredOut { get; set; }
        public Int32 Index { get; set; }

        public List<Row> Rows { get; set; }
    }
}
