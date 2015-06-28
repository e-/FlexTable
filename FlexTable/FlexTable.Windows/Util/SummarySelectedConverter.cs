using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace FlexTable.Util
{
    class SummarySelectedConverter : BooleanConverter<SolidColorBrush>
    {
        public SummarySelectedConverter() : base(new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)), new SolidColorBrush(Colors.Transparent)) { }
    }
}
