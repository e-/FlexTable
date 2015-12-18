using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using FlexTable.ViewModel;

namespace FlexTable.Crayon.Chart
{
    public class GroupedBarChartDatum
    {
        public ColumnViewModel ColumnViewModel { get; set; }
        public Object Key { get; set; }

        public List<BarChartDatum> Children { get; set; }
        public IEnumerable<Row> Rows
        {
            get
            {
                return Children?.SelectMany(c => c.Rows);
            }
        }
        public IEnumerable<Row> EnvelopeRows { get { return Children?.SelectMany(c => c.EnvelopeRows); } }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
