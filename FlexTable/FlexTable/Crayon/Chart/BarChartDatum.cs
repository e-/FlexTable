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
        public Int32 Order { get; set; }

        public GroupedBarChartDatum Parent { get; set; }

        private IEnumerable<Row> envelopeRows;
        public IEnumerable<Row> EnvelopeRows
        {
            get
            {
                return envelopeRows;
            }
            set
            {
                envelopeRows = value;
                UpdateBarState();
            }
        }

        private IEnumerable<Row> rows;
        public IEnumerable<Row> Rows
        {
            get
            {
                return rows;
            }
            set
            {
                rows = value;
                UpdateBarState();
            }
        }

        void UpdateBarState()
        {
            if (rows == null) barState = BarState.Default;
            else if (rows.Count() == 0) barState = BarState.Unselected;
            else if (rows.Count() < envelopeRows.Count()) barState = BarState.PartiallySelected;
            else barState = BarState.FullySelected;
        }

        private BarState barState;
        public BarState BarState
        {
            get
            {
                return barState;
            }
        }

        public Boolean IsUnselected { get { return barState == BarState.Unselected; } }

        public BarChartDatum()
        {
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
