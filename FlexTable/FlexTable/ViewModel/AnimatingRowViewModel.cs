using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class AnimatingRowViewModel
    {
        public RowViewModel RowViewModel { get; set; }
        public Double X { get; set; }
        public ColumnViewModel ColumnViewModel { get; set; }
    }
}
