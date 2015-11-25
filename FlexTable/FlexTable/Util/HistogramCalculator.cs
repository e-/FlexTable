using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using FlexTable.ViewModel;

namespace FlexTable.Util
{
    public class Bin
    {
        public ColumnViewModel ColumnViewModel { get; set; }
        public Double Min { get; set; }
        public Double Max { get; set; }
        public IEnumerable<Row> Rows { get; set; }
    }

    public class HistogramCalculator
    {
        public static IEnumerable<Bin> Bin(Double min, Double max, Double step, IEnumerable<Row> rows, ColumnViewModel numerical)
        {
            Int32[] array;
            if (min == max)
            {
                array = new Int32[1];
                step = 10;
            }
            else {
                array = new Int32[(Int32)Math.Ceiling((max - min) / step)];
            }

            Int32 i = 0;
            for (i = 0; i < array.Length; ++i) array[i] = 0;

            return rows
                .GroupBy(row =>
                {
                    Double value = (Double)row.Cells[numerical.Index].Content;
                    Int32 index = (Int32)Math.Floor((value - min) / step);

                    if (index >= array.Length) index = array.Length - 1;

                    return index;
                })
                .Select(group => new Bin()
                {
                    ColumnViewModel = numerical,
                    Min = min + step * group.Key,
                    Max = min + step * (1 + group.Key),
                    Rows = group
                });
        }
    }
}
