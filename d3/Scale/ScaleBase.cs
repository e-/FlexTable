using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d3.Scale
{
    public class Tick
    {
        public Double RangeValue { get; set; }
        public String Label { get; set; }
        public Object DomainValue { get; set; }
    }

    /// <summary>
    /// describes a scale which accepts a real number and returns a real nubmer.
    /// </summary>
    public abstract class ScaleBase
    {
        public abstract Double RangeStart { get; set; }
        public abstract Double RangeEnd { get; set; }
        public abstract Int32 TickCount { get; set; }

        public abstract Double Invert(Double y);
        public abstract List<Tick> GetTicks();
        public abstract Double Map(Object x);
        public abstract Double ClampedMap(Object x);
    }
}
