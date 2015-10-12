using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.ViewModel;

namespace FlexTable.Model
{
    public class ViewStatus
    {
        private List<ColumnViewModel> selectedColumnViewModels = new List<ColumnViewModel>();
        public List<ColumnViewModel> SelectedColumnViewModels => selectedColumnViewModels;


        public ViewStatus Clone()
        {
            ViewStatus cloned = new ViewStatus();
            foreach(ColumnViewModel cvm in selectedColumnViewModels)
            {
                cloned.SelectedColumnViewModels.Add(cvm);
            }

            return cloned;
        }        
    }
}
