using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace FlexTable
{
    public enum SelectionChangedType
    {
        Add,
        Remove,
        Clear,
        Replace
    }
    /*
    [Flags]
    public enum ReflectType2
    {
        Default = 0,
        TrackPreviousParagraph = 1 << 0,
        OnCreate = 1 << 1,
        OnSelectionChanged = 1 << 2
    }

    public enum ReflectReason2
    {
        SelectionChanged,
        FilterOut,
        ColumnViewModelChanged,
        PreviewRequested,
        Undo,
        ColumnViewModelSelected,
        ColumnViewModelUnselected,
        PageScrolled,
        RowSelected
    }
    */
    public enum ReflectReason
    {
        Preview,
        CancelPreview,

        Undo,

        ColumnSelected,
        ColumnUnselected,
        ColumnChanged,
        ColumnHidden,
        ColumnShown,
        ColumnSorted,
        AggregateFunctionChanged,

        RowSelected,
        RowUnselected,
        RowFiltered,
        RowSelectionChanged,

        PageScrolled
    };

    // Define an extension method in a non-nested static class.
    public static class Extensions
    {
        public static bool TrackPreviousParagraph(this ReflectReason reason)
        {
            switch (reason)
            {
                //case (ReflectReason.ColumnUnselected):
                case (ReflectReason.PageScrolled):
                case (ReflectReason.Preview):
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 해당 인터랙션이 보여지는 차트의 타입 및 차트 데이터를 바꾸는 경우는 true를 리턴
        /// 만약 셀렉션만 바꾸고 차트 데이터는 그대로라면 false 리턴
        /// </summary>
        public static bool ChartTypeChanged(this ReflectReason reason)
        {
            switch (reason)
            {
                // 차트 타입 및 데이터를 바꾸지 않는 인터랙션 리스트
                case (ReflectReason.Undo):
                case (ReflectReason.ColumnHidden):
                case (ReflectReason.ColumnShown):
                case (ReflectReason.RowSelected):
                case (ReflectReason.RowUnselected):
                case (ReflectReason.RowSelectionChanged):
                case (ReflectReason.PageScrolled):
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 해당 인터랙션이 차트의 타입을 바꾸지 않고 셀렉션만 바꾸는 경우 true 리턴
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static bool SelectionTypeChanged(this ReflectReason reason)
        {
            switch (reason)
            {
                case (ReflectReason.RowSelected):
                case (ReflectReason.RowUnselected):
                case (ReflectReason.RowSelectionChanged):
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 모든 페이지의 차트를 바꾸어야 하는 인터랙션들 
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static bool UpdateAllPageViews(this ReflectReason reason)
        {
            switch (reason)
            {
                case (ReflectReason.ColumnSorted):
                case (ReflectReason.AggregateFunctionChanged):
                case (ReflectReason.RowFiltered):
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 마지막 페이지의 차트만 바꾸면 되는 인터랙션들
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static bool UpdateLastPageView(this ReflectReason reason)
        {
            switch (reason)
            {
                case (ReflectReason.Preview):
                case (ReflectReason.CancelPreview):
                case (ReflectReason.ColumnSelected):
                case (ReflectReason.ColumnChanged):
                case (ReflectReason.RowSelected):
                case (ReflectReason.RowUnselected):
                case (ReflectReason.RowSelectionChanged):
                    return true;
            }
            return false;
        }
    }

    public class Const
    {
        public const Double PageViewToggleThreshold = 100;
        public const Double SelectionDismissThreshold = 200;
        public static Double RowHeight = 20;
        public static ResourceLoader Loader = new ResourceLoader();

        public static void Initialize()
        {
            RowHeight = (Double)App.Current.Resources["RowHeight"];
        }        
    }
}
