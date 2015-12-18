using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.ViewModel;
using FlexTable.Model;

namespace FlexTable.Crayon.Chart
{
    public enum BarState
    {
        Default,
        Unselected,
        PartiallySelected,
        FullySelected
    }

    public class BarChartDatum
    {
        public ColumnViewModel ColumnViewModel { get; set; }
        public Object Key { get; set; }

        public Double EnvelopeValue { get; set; }
        public Double Value { get; set; }

        public GroupedBarChartDatum Parent { get; set; }

        public IEnumerable<Row> EnvelopeRows { get; set; }
        public IEnumerable<Row> Rows { get; set; }

        public BarState BarState { get
            {
                if (Rows == null) return BarState.Default;
                if (Rows.Count() == 0) return BarState.Unselected;
                if (Rows.Count() < EnvelopeRows.Count()) return BarState.PartiallySelected;
                return BarState.FullySelected;
            } }
        
        public BarChartDatum()
        {
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
