using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d3.Format
{
    public class IntegerBalanced
    {
        // 정수의 경우 소수점 생략하고 다 표시 아니면 소수점 세자리까지

        public static String Format(Double value)
        {
            if (value >= 1000 || value <= -1000)
                return value.ToString("#,#");

            if (-10 <= value && value <= 10)
                return value.ToString("0.###"); //);//.ToString("#,0");
            else
                return value.ToString("0.##");
        }
    }
}
