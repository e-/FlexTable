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

        private Model.Column column;
        public Model.Column Column { get { return column; } set { column = value; OnPropertyChanged("ColumnViewModel"); } }

        private Int32 index;
        public Int32 Index { get { return index; } set { index = value; OnPropertyChanged("Index"); } }

        private Int32 order;
        public Int32 Order { get { return order; } set { order = value; OnPropertyChanged("Order"); } }

        private Double width;
        public Double Width { get { return width; } set { width = value; OnPropertyChanged("Width"); } }

        private Double x;
        public Double X { get { return x; } set { x = value; OnPropertyChanged("X"); } }

        private Boolean enabled = true;
        public Boolean Enabled { get { return enabled; } set { enabled = value; OnPropertyChanged("Enabled"); } }      

        public Model.ColumnType Type { get; set; }
        public String TypeString { get { return Type.ToString(); } }

        /*private List<Model.Bin> bins;
        public List<Model.Bin> Bins { get { return bins; } set { bins = value; } }*/

        private Boolean isGroupedBy;
        public Boolean IsGroupedBy { get { return isGroupedBy; } set { isGroupedBy = value; OnPropertyChanged("IsGroupedBy"); } }

        private Boolean isDrawnOnChart;
        public Boolean IsDrawnOnChart { get { return isDrawnOnChart; } set { isDrawnOnChart = value; OnPropertyChanged("IsDrawnOnChart"); } }

        private List<Model.Category> categories;
        public List<Model.Category> Categories { get { return categories; } set { categories = value; } }

        private Model.AggregationType aggregationType = Model.AggregationType.Average;
        public Model.AggregationType AggregationType { get { return aggregationType; } set { aggregationType = value; } }

        public ColumnViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }
    }
}
