using FlexTable.Util;
using FlexTable.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace FlexTable.Model
{
    public class Category : Notifiable
    {
        private String value;
        public String Value { get { return value; } set { this.value = value; OnPropertyChanged(nameof(Value)); } }

        private Boolean isKept = true;
        public Boolean IsKept { get { return isKept; } set { this.isKept = value;  OnPropertyChanged(nameof(IsKept)); } }

        public Int32 Order { get; set; }
        public Color Color { get; set; }
        public Boolean IsVirtual { get; set; } = false;

        public ColumnViewModel ColumnViewModel;

        public override String ToString()
        {
            return value;
        }
    }
}
