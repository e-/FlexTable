using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d3.Scale
{
    public class Ordinal : ScaleBase
    {
        private Double rangeStart = 0.0, rangeEnd = 1.0;
        override public Double RangeStart { get { return rangeStart; } set { rangeStart = value; } }
        override public Double RangeEnd { get { return rangeEnd; } set { rangeEnd = value; } }

        private List<Object> domain = new List<Object>();
        public List<Object> Domain { get { return domain; } }
        public override Int32 TickCount { get { return domain.Count; } set { } }

        override public Double Map(Object key)
        {
            if (domain.Contains(key))
            {
                Double interval = (rangeEnd - rangeStart) / (domain.Count);
                return domain.IndexOf(key) * interval + interval / 2 + rangeStart;
            }
            return rangeStart;
        }

        override public Double ClampedMap(Object key)
        {
            return Map(key);
        }

        override public Double Invert(Double y)
        {
            throw new Exception("Invert is not supported for ordinal scales");
        }

        override public List<Tick> GetTicks()
        {
            List<Tick> ticks = new List<Tick>();
            Int32 index = 0;
            Double interval = (rangeEnd - rangeStart) / (domain.Count);

            foreach (Object obj in domain)
            {
                ticks.Add(new Tick()
                {
                    Label = obj.ToString(),
                    DomainValue = obj,
                    RangeValue = interval * index + interval / 2 + rangeStart
                });
                ++index;
            }
            return ticks;
        }
    }
}
