using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.View;

namespace FlexTable.ViewModel
{
    public class CustomHistogramViewModel : NotifyViewModel
    {
        MainPageViewModel mainPageViewModel;
        CustomHistogramView customHistogramView;

        public CustomHistogramViewModel(MainPageViewModel mainPageViewModel, CustomHistogramView customHistogramView)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.customHistogramView = customHistogramView;
        }

        public void Show(IEnumerable<Double> data)
        {
            customHistogramView.Update(data);
        }

        
    }
}
