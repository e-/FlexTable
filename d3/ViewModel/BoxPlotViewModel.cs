using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d3.ViewModel
{
    class BoxPlotViewModel : Notifiable
    {
        private Double min;
        public Double Min { get { return min; } set { min = value; OnPropertyChanged("Min"); } }

        private Double max;
        public Double Max { get { return max; } set { max = value; OnPropertyChanged("Max"); } }

        private Double firstQuartile;
        public Double FirstQuartile { get { return firstQuartile; } set { firstQuartile = value; OnPropertyChanged("FirstQuartile"); } }

        private Double thirdQuartile;
        public Double ThirdQuartile { get { return thirdQuartile; } set { thirdQuartile = value; OnPropertyChanged("ThirdQuartile"); } }

        private Double median;
        public Double Median { get { return median; } set { median = value; OnPropertyChanged("Median"); } }

        private Double mean;
        public Double Mean { get { return mean; } set { mean = value; OnPropertyChanged("Mean"); } }

        private Double boxWidth;
        public Double BoxWidth { get { return boxWidth; } set { boxWidth = value > 0 ? value : 0; OnPropertyChanged("BoxWidth"); } }

        private Double boxX;
        public Double BoxX { get { return boxX; } set { boxX = value; OnPropertyChanged("BoxX"); } }

        private Double whisker1X;
        public Double Whisker1X { get { return whisker1X; } set { whisker1X = value; OnPropertyChanged("Whisker1X"); } }

        private Double whisker2X;
        public Double Whisker2X { get { return whisker2X; } set { whisker2X = value; OnPropertyChanged("Whisker2X"); } }

        private Scale.Linear scale = new Scale.Linear()
        {
            RangeStart = 50,
            RangeEnd = 600        
        };
        public Scale.Linear Scale { get { return scale; } set { scale = value; OnPropertyChanged("Scale"); } }

        public void Update()
        {
            scale.DomainStart = min;
            scale.DomainEnd = max;

            scale.Nice();
            Scale = scale;

            BoxWidth = scale.Map(thirdQuartile) - scale.Map(firstQuartile);
            BoxX = scale.Map(firstQuartile);
            Whisker1X = scale.Map(min);
            Whisker2X = scale.Map(max);
        }
    }
}
