using FlexTable.Model;
using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class FilterViewModel : Notifiable
    {
        private MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

        IMainPage view;

        private ObservableCollection<ColumnViewModel> filterColumnViewModels = new ObservableCollection<ColumnViewModel>();
        public ObservableCollection<ColumnViewModel> FilterColumnViewModels { get { return filterColumnViewModels; } set { filterColumnViewModels = value; OnPropertyChanged(nameof(FilterColumnViewModels)); } }

        private Int32 totalRowCount = 0;
        public Int32 TotalRowCount { get { return totalRowCount; } set { totalRowCount = value; OnPropertyChanged(nameof(TotalRowCount)); } }

        private Int32 filteredRowCount = 0;
        public Int32 FilteredRowCount { get { return filteredRowCount; } set { filteredRowCount = value; OnPropertyChanged(nameof(FilteredRowCount)); } }

        public FilterViewModel(MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.view = view;
        }

        public void Initialize()
        {
            FilterColumnViewModels = new ObservableCollection<ColumnViewModel>(mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical));

            TotalRowCount = FilteredRowCount = mainPageViewModel.SheetViewModel.Sheet.Rows.Count;
        }
    }
}
