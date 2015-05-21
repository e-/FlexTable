using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Util
{
    public sealed class BooleanToOpacityConverter : BooleanConverter<Double>
    {
        public BooleanToOpacityConverter() : base(1.0, 0) { }
    }
}
