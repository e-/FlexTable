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
        public static String FormatHeaderName(Model.Column column, Model.ColumnType type, AggregativeFunctions.BaseAggregation aggregativeFunction)
        {
            if (type == Model.ColumnType.Categorical)
                return column.Name;

            if (aggregativeFunction is AggregativeFunctions.MinAggregation)
            {
                return String.Format("Min({0})", column.Name);
            }
            if (aggregativeFunction is AggregativeFunctions.MaxAggregation)
            {
                return String.Format("Max({0})", column.Name);
            }
            if (aggregativeFunction is AggregativeFunctions.AverageAggregation)
            {
                return String.Format("Avg({0})", column.Name);
            }
            if (aggregativeFunction is AggregativeFunctions.SumAggregation)
            {
                return String.Format("Sum({0})", column.Name);
            }

            return column.Name;
        }

        private ViewModel.MainPageViewModel mainPageViewModel;
        public ViewModel.MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        private Model.Column column;
        public Model.Column Column { 
            get { return column; } 
            set { 
                column = value;
                HeaderName = FormatHeaderName(column, Type, aggregativeFunction);
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
        public String TypeString { get { return Type.ToString(); } }

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

        private String headerName;
        public String HeaderName { get { return headerName; } set { headerName = value; OnPropertyChanged("HeaderName"); } }

        private AggregativeFunctions.BaseAggregation aggregativeFunction = new AggregativeFunctions.AverageAggregation();
        public AggregativeFunctions.BaseAggregation AggregativeFunction
        {
            get { return aggregativeFunction; }
            set { aggregativeFunction = value; HeaderName = FormatHeaderName(column, Type, aggregativeFunction); OnPropertyChanged("AggregativeFunction"); }
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
    }
}
