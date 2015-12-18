using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using FlexTable.ViewModel;

namespace FlexTable.Crayon.Chart
{
    public enum LineState
    {
        Default,
        Unselected,
        Selected
    }

    public class LineChartDatum
    {
        public ColumnViewModel ColumnViewModel { get; set; }
        public Object Key { get; set; }

        public List<DataPoint> DataPoints { get; set; }

        public LineState LineState
        {
            get
            {
                if (Rows == null) return LineState.Default;
                if (Rows.Count() == 0) return LineState.Unselected;
                return LineState.Selected;
            }
        }

        public IEnumerable<Row> EnvelopeRows
        {
            get { return DataPoints.SelectMany(dp => dp.EnvelopeRows); }
        }

        public IEnumerable<Row> Rows
        {
            get {
                if (DataPoints.All(dp => dp.Rows == null)) return null;
                else return DataPoints.Where(dp => dp.Rows != null).SelectMany(dp => dp.Rows);
            }
        }
        
        public override string ToString()
        {
            return Key.ToString();
        }
    }

    public class DataPoint
    {
        public Object Item1 { get; set; }
        public Object Item2 { get; set; }
        public Object EnvelopeItem2 { get; set; }

        public LineChartDatum Parent { get; set; }

        public IEnumerable<Row> Rows { get; set; }
        public IEnumerable<Row> EnvelopeRows { get; set; }
    }
}
