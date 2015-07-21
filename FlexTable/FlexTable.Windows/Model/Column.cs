using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Column : NotifyModel
    {
        private String name;
        public String Name { get { return name; } set { name = value; } }
       
        public static ColumnType GuessColumnType(IEnumerable<String> cellValues)
        {
            Boolean allDouble = true;
            Double result;
            List<String> differentValues = new List<String>();

            foreach (String value in cellValues)
            {
                if (differentValues.IndexOf(value) < 0)
                {
                    differentValues.Add(value);
                }

                if (!Double.TryParse(value, out result))
                {
                    allDouble = false;
                    break;
                }
            }

            // 문자가 하나라도 있으면 무조건 범주형
            if (!allDouble) return ColumnType.Categorical;

            if (differentValues.Count < 10) return ColumnType.Categorical;
            return ColumnType.Numerical;
        }

        public static List<Bin> GetFrequencyBins(IEnumerable<ViewModel.RowViewModel> rows, Int32 cellIndex)
        {
            Dictionary<String, List<ViewModel.RowViewModel>> dictionary = new Dictionary<String, List<ViewModel.RowViewModel>>();

            foreach (ViewModel.RowViewModel row in rows)
            {
                String value = row.Cells[cellIndex].RawContent;
                if (!dictionary.ContainsKey(value))
                {
                    dictionary[value] = new List<ViewModel.RowViewModel>();
                }
                dictionary[value].Add(row);
            }

            var sorted = dictionary.Keys.ToList();
            sorted.Sort();

            List<Bin> bins = new List<Bin>();
            Int32 index = 0;
            foreach (String key in sorted)
            {
                bins.Add(new Bin() { Name = key, Count = dictionary[key].Count, Index = index++, RowViewModels = dictionary[key]});
            }

            return bins;
        }

        public static List<Bin> GetHistogramBins(IEnumerable<Double> cellValues)
        {
            Double min = cellValues.Min(),
                   max = cellValues.Max();

            if (min == max) max = min + 1;

            Int32 binCount = 8;
            Double interval = (max - min) / binCount;
            
            List<Bin> bins = new List<Bin>();
            for (Int32 i = 0; i < binCount; ++i)
            {
                Double start = Math.Round(min + interval * i),
                       end = Math.Round(min + interval * (i+1))
                       ;

                bins.Add(new Bin(){
                    Name = String.Format("{0}-{1}", start, end),
                    Count = 0,
                    Index = i
                });
            }

            foreach (Double value in cellValues)
            {
                Int32 index = (Int32)Math.Floor((value - min) * binCount / (max - min));
                if (index >= binCount) index = binCount - 1;
                bins[index].Count++;
            }

            return bins;
        }
    }
}
