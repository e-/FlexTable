using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Column
    {
        private String name;
        public String Name { get { return name; } set { name = value; } }

        private Int32 index;
        public Int32 Index { get { return index; } set { index = value; } }

        private Double width;
        public Double Width { get { return width; } set { width = value; } }

    }
}
