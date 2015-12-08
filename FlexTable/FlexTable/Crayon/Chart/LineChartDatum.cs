using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using FlexTable.ViewModel;

namespace FlexTable.Crayon.Chart
{
    public class LineChartDatum
    {
        public ColumnViewModel ColumnViewModel { get; set; }
        public Object Key { get; set; }

        public List<DataPoint> DataPoints { get; set; }

        public IEnumerable<Row> Rows { get; set; }

        public override string ToString()
        {
            return Key.ToString();
        }
    }

    public class DataPoint
    {
        public Object Item1 { get; set; }
        public Object Item2 { get; set; }
        public LineChartDatum Parent { get; set; }
        public IEnumerable<Row> Rows { get; set; }

        public DataPoint(Object item1, Object item2, LineChartDatum parent, IEnumerable<Row> rows)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Parent = parent;
            this.Rows = rows;
        }
    }
}
