using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Util
{
    public class HistogramCalculator
    {
        public static List<Tuple<Double, Double, Int32>> Bin(Double min, Double max, Double step, IEnumerable<Double> values)
        {
            //List<Tuple<Double, Double, Int32>> result = new List<Tuple<double, double, int>>();
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

            foreach (Double value in values)
            {
                Int32 index = (Int32)Math.Floor((value - min) / step);

                if (index >= array.Length) index = array.Length - 1;
                array[index]++;
            }

            List<Tuple<Double, Double, Int32>> result = new List<Tuple<double, double, int>>();
            for (i = 0; i < array.Length; ++i)
            {
                result.Add(new Tuple<double, double, Int32>(min + step * i, min + step * (i + 1), array[i]));
            }

            return result;
        }
    }
}
