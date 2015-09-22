using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Util
{
    class BooleanToInvertedOpacityConverter : BooleanConverter<Double>
    {
        public BooleanToInvertedOpacityConverter() : base(0, 1){}
    }
}
