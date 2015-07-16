using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Cell : NotifyModel
    {
        private String rawContent;
        public String RawContent { get { return rawContent; } set { rawContent = value; OnPropertyChanged("RawContent"); } }

        private Object content;
        public Object Content { get { return content; } set { content = value; OnPropertyChanged("Content"); } }
           
        public ViewModel.ColumnViewModel ColumnViewModel { get; set; }
    }
}
