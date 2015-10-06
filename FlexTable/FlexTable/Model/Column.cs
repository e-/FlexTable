using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Column : NotifyModel
    {
        private String name;
        public String Name { get { return name; } set { name = value; } }
    }
}
