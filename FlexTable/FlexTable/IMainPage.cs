using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FlexTable
{
    public interface IMainPage
    {
        View.TableView TableView { get; }
        View.ExplorationView ExplorationView { get; }
        TextBlock DummyTextBlock { get; }
    }
}
