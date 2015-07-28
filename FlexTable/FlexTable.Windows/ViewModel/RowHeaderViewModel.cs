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
        private ObservableCollection<Model.RowHeader> rowHeaderItems = new ObservableCollection<Model.RowHeader>();
        public ObservableCollection<Model.RowHeader> RowHeaderItems { get { return rowHeaderItems; } }

        ViewModel.MainPageViewModel mainPageViewModel;

        public RowHeaderViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void SetMaximumRowNumber(Int32 n)
        {
            Int32 maxN = n;

            if (maxN < 50) maxN = 50;

            for (Int32 i = 0; i < maxN; ++i)
            {
                rowHeaderItems.Add(new Model.RowHeader()
                {
                    Index = i + 1,
                    Opacity = i < n ? 1 : 0
                });
            }
        }

        public void SetRowNumber(Int32 n)
        {
            foreach (Model.RowHeader rh in rowHeaderItems)
            {
                rh.Opacity = 0;
            }

            for (Int32 i = 0; i < n; ++i)
            {
                rowHeaderItems[i].Opacity = 1;
            }
        }
    }
}
