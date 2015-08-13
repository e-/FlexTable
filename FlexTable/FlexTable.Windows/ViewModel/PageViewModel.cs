using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using FlexTable.Model;
using Windows.UI.Xaml.Media.Animation;

namespace FlexTable.ViewModel
{
    public class PageViewModel : NotifyViewModel
    {
        private ColumnViewModel columnViewModel;
        public ColumnViewModel ColumnViewModel { get { return columnViewModel; } set { columnViewModel = value; OnPropertyChanged("ColumnViewModel"); } }

        private DescriptiveStatisticsResult boxPlotViewModel;
        public DescriptiveStatisticsResult BoxPlotViewModel { get { return boxPlotViewModel; } set { boxPlotViewModel = value; OnPropertyChanged("BoxPlotViewModel"); } }

        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        PivotTableViewModel pivotTableViewModel;
        public PivotTableViewModel PivotTableViewModel { get { return pivotTableViewModel; } }

        CustomHistogramViewModel customHistogramViewModel;
        public CustomHistogramViewModel CustomHistogramViewModel { get { return customHistogramViewModel; } }

        private Boolean isSummaryVisible = false;
        public Boolean IsSummaryVisible { get { return isSummaryVisible; } set { isSummaryVisible = value; OnPropertyChanged("IsSummaryVisible"); } }

        private Boolean isGroupedBarChartVisible = false;
        public Boolean IsGroupedBarChartVisible { get { return isGroupedBarChartVisible; } set { isGroupedBarChartVisible = value; OnPropertyChanged("IsGroupedBarChartVisible"); } }

        private Boolean isPivotTableVisible = false;
        public Boolean IsPivotTableVisible { get { return isPivotTableVisible; } set { isPivotTableVisible = value; OnPropertyChanged("IsPivotTableVisible"); } }                                                                                                                                                     

        private Boolean isCategoricalColumn;
        public Boolean IsCategoricalColumn { get { return isCategoricalColumn; } set { isCategoricalColumn = value; OnPropertyChanged("IsCategoricalColumn"); } }

        private Boolean isNumericalColumn;
        public Boolean IsNumericalColumn { get { return isNumericalColumn; } set { isNumericalColumn = value; OnPropertyChanged("IsNumericalColumn"); } }

        private Boolean isPivotBarChartVisible;
        public Boolean IsPivotBarChartVisible { get { return isPivotBarChartVisible; } set { isPivotBarChartVisible = value; OnPropertyChanged("IsPivotBarChartVisible"); } }

        private Boolean isPivotGroupedBarChartVisible;
        public Boolean IsPivotGroupedBarChartVisible { get { return isPivotGroupedBarChartVisible; } set { isPivotGroupedBarChartVisible = value; OnPropertyChanged("IsPivotGroupedBarChartVisible"); } }

        private Boolean isGroupedBy = false;
        public Boolean IsGroupedBy { get { return isGroupedBy; } set { isGroupedBy = value; OnPropertyChanged("IsGroupedBy"); } }

        View.PageView pageView;

        public PageViewModel(MainPageViewModel mainPageViewModel, View.PageView pageView)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.pageView = pageView;
            this.pivotTableViewModel = new PivotTableViewModel(mainPageViewModel, pageView.PivotTableView);
            this.customHistogramViewModel = new CustomHistogramViewModel(mainPageViewModel, pageView.CustomHistogramView);
        }

        public void ShowSummary(ColumnViewModel columnViewModel)
        {
            IsSummaryVisible = true;

            ColumnViewModel = columnViewModel;

            if (columnViewModel.Type == Model.ColumnType.Categorical)
            {
                IsCategoricalColumn = true;
                IsNumericalColumn = false;
                IsPivotBarChartVisible = false;
                // bar and pie

                // grouped bar chart가 될 수도 있음.
                pageView.BarChart.Data = mainPageViewModel.SheetViewModel.CountByColumnViewModel(columnViewModel)
                    .OrderBy(t => t.Item1.Order)
                    .Select(t => new Tuple<Object, Double>(t.Item1, t.Item2));
                pageView.BarChart.Update();

                if (mainPageViewModel.SheetViewModel.GroupedColumnViewModels.Count > 1 && !columnViewModel.IsGroupedBy)
                {
                    IsPivotTableVisible = true;
                    pivotTableViewModel.Preview(mainPageViewModel.SheetViewModel.GroupedColumnViewModels, columnViewModel);
                }
                else
                {
                    IsPivotTableVisible = false;
                }

                if (mainPageViewModel.SheetViewModel.GroupedColumnViewModels.Count == 1 && !columnViewModel.IsGroupedBy)
                {
                    IsGroupedBarChartVisible = true;

                    pageView.GroupedBarChart.Data = mainPageViewModel.SheetViewModel.CountByDoubleColumnViewModel(columnViewModel)
                        .OrderBy(t => t.Item1.Order * 10000 + t.Item2.Order)
                        .Select(tp => new Tuple<Object, Object, Double>(tp.Item1.ToString(), tp.Item2.ToString(), tp.Item3));
                    pageView.GroupedBarChart.Update();
                }
                else
                {
                    IsGroupedBarChartVisible = false;
                }
            }
            else
            {
                IsNumericalColumn = true;
                IsCategoricalColumn = false;
                IsPivotTableVisible = false;
                IsGroupedBarChartVisible = false;

                BoxPlotViewModel = DescriptiveStatistics.Analyze(
                    mainPageViewModel.TableViewModel.RowViewModels.Select(r => (Double)r.Cells[columnViewModel.Index].Content)
                    );

                pageView.BoxPlot.UpdateLayout();
                pageView.BoxPlot.Update();

                pageView.NumericalHistogram.Data = Util.HistogramCalculator.Bin(
                        pageView.BoxPlot.BoxPlotViewModel.Scale.DomainStart,
                        pageView.BoxPlot.BoxPlotViewModel.Scale.DomainEnd,
                        pageView.BoxPlot.BoxPlotViewModel.Scale.Step,
                        mainPageViewModel.TableViewModel.RowViewModels.Select(r => (Double)r.Cells[columnViewModel.Index].Content)
                    )
                    .Select(d => new Tuple<Object, Double>(d.Item1, d.Item3));

                pageView.NumericalHistogram.Update();

                if (mainPageViewModel.SheetViewModel.GroupedColumnViewModels.Count > 1)
                {
                    Int32 count = mainPageViewModel.SheetViewModel.GroupedColumnViewModels.Count;

                    ColumnViewModel g1 = mainPageViewModel.SheetViewModel.GroupedColumnViewModels[count - 2],
                                    g2 = mainPageViewModel.SheetViewModel.GroupedColumnViewModels[count - 1];

                    IsPivotGroupedBarChartVisible = true;

                    pageView.PivotGroupedBarChart.Data = mainPageViewModel.SheetViewModel
                        .GroupingResult
                        .OrderBy(g => g.Keys[g1].Order * 10000 + g.Keys[g2].Order)
                        .Select(g => new Tuple<Object, Object, Double>(
                            g.Keys[g1], 
                            g.Keys[g2], 
                            g.Rows.Select(r => (Double)r.Cells[columnViewModel.Index].Content).Average()
                        ));

                    pageView.PivotGroupedBarChart.Update();
                }
                else
                {
                    IsPivotGroupedBarChartVisible = false;
                }

                if (mainPageViewModel.SheetViewModel.GroupedColumnViewModels.Count == 1)
                {
                    IsPivotBarChartVisible = true;
                    ColumnViewModel groupedColumnViewModel = mainPageViewModel.SheetViewModel.GroupedColumnViewModels.First();

                    pageView.PivotBarChart.Data = mainPageViewModel.SheetViewModel
                        .GroupingResult
                        .OrderBy(g => g.Keys[groupedColumnViewModel].Order)
                        .Select(g => new Tuple<Object, Double>(
                            g.Keys[groupedColumnViewModel], g.Rows.Select(r => (Double)r.Cells[columnViewModel.Index].Content).Average()
                        ));

                    pageView.PivotBarChart.Update();
                }
                else
                {
                    IsPivotBarChartVisible = false;
                }

                // TODO: custom histogram 그리기
                customHistogramViewModel.Show(
                    mainPageViewModel.TableViewModel.RowViewModels.Select(
                        r => (Double)r.Cells[columnViewModel.Index].Content
                    )
                );                
            }

            pageView.UpdateCarousel();
        }

        public void Hide()
        {
            IsSummaryVisible = false;
            ColumnViewModel = null;
        }

        public void Tapped(View.PageView pageView)
        {
            mainPageViewModel.ExplorationViewModel.PageViewTapped(this, pageView);
        }

        public void GoUp()
        {
            pageView.GoUp();
            IsGroupedBy = false;
        }

        public void GoDown()
        {
            pageView.GoDown();
            IsGroupedBy = true;
        }

        public void StrokeAdded(InkStroke stroke)
        {
            /*
            Int32 index = 0;
            Rect rect = stroke.BoundingRect;
            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            foreach (Model.Bin bin in column.Bins.Select(b => b as Object).ToList())
            {
                Double x0 = LegendTextXGetter(bin, index),
                       y0 = LegendPatchYGetter(bin, index) + 10,
                       y1 = y0 + LegendPatchHeightGetter(bin, index) + 10;

                if (x0 <= center.X - mainPageViewModel.Width / 2 + ChartWidth && y0 <= center.Y && center.Y <= y1)
                {
                    bin.IsFilteredOut = !bin.IsFilteredOut;
                    break;
                }             
                index++;
            }

            d3.Scale.Ordinal xScale = new d3.Scale.Ordinal()
            {
                RangeStart = 70,
                RangeEnd = ChartWidth
            };
            foreach (Model.Bin bin in column.Bins.Where(b => !b.IsFilteredOut)) { xScale.Domain.Add(bin.Name); }
            XScale = xScale;

            Data = new d3.Selection.Data()
            {
                Real = column.Bins.Where(b => !b.IsFilteredOut).Select(b => b as Object).ToList()
            };

            LegendData = new d3.Selection.Data()
            {
                Real = column.Bins.Select(b => b as Object).ToList()
            };

            mainPageViewModel.UpdateFiltering();*/
        }

        
    }
}
