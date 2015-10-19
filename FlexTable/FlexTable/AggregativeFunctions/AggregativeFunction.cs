using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable
{
    public class AggregativeFunction
    {
        public static List<String> Names => new List<String>()
        {
            "Min",
            "Max",
            "Avg",
            "Sum"
        };

        public static BaseAggregation FromName(String name)
        {
            switch (name)
            {
                case "Max":
                    return new MaxAggregation();
                case "Min":
                    return new MinAggregation();
                case "Sum":
                    return new SumAggregation();
                case "Avg":
                    return new AverageAggregation();
            }
            throw new Exception("Unknown aggregation name");
        }

        public abstract class BaseAggregation
        {
            public abstract String Name { get; }

            public abstract Double Aggregate(IEnumerable<Double> values);
        }

        public class MaxAggregation : BaseAggregation
        {
            public override string Name
            {
                get { return "Max"; }
            }

            public override Double Aggregate(IEnumerable<Double> values)
            {
                return values.Count() == 0 ? 0 : values.Max();
            }
        }

        public class AverageAggregation : BaseAggregation
        {
            public override string Name
            {
                get { return "Avg"; }
            }

            public override Double Aggregate(IEnumerable<Double> values)
            {
                return values.Count() == 0 ? 0 : values.Average();
            }
        }

        public class MinAggregation : BaseAggregation
        {
            public override string Name
            {
                get { return "Min"; }
            }

            public override Double Aggregate(IEnumerable<Double> values)
            {
                return values.Count() == 0 ? 0 : values.Min();
            }
        }

        public class SumAggregation : BaseAggregation
        {
            public override string Name
            {
                get { return "Sum"; }
            }

            public override Double Aggregate(IEnumerable<Double> values)
            {
                return values.Count() == 0 ? 0 : values.Sum();
            }
        }

        public class None : BaseAggregation
        {
            public override string Name
            {
                get { return "None"; }
            }

            public override Double Aggregate(IEnumerable<Double> values)
            {
                throw new Exception("Aggregate function of None called!");
            }
        }
    }
}


