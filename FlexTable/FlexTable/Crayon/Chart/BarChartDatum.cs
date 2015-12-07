using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.ViewModel;

namespace FlexTable.Crayon.Chart
{
    public class BarChartDatum
    {
        public ColumnViewModel ColumnViewModel { get; set; }
        public Object Key { get; set; }
        public Double Value { get; set; }
        public GroupedBarChartDatum Parent { get; set; }
        public IEnumerable<Model.Row> Rows { get; set; }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
