using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace FlexTable.Util
{
    class IndexToRowBackgroundConverter : ParityConverter<SolidColorBrush>
    {
        public IndexToRowBackgroundConverter() : base((SolidColorBrush)App.Current.Resources["RowGuidelineBrush0"], (SolidColorBrush)App.Current.Resources["RowGuidelineBrush1"]) {}
    }
}
