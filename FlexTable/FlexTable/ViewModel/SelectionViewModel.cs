using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.ViewModel
{
    public class SelectionViewModel
    {
        private MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

        IMainPage view;
        
        public SelectionViewModel(MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.view = view;
        }

        public void Initialize()
        {

        }
    }
}
