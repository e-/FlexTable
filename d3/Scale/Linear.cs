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

        private Int32 tickCount = 10;
        public override Int32 TickCount { get { return tickCount; } set { tickCount = value; } }

        public Double Step { get; set; }

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

            Double value = (x - domainStart) / (domainEnd - domainStart) * (rangeEnd - rangeStart) + rangeStart;

            if (double.IsNaN(value)) return 0;
            return value;
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
            
            Double value = (x - domainStart) / (domainEnd - domainStart) * (rangeEnd - rangeStart) + rangeStart;

            if (double.IsNaN(value)) return 0;
            return value;
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


        override public List<Tick> GetTicks()
        {
            List<Tick> ticks = new List<Tick>();
            for (Int32 i = 0; i < TickCount; ++i)
            {
                Double domainValue = domainStart + (i+1) * Step;
                Double rangeValue = this.Map(domainValue);

                if (domainValue >= domainEnd + Step)
                    break;

                ticks.Add(new Tick()
                {
                    Label = domainValue.ToString(),
                    DomainValue = domainValue,
                    RangeValue = rangeValue
                });
            }

            return ticks;
        }

        public void Nice()
        {
            Nice(TickCount);
        }

        public void Nice(Int32 m)
        {
            Double span = DomainEnd - domainStart,
                   step = Math.Pow(10, Math.Floor(Math.Log10(span / m))),
                   err = m / span * step;

            if (err < .15) step *= 10;
            else if (err < .35) step *= 5;
            else if (err < .75) step *= 2;

            Step = step;

            DomainStart = Math.Floor(domainStart / step) * step;
            DomainEnd = Math.Ceiling(domainEnd / step) * step;
        }
    }
}
