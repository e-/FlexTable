using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d3.Format
{
    public class SIPrefix
    {
        public static String Format(Double value)
        {
            /*if (value >= 100000000000)
                return (value / 1000000000).ToString("#,0") + " B";*/
            if (value >= 1000000000)
                return (value / 1000000000).ToString("#.##") + "B";
            if (value >= 100000000)
                return (value / 1000000000).ToString("0.##") + "B";
            if (value >= 1000000)
                return (value / 1000000).ToString("#.##") + "M";
            if (value >= 100000)
                return (value / 1000000).ToString("0.##") + "M";
            if (value >= 1000)
                return (value / 1000).ToString("#.##") + "K";

            return value.ToString("0.###"); //);//.ToString("#,0");
        }
    }
}
