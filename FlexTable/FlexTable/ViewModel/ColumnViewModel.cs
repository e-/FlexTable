using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3;
using FlexTable.Util;
using FlexTable.Model;

namespace FlexTable.ViewModel
{
    public class ColumnViewModel : Notifiable
    {     
        private MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        private Column column;
        public Column Column { 
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

        public Double StashedX { get; set; }
        private Double x;
        public Double X { get { return x; } set { x = value; OnPropertyChanged("X"); } }

        private Boolean isHidden;
        public Boolean IsHidden { get { return isHidden; } set { isHidden = value; OnPropertyChanged("IsHidden"); } }

        public ColumnType Type { get; set; }
        public CategoricalType CategoricalType { get; set; }
        public String Unit { get; set; }

        private Boolean isSelected = false;
        public Boolean IsSelected { get { return isSelected; } set { isSelected = value; OnPropertyChanged("IsSelected"); } }

        public Boolean IsAscendingSorted { get { return sortOption == SortOption.Ascending; } }
        public Boolean IsDescendingSorted { get { return sortOption == SortOption.Descending; } }

        private SortOption sortOption = SortOption.None;
        public SortOption SortOption { get { return sortOption; } set
            {
                sortOption = value;
                OnPropertyChanged(nameof(IsAscendingSorted));
                OnPropertyChanged(nameof(IsDescendingSorted));
            } }

        public Int32 SortPriority { get; set; } = 0;

        private List<Category> categories;
        public List<Category> Categories { get { return categories; } set { categories = value; } }

        public Boolean ContainString { get; set; }

        public String Name { get { return column.Name; } }
        public String HeaderName { 
            get { return FormatHeaderName(mainPageViewModel.ExplorationViewModel.ViewStatus); } 
        }

        public String UnitString
        {
            get
            {
                String unitString = "";
                if (Unit == null || Unit.Length == 0)
                {

                }
                else
                {
                    unitString = $" ({Unit})";
                }

                return unitString;
            }

        }
        public String HeaderNameWithUnit
        {
            get
            {
                return HeaderName + UnitString;
            }
        }

        public String AggregatedName
        {
            get { return FormatAggregatedName(column, Type, aggregativeFunction); }
        }

        public String FormatHeaderName(ViewStatus viewStatus)
        {
            if (Type == ColumnType.Categorical)
                return column.Name;
            
            if (viewStatus.IsEmpty)
                return column.Name;
            
            if (viewStatus.IsN && viewStatus.FirstColumn == this)
            {
                return String.Format(
                    Const.Loader.GetString("BinnedColumnName").Replace("|", ""),
                    column.Name
                );
            }

            if (viewStatus.IsNN)
            {
                if(viewStatus.IsScatterplotVisible)
                    return column.Name;
            }

            if (viewStatus.IsCNN)
            {
                if (viewStatus.IsScatterplotVisible)
                    return column.Name;
            }

            return FormatAggregatedName(column, Type, aggregativeFunction);
        }

        public String FormatPivotTableHeaderName(ViewStatus viewStatus)
        {
            if (Type == ColumnType.Categorical)
                return column.Name;
            
            if (viewStatus.IsN && viewStatus.FirstColumn == this)
            {
                return String.Format(
                    Const.Loader.GetString("BinnedColumnName").Replace("|", ""),
                    column.Name
                );
            }

            return FormatAggregatedName(column, Type, aggregativeFunction);
        }

        String aggregatedColumnNameFormat = Const.Loader.GetString("AggregatedColumnName").Replace("|", "");
        public String FormatAggregatedName(Column column, ColumnType type, AggregativeFunction.BaseAggregation aggregativeFunction)
        {
            if (aggregativeFunction is AggregativeFunction.MinAggregation ||
                aggregativeFunction is AggregativeFunction.MaxAggregation ||
                aggregativeFunction is AggregativeFunction.MeanAggregation ||
                aggregativeFunction is AggregativeFunction.SumAggregation)
            {
                return String.Format(aggregatedColumnNameFormat, aggregativeFunction.Name, column.Name);
            }

            return column.Name;
        }

        private AggregativeFunction.BaseAggregation aggregativeFunction = new AggregativeFunction.SumAggregation();
        public AggregativeFunction.BaseAggregation AggregativeFunction
        {
            get { return aggregativeFunction; }
            set { aggregativeFunction = value; OnPropertyChanged("HeaderName"); OnPropertyChanged("AggregativeFunction"); }
        }

        public ColumnViewModel(MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void UpdateHeaderName()
        {
            OnPropertyChanged("HeaderName");
        }

        public void Stash()
        {
            StashedX = X;
        }

        public Int32 SortDirection => this.SortOption == SortOption.Descending ? -1 : 1;
    }
}
