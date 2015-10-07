using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FlexTable.Model;
using d3;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class DistributionView : UserControl
    {
        public DistributionView()
        {
            this.InitializeComponent();
        }

        public void Update(DescriptiveStatisticsResult result, IEnumerable<Double> data)
        {
            HistogramElement.Width = (Double)App.Current.Resources["ParagraphWidth"] - 20;
            BoxPlotElement.Width = (Double)App.Current.Resources["ParagraphWidth"] - 90;
            BoxPlotElement.Min = result.Min;
            BoxPlotElement.Max = result.Max;
            BoxPlotElement.FirstQuartile = result.FirstQuartile;
            BoxPlotElement.Median = result.Median;
            BoxPlotElement.ThirdQuartile = result.ThirdQuartile;
            BoxPlotElement.Mean = result.Mean;


            BoxPlotElement.Update();

            d3.Scale.Linear linear = new d3.Scale.Linear()
            {
                DomainStart = result.Min,
                DomainEnd = result.Max,
                RangeStart = 0,
                RangeEnd = 1
            };

            linear.Nice();
            
            HistogramElement.Data = Util.HistogramCalculator.Bin(
                linear.DomainStart,
                linear.DomainEnd,
                linear.Step,
                data
                )
                .Select(d => new Tuple<Object, Double>(d.Item1, d.Item3));

            HistogramElement.Update();
        }
    }
}
