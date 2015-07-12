using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d3.Scale
{
    public class Linear : ScaleBase
    {
        private Double rangeStart = 0.0, rangeEnd = 1.0;
        override public Double RangeStart { get { return rangeStart; } set { rangeStart = value; } }
        override public Double RangeEnd { get { return rangeEnd; } set { rangeEnd = value; } }

        private Double domainStart = 0.0, domainEnd = 1.0;
        public Double DomainStart { get { return domainStart; } set { domainStart = value; } }
        public Double DomainEnd { get { return domainEnd; } set { domainEnd = value; } }

        private Boolean clamp;
        public Boolean Clamp { get { return clamp; } set { clamp = value; } }

        override public Double Map(Object rawX)
        {
            Double x;
            try
            {
                x = Convert.ToDouble(rawX);
            }
            catch
            {
                return 0;
            }
            Double min = Math.Min(domainStart, domainEnd),
                   max = Math.Max(domainStart, domainEnd);

            if (clamp)
            {
                if (x < min) x = min;
                if (x > max) x = max;
            }

            return (x - domainStart) / (domainEnd - domainStart) * (rangeEnd - rangeStart) + rangeStart;
        }

        override public Double ClampedMap(Object rawX)
        {
            Double x;
            try
            {
                x = Convert.ToDouble(rawX);
            }
            catch
            {
                return 0;
            }

            Double min = Math.Min(domainStart, domainEnd),
                   max = Math.Max(domainStart, domainEnd);

            if (x < min) x = min;
            if (x > max) x = max;
            
            return (x - domainStart) / (domainEnd - domainStart) * (rangeEnd - rangeStart) + rangeStart;
        }

        override public Double Invert(Double x)
        {
            Double min = Math.Min(rangeStart, rangeEnd),
                   max = Math.Max(rangeStart, rangeEnd);

            if (clamp)
            {
                if (x < min) x = min;
                if (x > max) x = max;
            }

            return (x - rangeStart) / (rangeEnd - rangeStart) * (domainEnd - domainStart) + domainStart;
        }

        override public Int32 SuggestTickCount(Int32 suggested)
        {
            return suggested;
        }

        override public List<Tick> GetTicks(Int32 suggestedTickCount)
        {
            List<Tick> ticks = new List<Tick>();
            for (Int32 i = 0; i < suggestedTickCount; ++i)
            {
                Double domainValue = (Double)i / (suggestedTickCount - 1) * (domainEnd - domainStart) + domainStart;

                domainValue = Math.Round(domainValue);

                Double rangeValue = this.Map(domainValue);

                ticks.Add(new Tick()
                {
                    Label = domainValue.ToString(),
                    DomainValue = domainValue,
                    RangeValue = rangeValue
                });
            }

            return ticks;
        }
    }
}
