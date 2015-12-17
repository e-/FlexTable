using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;

namespace FlexTable.Crayon
{
    public class Event
    {
        public delegate void SelectionChangedEventHandler(object sender, IEnumerable<Row> rows, SelectionChangedType selectionChangedType, ReflectReason reason);
        public delegate void FilterOutEventHandler(object sender, String title, IEnumerable<Row> rows);
    }
}
