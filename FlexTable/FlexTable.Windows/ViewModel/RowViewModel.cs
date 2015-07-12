using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class RowViewModel : NotifyViewModel
    {
        ViewModel.MainPageViewModel mainPageViewModel;

        private Model.Row row;
        public Model.Row Row { get { return row; } set { row = value; } }

        public RowViewModel(MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }
    }
}
