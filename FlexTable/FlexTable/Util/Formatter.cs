using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Util
{
    public class Formatter
    {
        
        /// <summary>
        /// 숫자 앞에 4자리를 요약해서 보여줍니다. 100 이상의 수를 표시할 때에는 모두 표시합니다.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 숫자 앞에 3자리를 요약해서 보여줍니다. 100 이상의 수를 표시할 때에는 모두 표시합니다.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// SI Prefix를 이용하여 보여줍니다. 유효숫자는 최대 세 자리만 표시됩니다.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String SIPrefix(Double value)
        {
            /*if (value >= 100000000000)
                return (value / 1000000000).ToString("#,0") + " B";*/
            if (Math.Abs(value) >= 1000000000)
                return (value / 1000000000).ToString("#.##") + "B";
            if (Math.Abs(value) >= 100000000)
                return (value / 1000000000).ToString("0.##") + "B";
            if (Math.Abs(value) >= 1000000)
                return (value / 1000000).ToString("#.##") + "M";
            if (Math.Abs(value) >= 100000)
                return (value / 1000000).ToString("0.##") + "M";
            if (Math.Abs(value) >= 1000)
                return (value / 1000).ToString("#.##") + "K";

            return value.ToString("0.###"); //);//.ToString("#,0");
        }

        /// <summary>
        /// SI Prefix를 이용하여 보여줍니다. 유효숫자는 최대 세 자리만 표시됩니다.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String Auto(Double value)
        {
            if (Math.Abs(value) >= 1000000000)
                return (value / 1000000000).ToString("#.##") + "B";
            if (Math.Abs(value) >= 100000000)
                return (value / 1000000000).ToString("0.##") + "B";
            if (Math.Abs(value) >= 1000000)
                return (value / 1000000).ToString("#.##") + "M";
            if (Math.Abs(value) >= 100000)
                return (value / 1000000).ToString("0.##") + "M";
            if (Math.Abs(value) >= 1000)
                return (value / 1000).ToString("#.##") + "K";

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
