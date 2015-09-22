using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Util
{
    public class Formatter
    {
        public static String FormatAuto4(Double value)
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

        public static String FormatAuto3(Double value)
        {
            if (Math.Abs(value) < 10)
            {
                return value.ToString("#,0.##");
            }
            else if (Math.Abs(value) < 100)
            {
                return value.ToString("#,0.#");
            }
            return value.ToString("#,0");
        }
    }
}
