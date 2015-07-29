using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Category : NotifyModel
    {
        private String value;
        public String Value { get { return value; } set { this.value = value; OnPropertyChanged("Value"); } }

        public Int32 Order { get; set; }

        public override String ToString()
        {
            return value;
        }
    }
}
