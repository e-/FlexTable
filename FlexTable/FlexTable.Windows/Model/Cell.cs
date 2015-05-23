using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class Cell
    {
        private String rawContent;
        public String RawContent { get { return rawContent; } set { rawContent = value; } }

        private Object content;
        public Object Content { get { return content; } set { content = value; } }
           
        public Column Column { get; set; }
    }
}
