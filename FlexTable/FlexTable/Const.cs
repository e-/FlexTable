using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable
{
    [Flags]
    public enum ReflectType
    {
        Default = 0,
        TrackPreviousParagraph = 1 << 0,
        OnCreate = 1 << 1,
        OnSelectionChanged = 1 << 2
    }

    public enum SelectionChangedType
    {
        Add,
        Remove,
        Clear,
        Replace
    }

    public enum ReflectReason
    {
        SelectionChanged,
        FilterOut,
        ColumnViewModelChanged,
        PreviewRequested,
        Undo,
        ColumnViewModelSelected,
        ColumnViewModelUnselected,
        PageScrolled
    }

    public class Const
    {
        public static Double PageViewToggleThreshold = 200;
        public static Double RowHeight = 20;

        public static void Initialize()
        {
            RowHeight = (Double)App.Current.Resources["RowHeight"];
        }
        
    }
}
