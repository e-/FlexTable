using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using FlexTable.ViewModel;

namespace FlexTable.Crayon.Chart
{
    public enum ScatterplotState
    {
        Default,
        Unselected,
        Selected
    }

    public class ScatterplotDatum
    {
        public Object Key { get; set; }
        public Double Value1 { get; set; }
        public Double Value2 { get; set; }
        public Row Row { get; set; }
        public ScatterplotState State { get; set; }
        public ColumnViewModel ColumnViewModel { get; set; }
    }
}
