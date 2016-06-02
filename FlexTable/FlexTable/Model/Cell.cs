using FlexTable.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Cell : Notifiable
    {
        private String rawContent;
        public String RawContent { get { return rawContent; } set { rawContent = value; OnPropertyChanged(nameof(RawContent)); } }

        private Object content;
        public Object Content { get { return content; } set { content = value; OnPropertyChanged(nameof(Content)); } }

        public ViewModel.ColumnViewModel ColumnViewModel { get; set; }
    }
}
