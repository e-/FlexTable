using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class ColumnViewModel : NotifyViewModel
    {     
        private ViewModel.MainPageViewModel mainPageViewModel;
        public ViewModel.MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        private Model.Column column;
        public Model.Column Column { 
            get { return column; } 
            set { 
                column = value;
                OnPropertyChanged("HeaderName");
            }
        }

        private Int32 index;
        public Int32 Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }

        private Int32 order;
        public Int32 Order { get { return order; } set { order = value; OnPropertyChanged("Order"); } }

        private Double width;
        public Double Width { get { return width; } set { width = value; OnPropertyChanged("Width"); } }

        private Double x;
        public Double X { get { return x; } set { x = value; OnPropertyChanged("X"); } }

        private Boolean isHidden;
        public Boolean IsHidden { get { return isHidden; } set { isHidden = value; OnPropertyChanged("IsHidden"); } }

        public Model.ColumnType Type { get; set; }

        private Boolean isGroupedBy = false;
        public Boolean IsGroupedBy { get { return isGroupedBy; } set { isGroupedBy = value; OnPropertyChanged("IsGroupedBy"); } }

        private Boolean isSelected = false;
        public Boolean IsSelected { get { return isSelected; } set { isSelected = value; OnPropertyChanged("IsSelected"); } }

        private Boolean isAscendingSorted = false;
        public Boolean IsAscendingSorted { get { return isAscendingSorted; } set { isAscendingSorted = value; OnPropertyChanged("IsAscendingSorted"); } }

        private Boolean isDecendingSorted = false;
        public Boolean IsDescendingSorted { get { return isDecendingSorted; } set { isDecendingSorted = value; OnPropertyChanged("IsDescendingSorted"); } }

        public Boolean IsXDirty { get; set; }

        private List<Model.Category> categories;
        public List<Model.Category> Categories { get { return categories; } set { categories = value; } }

        public String HeaderName { 
            get { return FormatHeaderName(column, Type, aggregativeFunction); } 
        }

        public String AggregatedName
        {
            get { return FormatAggregatedName(column, Type, aggregativeFunction); }
        }

        public String FormatHeaderName(Model.Column column, Model.ColumnType type, AggregativeFunctions.BaseAggregation aggregativeFunction)
        {
            if (type == Model.ColumnType.Categorical || type == Model.ColumnType.Datetime)
                return column.Name;
            
            if (mainPageViewModel.ExplorationViewModel.SelectedColumnViewModels.Count == 0)
                return column.Name;

            if (mainPageViewModel.ExplorationViewModel.SelectedColumnViewModels.Count == 1 &&
                mainPageViewModel.ExplorationViewModel.SelectedColumnViewModels[0] == this)
                return $"Bin({column.Name})";

            return FormatAggregatedName(column, type, aggregativeFunction);
        }

        public String FormatAggregatedName(Model.Column column, Model.ColumnType type, AggregativeFunctions.BaseAggregation aggregativeFunction)
        {
            if (aggregativeFunction is AggregativeFunctions.MinAggregation)
            {
                return $"Min({column.Name})";
            }
            if (aggregativeFunction is AggregativeFunctions.MaxAggregation)
            {
                return $"Max({column.Name})";
            }
            if (aggregativeFunction is AggregativeFunctions.AverageAggregation)
            {
                return $"Avg({column.Name})";
            }
            if (aggregativeFunction is AggregativeFunctions.SumAggregation)
            {
                return $"Sum({column.Name})";
            }

            return column.Name;
        }

        private AggregativeFunctions.BaseAggregation aggregativeFunction = new AggregativeFunctions.AverageAggregation();
        public AggregativeFunctions.BaseAggregation AggregativeFunction
        {
            get { return aggregativeFunction; }
            set { aggregativeFunction = value; OnPropertyChanged("HeaderName"); OnPropertyChanged("AggregativeFunction"); }
        }

        public ColumnViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void Hide()
        {
            isHidden = true;
            foreach (View.RowPresenter rowPresenter in mainPageViewModel.TableViewModel.AllRowPresenters)
            {
                rowPresenter.CellPresenters[index].Opacity = 0.15;
            }
            foreach (View.RowPresenter rowPresenter in mainPageViewModel.TableViewModel.TemporaryRowPresenters)
            {
                rowPresenter.CellPresenters[index].Opacity = 0.15;
            }
        }

        public void Show()
        {
            isHidden = false;
            foreach (View.RowPresenter rowPresenter in mainPageViewModel.TableViewModel.AllRowPresenters)
            {
                rowPresenter.CellPresenters[index].Opacity = 1;
            }
            foreach (View.RowPresenter rowPresenter in mainPageViewModel.TableViewModel.TemporaryRowPresenters)
            {
                rowPresenter.CellPresenters[index].Opacity = 1;
            }
        }

        public void UpdateHeaderName()
        {
            OnPropertyChanged("HeaderName");
        }
    }
}
