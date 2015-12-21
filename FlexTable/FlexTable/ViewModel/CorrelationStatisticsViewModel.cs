using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.ViewModel;
using Collection = System.Collections.Generic.IEnumerable<double>;

namespace FlexTable.Model
{
    public class CorrelationStatisticsViewModel
    {
        private MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

        public String XVariableName { get; set; }
        public String YVariableName { get; set; }

        public Double PearsonCoefficient { get; set; }
        public String PearsonCoefficientString { get { return Format(PearsonCoefficient); } }

        public Double RSquared { get; set; }
        public String RSquaredString { get { return Format(RSquared); } }

        public Double Slope { get; set; }
        public String SlopeString { get {
            if (Slope == 1)
            {
                return String.Empty;
            }
            return Format(Slope) + " "; 
        } }

        public Double YIntercept { get; set; }
        public String YInterceptString { get {
            if (YIntercept == 0)
            {
                return String.Empty;
            }
            else if(YIntercept > 0){
                return string.Format("+ {0}",Format(YIntercept));
            }
            else{
                return string.Format("- {0}",Format(-YIntercept));
            }
        } }

        public String Format(Double value)
        {
            if (Math.Abs(value) < 10)
            {
                return value.ToString("#,0.###");
            }
            else if (Math.Abs(value) < 100)
            {
                return value.ToString("#,0.##");
            }
            return value.ToString("#,0");
        }

        public CorrelationStatisticsViewModel(MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }
    }

    public class CorrelationStatistics
    {
        public static Double Mean(Collection values)
        {
            return values.Sum() / values.Count();
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

        public static Double PearsonCoefficient(IEnumerable<Double> xVals, IEnumerable<Double> yVals)
        {
            Double[] xx = xVals.ToArray();
            Double[] yy = yVals.ToArray();

            Int32 count = xVals.Count();
            Double sigma1 = SampleStandardDeviation(xVals);
            Double sigma2 = SampleStandardDeviation(yVals);
            Double mu1 = Mean(xVals);
            Double mu2 = Mean(yVals);
            Double sum = 0;

            Int32 i;
            for(i=0;i<count;++i) {
                Double x = xx[i];
                Double y = yy[i];
                sum += (x - mu1) * (y - mu2);
            }

            return sum / count / sigma1 / sigma2;
        }

        public static void LinearRegression(IEnumerable<Double> xVals, IEnumerable<Double> yVals,
                                out double rsquared, out double yintercept,
                                out double slope)
        {
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = xVals.Count();
            Double[] xx = xVals.ToArray();
            Double[] yy = yVals.ToArray();

            for (int ctr = 0; ctr < count; ctr++)
            {
                double x = xx[ctr];
                double y = yy[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }

        public static CorrelationStatisticsViewModel Analyze(MainPageViewModel mainPageViewModel, String xName, String yName, IEnumerable<Double> values1, IEnumerable<Double> values2)
        {
            Double RSqaured, YIntercept, Slope;

            LinearRegression(values1, values2, out RSqaured, out YIntercept, out Slope);

            return new CorrelationStatisticsViewModel(mainPageViewModel)
            {
                XVariableName = xName,
                YVariableName = yName,
                YIntercept = YIntercept,
                RSquared = RSqaured,
                Slope = Slope,
                PearsonCoefficient = PearsonCoefficient(values1, values2)
            };
        }
    }
}
