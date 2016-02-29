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
            Const.Loader.GetString("Min"),
            Const.Loader.GetString("Mean"),
            Const.Loader.GetString("Max"),
            Const.Loader.GetString("Sum")
        };

        public static BaseAggregation FromName(String name)
        {
            if (name == Const.Loader.GetString("Max"))
            {
                return new MaxAggregation();
            }
            else if (name == Const.Loader.GetString("Min"))
            {
                return new MinAggregation();
            }
            else if (name == Const.Loader.GetString("Sum"))
            {
                return new SumAggregation();
            }
            else if (name == Const.Loader.GetString("Mean"))
            {
                return new MeanAggregation();
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
                get { return Const.Loader.GetString("Max"); }
            }

            public override Double Aggregate(IEnumerable<Double> values)
            {
                return values.Count() == 0 ? 0 : values.Max();
            }
        }

        public class MeanAggregation : BaseAggregation
        {
            public override string Name
            {
                get { return Const.Loader.GetString("Mean"); }
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
                get { return Const.Loader.GetString("Min"); }
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
                get { return Const.Loader.GetString("Sum"); }
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
                get { return Const.Loader.GetString("None"); }
            }

            public override Double Aggregate(IEnumerable<Double> values)
            {
                throw new Exception("Aggregate function of None called!");
            }
        }
    }
}


