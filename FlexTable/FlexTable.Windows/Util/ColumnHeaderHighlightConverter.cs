using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace FlexTable.Util
{
    class ColumnHeaderHighlightConverter : BooleanConverter<SolidColorBrush>
    {
        public ColumnHeaderHighlightConverter() : base(new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)), new SolidColorBrush(Colors.Transparent)) { }
    }
}
