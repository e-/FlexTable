using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Cell
    {
        private Object content;
        
        public Object Content { get { return content; } set { content = value; } }

        public String ContentAsString { get { return (String)content; } }
        public Double ContentAsDouble { get { return (Double)content; } }

        public Column Column { get; set; }
    }
}
