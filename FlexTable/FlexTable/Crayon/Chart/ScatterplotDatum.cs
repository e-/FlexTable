using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using FlexTable.ViewModel;

namespace FlexTable.Crayon.Chart
{
    public class ScatterplotDatum
    {
        public Object Key { get; set; }
        public Double Value1 { get; set; }
        public Double Value2 { get; set; }
        public Row Row { get; set; }
        public ColumnViewModel ColumnViewModel { get; set; }

        public ScatterplotDatum(Object key, Double value1, Double value2, Row row, ColumnViewModel columnViewModel)
        {
            this.Key = key;
            this.Value1 = value1;
            this.Value2 = value2;
            this.Row = row;
            this.ColumnViewModel = columnViewModel;
        }
    }
}
