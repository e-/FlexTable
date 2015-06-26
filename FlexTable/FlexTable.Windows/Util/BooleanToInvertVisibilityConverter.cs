using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FlexTable.Util
{
    public sealed class BooleanToInvertVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToInvertVisibilityConverter() : base(Visibility.Collapsed, Visibility.Visible) { }
    }
}
