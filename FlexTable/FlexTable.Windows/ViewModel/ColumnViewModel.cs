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
