using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FlexTable.Util
{
    public sealed class BooleanToInvertedVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToInvertedVisibilityConverter() : base(Visibility.Collapsed, Visibility.Visible) { }
    }
}
