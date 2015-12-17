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
using FlexTable.Crayon.Chart;
using FlexTable.ViewModel;
using FlexTable.Util;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class DistributionView : UserControl
    {
        public PageViewModel ViewModel { get { return (PageViewModel)DataContext; } }
        public BarChart Histogram { get { return HistogramElement; } }

        public DistributionView()
        {
            this.InitializeComponent();
        }

        public void Feed(IEnumerable<Row> rows, ColumnViewModel numerical)
        {
            DescriptiveStatisticsResult result = DescriptiveStatistics.Analyze(
                       rows.Select(r => (Double)r.Cells[numerical.Index].Content)
                       );

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

            List<Bin> bins = HistogramCalculator.Bin(
                linear.DomainStart,
                linear.DomainEnd,
                linear.Step,
                rows,
                numerical
                ).ToList();

            if (numerical.SortOption == SortOption.Descending) { bins = bins.OrderByDescending(b => b.Min).ToList(); }
            else { bins = bins.OrderBy(b => b.Min).ToList(); }

            HistogramElement.Data =
                bins
                .Select(d => new BarChartDatum()
                {
                    Key = $"~{Formatter.Auto(d.Max)}",
                    ColumnViewModel = numerical,
                    Value = d.Rows.Count(),
                    EnvelopeValue = d.Rows.Count(),
                    Rows = d.Rows,
                    EnvelopeRows = d.Rows
                }).ToList();
        }
    }
}
