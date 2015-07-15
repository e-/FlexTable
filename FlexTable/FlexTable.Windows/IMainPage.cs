using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable
{
    public interface IMainPage
    {
        void ScrollToColumn(Model.Column column);
        void UpdateColumnHeaders();
        void AddRow(View.RowPresenter rowPresenter);
        void RemoveRow(View.RowPresenter rowPresenter);
    }
}
