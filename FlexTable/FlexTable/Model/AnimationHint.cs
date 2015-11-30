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
        public Dictionary<ColumnViewModel, Double> ColumnViewModelPosition { get; set; } = new Dictionary<ColumnViewModel, double>();

        public static AnimationHint Create(SheetViewModel sheetViewModel)
        {
            AnimationHint hint = new AnimationHint();

            foreach(ColumnViewModel cvm in sheetViewModel.ColumnViewModels)
            {
                hint.ColumnViewModelPosition[cvm] = cvm.X;
            }

            return hint;
        }
    }
}
