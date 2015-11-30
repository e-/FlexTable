using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.ViewModel;
using Windows.UI.Xaml;

namespace FlexTable.Model
{
    public class ViewStatus
    {
        private List<ColumnViewModel> selectedColumnViewModels = new List<ColumnViewModel>();
        public List<ColumnViewModel> SelectedColumnViewModels => selectedColumnViewModels;

        private Int32 totalCount => selectedColumnViewModels.Count;
        private Int32 numericalCount => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Count();
        private Int32 categoricalCount => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Count();

        public Boolean IsEmpty => totalCount == 0;

        public Boolean IsC => totalCount == 1 && categoricalCount == 1;
        public Boolean IsN => totalCount == 1 && numericalCount == 1;

        public Boolean IsCC => totalCount == 2 && categoricalCount == 2;
        public Boolean IsCN => totalCount == 2 && categoricalCount == 1 && numericalCount == 1;
        public Boolean IsNN => totalCount == 2 && numericalCount == 2;

        public Boolean IsCCC => totalCount == 3 && categoricalCount == 3;
        public Boolean IsCCN => totalCount == 3 && categoricalCount == 2 && numericalCount == 1;
        public Boolean IsCNN => totalCount == 3 && categoricalCount == 1 && numericalCount == 2;
        public Boolean IsNNN => totalCount == 3 && numericalCount == 3;

        public Boolean IsCnN0 => numericalCount == 0;
        public Boolean IsCnN1 => numericalCount == 1;
        public Boolean IsCnNn => numericalCount >= 1;

        public ColumnViewModel FirstColumn => selectedColumnViewModels.First();
        public ColumnViewModel FirstCategorical => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).First();
        public ColumnViewModel SecondCategorical => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ElementAt(1);
        public ColumnViewModel FirstNumerical => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).First();
        public ColumnViewModel SecondNumerical => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).ElementAt(1);

        public UIElement ActivatedChart { get; set; }
        public Boolean IsScatterplotVisible => ActivatedChart is Crayon.Chart.Scatterplot;
        public Boolean IsLineChartVisible => ActivatedChart is Crayon.Chart.LineChart;

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
