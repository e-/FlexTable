using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using FlexTable.ViewModel;

namespace FlexTable.Crayon
{
    public class Event
    {
        public delegate void SelectionChangedEventHandler(object sender, IEnumerable<Row> rows, SelectionChangedType selectionChangedType);
        public delegate void FilterOutEventHandler(object sender, IEnumerable<Category> categories);
        public delegate void RemoveColumnViewModelEventHandler(object sender, String name);
    }
}
