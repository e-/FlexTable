using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Crayon.Chart
{
    public class BarChartDatum
    {
        public Object Key { get; set; }
        public Double Value { get; set; }
        public IEnumerable<Model.Row> Rows { get; set; }
    }
}
