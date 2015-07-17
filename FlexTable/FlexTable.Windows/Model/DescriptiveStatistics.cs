using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Collection = System.Collections.Generic.IEnumerable<double>;

namespace FlexTable.Model
{
    public class DescriptiveStatisticsResult
    {
        public Double Min { get; set; }
        public Double Max { get; set; }
        public Double Mean { get; set; }
        public Double MeanDeviation { get; set; }
        public Double FirstQuartile { get; set; }
        public Double Median { get; set; }
        public Double ThirdQuartile { get; set; }
        public Double Range { get; set; }
        public Double SampleVariance { get; set; }
        public Double SampleStandardDeviation { get; set; }
        public Double SampleSkewness { get; set; }
        public Double SampleKurtosis { get; set; }
    }

    public class DescriptiveStatistics
    {
        public static Double Min(Collection values)
        {
            return values.Min();
        }

        public static Double Max(Collection values)
        {
            return values.Max();
        }

        public static Double Mean(Collection values)
        {
            return values.Sum() / values.Count();
        }

        public static Double MeanDeviation(Collection values)
        {
            Double mean = Mean(values);
            Double sum = 0;

            foreach (Double value in values)
            {
                sum += Math.Abs(value - mean);
            }

            return sum / values.Count();
        }

        public static Double FirstQuartile(Collection values)
        {
            return values.OrderBy(v => v).ToArray()[values.Count() / 4];
        }

        public static Double Median(Collection values)
        {
            return values.OrderBy(v => v).ToArray()[values.Count() / 2];
        }

        public static Double ThirdQuartile(Collection values)
        {
            return values.OrderBy(v => v).ToArray()[values.Count() * 3 / 4];
        }

        public static Double Range(Collection values)
        {
            return Max(values) - Min(values);
        }

        public static Double SampleVariance(Collection values)
        {
            Double sum = 0;
            Double mean = Mean(values);

            foreach (Double value in values)
            {
                sum += (value - mean) * (value - mean);
            }
            return sum / values.Count();
        }

        public static Double SampleStandardDeviation(Collection values)
        {
            return Math.Sqrt(SampleVariance(values));
        }

        public static Double SampleSkewness(Collection values)
        {
            Double mean = Mean(values);
            Double sum1 = 0, sum2 = 0;
            Int32 n = values.Count();

            foreach (Double value in values)
            {
                sum1 += Math.Pow(value - mean, 3);
                sum2 += Math.Pow(value - mean, 2);
            }

            return (sum1 / n) / Math.Pow(sum2 / (n - 1), 1.5);
        }

        public static Double SampleKurtosis(Collection values)
        {
            Double mean = Mean(values);
            Double sum1 = 0, sum2 = 0;
            Int32 n = values.Count();

            foreach (Double value in values)
            {
                sum1 += Math.Pow(value - mean, 4);
                sum2 += Math.Pow(value - mean, 2);
            }

            return (sum1 / n) / Math.Pow(sum2 / n, 2) - 3;
        }
        
        public static DescriptiveStatisticsResult Analyze(IEnumerable<Double> values)
        {
            return new DescriptiveStatisticsResult()
            {
                Min = Min(values),
                Mean = Mean(values),
                Max = Max(values),
                MeanDeviation = MeanDeviation(values),
                FirstQuartile = FirstQuartile(values),
                Median = Median(values),
                ThirdQuartile = ThirdQuartile(values),
                Range = Range(values),
                SampleVariance = SampleVariance(values),
                SampleStandardDeviation = SampleStandardDeviation(values),
                SampleSkewness = SampleSkewness(values),
                SampleKurtosis = SampleKurtosis(values)
            };
        }
    }
}
