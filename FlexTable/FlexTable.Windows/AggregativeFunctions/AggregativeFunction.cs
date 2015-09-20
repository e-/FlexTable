using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.AggregativeFunctions
{
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
            return values.Max();
        }
    }

    public class AverageAggregation : BaseAggregation
    {
        public override string Name
        {
            get { return "Average"; }
        }

        public override Double Aggregate(IEnumerable<Double> values)
        {
            return values.Average();
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
            return values.Min();
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
            return values.Sum();
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


