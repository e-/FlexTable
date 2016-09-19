using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace FlexTable
{    
    public class Const
    {
        public const Double PageViewToggleThreshold = 100;
        public const Double SelectionDismissThreshold = 100;
        public static Double RowHeight = 20;
        public static Double ColumnHeaderHeight = 20;
        public static Double RowHeaderWidth = 100;
        public static Style CellStyle;
        public const Boolean PopupMenuEnabled = true;
        public static Double ColumnIndexerChatteringThreshold = 200;

        public static ResourceLoader Loader = new ResourceLoader();

        public static void Initialize()
        {
            RowHeight = (Double)App.Current.Resources["RowHeight"];
            ColumnHeaderHeight = (Double)App.Current.Resources["ColumnHeaderHeight"];
            RowHeaderWidth = (Double)App.Current.Resources["RowHeaderWidth"];
            CellStyle = (Style)App.Current.Resources["CellStyle"];
        }        
    }
}
