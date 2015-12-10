using FlexTable.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class AnimationHint
    {
        public enum AnimationType
        {
            SelectRows,
            UnselectRows,
            DependOnViewStatus
        };

        public Dictionary<ColumnViewModel, Double> ColumnViewModelPosition { get; set; } = new Dictionary<ColumnViewModel, double>();
        public TableViewModel.TableViewState TableViewState { get; set; }
        public AnimationType Type { get; set; }

        public static AnimationHint Create(SheetViewModel sheetViewModel, TableViewModel tableViewModel)
        {
            AnimationHint hint = new AnimationHint()
            {
                TableViewState = tableViewModel.State
            };

            foreach(ColumnViewModel cvm in sheetViewModel.ColumnViewModels)
            {
                hint.ColumnViewModelPosition[cvm] = cvm.X;
            }
            
            return hint;
        }
    }
}
