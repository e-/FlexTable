using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class RowHeaderViewModel : NotifyViewModel
    {
        ViewModel.MainPageViewModel mainPageViewModel;

        private ObservableCollection<Model.RowHeader> rowHeaderItems = new ObservableCollection<Model.RowHeader>();
        public ObservableCollection<Model.RowHeader> RowHeaderItems { get { return rowHeaderItems; } }

        public RowHeaderViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void SetRowNumber(Int32 n)
        {
            rowHeaderItems.Clear();
            for (Int32 i = 1; i <= n; ++i)
            {
                rowHeaderItems.Add(new Model.RowHeader() { Index = i });
            }
        }
    }
}
