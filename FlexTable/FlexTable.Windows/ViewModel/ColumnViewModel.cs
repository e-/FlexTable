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
        public Model.Column Column { get { return column; } set { column = value; OnPropertyChanged("Column"); } }

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

        private Boolean highlighted = false;
        public Boolean Highlighted { get { return highlighted; } set { highlighted = value; OnPropertyChanged("Highlighted"); } }

        public Model.ColumnType Type { get; set; }
        public String TypeString { get { return Type.ToString(); } }

        private List<Model.Bin> bins;
        public List<Model.Bin> Bins { get { return bins; } set { bins = value; } }

        private Double minValue;
        public Double MinValue { get { return minValue; } set { minValue = value; OnPropertyChanged("MinValue"); } }
        public String MinValueString { get { return String.Format("{0:#,0.#}", minValue); } }

        private Double maxValue;
        public Double MaxValue { get { return maxValue; } set { maxValue = value; OnPropertyChanged("MaxValue"); } }
        public String MaxValueString { get { return String.Format("{0:#,0.#}", maxValue); } }

        private Double meanValue;
        public Double MeanValue { get { return meanValue; } set { meanValue = value; OnPropertyChanged("MeanValue"); } }
        public String MeanValueString { get { return String.Format("{0:#,0.#}", meanValue); } }

        private Double medianValue;
        public Double MedianValue { get { return medianValue; } set { medianValue = value; OnPropertyChanged("MedianValue"); } }
        public String MedianValueString { get { return String.Format("{0:#,0.#}", medianValue); } }

        private Boolean isGroupedBy;
        public Boolean IsGroupedBy { get { return isGroupedBy; } set { isGroupedBy = value; OnPropertyChanged("IsGroupedBy"); } }

        private Boolean isDrawnOnChart;
        public Boolean IsDrawnOnChart { get { return isDrawnOnChart; } set { isDrawnOnChart = value; OnPropertyChanged("IsDrawnOnChart"); } }


        public Model.AggregationType AggregationType { get; set; }

        public ColumnViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }


        /*public void Highlight()
        {
            mainPageViewModel.HighlightColumn(column);
        }

        public void Unhighlight()
        {
            mainPageViewModel.UnhighlightColumn(column);
        }

        public void MarkEnabled()
        {
            mainPageViewModel.MarkColumnEnabled(column);
        }

        public void MarkDisabled()
        {
            mainPageViewModel.MarkColumnDisabled(column);
        }*/

        /*public void SortAscending()
        {
            mainPageViewModel.Sort(column, false);
        }

        public void SortDescending()
        {
            mainPageViewModel.Sort(column, true);
        }*/
    }
}
