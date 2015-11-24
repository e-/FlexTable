using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.ViewModel;

namespace FlexTable.Crayon.Chart
{
    public class GroupedBarChartDatum
    {
        public ColumnViewModel ColumnViewModel { get; set; }
        public Object Key { get; set; }

        public List<BarChartDatum> Children { get; set; }
        public IEnumerable<Model.Row> Rows
        {
            get
            {
                return Children?.SelectMany(c => c.Rows);
            }
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
