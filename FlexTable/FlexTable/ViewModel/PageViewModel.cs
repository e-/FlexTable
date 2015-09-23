﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using FlexTable.Model;
using Windows.UI.Xaml;
using FlexTable.View;
using Windows.UI.Xaml.Controls;

namespace FlexTable.ViewModel
{
    public class PageViewModel : NotifyViewModel
    {
        private ColumnViewModel columnViewModel;
        public ColumnViewModel ColumnViewModel { get { return columnViewModel; } set { columnViewModel = value; OnPropertyChanged("ColumnViewModel"); } }
        
        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

        PivotTableViewModel pivotTableViewModel;
        public PivotTableViewModel PivotTableViewModel => pivotTableViewModel; 

        /*CustomHistogramViewModel customHistogramViewModel;
        public CustomHistogramViewModel CustomHistogramViewModel { get { return customHistogramViewModel; } }
        */

        private Boolean isSummaryVisible = false;
        public Boolean IsSummaryVisible { get { return isSummaryVisible; } set { isSummaryVisible = value; OnPropertyChanged("IsSummaryVisible"); } }

        private Boolean isBarChartVisible = false;
        public Boolean IsBarChartVisible { get { return isBarChartVisible; } set { isBarChartVisible = value; OnPropertyChanged("IsBarChartVisible"); } }

        private Boolean isDescriptiveStatisticsVisible = false;
        public Boolean IsDescriptiveStatisticsVisible { get { return isDescriptiveStatisticsVisible; } set { isDescriptiveStatisticsVisible = value; OnPropertyChanged("IsDescriptiveStatisticsVisible"); } }
        
        private Boolean isDistributionVisible = false;
        public Boolean IsDistributionVisible { get { return isDistributionVisible; } set { isDistributionVisible = value; OnPropertyChanged("IsDistributionVisible"); } }

        private Boolean isGroupedBarChartVisible = false;
        public Boolean IsGroupedBarChartVisible { get { return isGroupedBarChartVisible; } set { isGroupedBarChartVisible = value; OnPropertyChanged("IsGroupedBarChartVisible"); } }

        private Boolean isScatterplotVisible = false;
        public Boolean IsScatterplotVisible { get { return isScatterplotVisible; } set { isScatterplotVisible = value; OnPropertyChanged("IsScatterplotVisible"); } }

        private Boolean isPivotTableVisible = false;
        public Boolean IsPivotTableVisible { get { return isPivotTableVisible; } set { isPivotTableVisible = value; OnPropertyChanged("IsPivotTableVisible"); } }

        private Boolean isCorrelationStatisticsVisible = false;
        public Boolean IsCorrelationStatisticsVisible { get { return isCorrelationStatisticsVisible; } set { isCorrelationStatisticsVisible = value; OnPropertyChanged("IsCorrelationStatisticsVisible"); } }

        private Boolean isGroupedBy = false;
        public Boolean IsGroupedBy { get { return isGroupedBy; } set { isGroupedBy = value; OnPropertyChanged("IsGroupedBy"); } }

        private Double pageHeight;
        public Double PageHeight { get { return pageHeight; } set { pageHeight = value;  OnPropertyChanged("PageHeight"); } }

        private Double pageWidth;
        public Double PageWidth { get { return pageWidth; } set { pageWidth = value; OnPropertyChanged("PageWidth"); } }

        public Func<Category, Func<RowViewModel, Boolean>> BarChartRowSelecter { get; set; }
        public Func<Category, Category, Func<RowViewModel, Boolean>> GroupedBarChartRowSelecter { get; set; }

        PageView pageView;

        public PageViewModel(MainPageViewModel mainPageViewModel, PageView pageView)
        {
            PageHeight = mainPageViewModel.Bounds.Height / 2;
            PageWidth = mainPageViewModel.Bounds.Width / 2;

            this.mainPageViewModel = mainPageViewModel;
            this.pageView = pageView;
            this.pivotTableViewModel = new PivotTableViewModel(mainPageViewModel, pageView.PivotTableView);
            //this.customHistogramViewModel = new CustomHistogramViewModel(mainPageViewModel, pageView.CustomHistogramView);
        }
        
        public void ShowSummary(ColumnViewModel columnViewModel)
        {
            IsSummaryVisible = true;
            IsBarChartVisible = false;
            IsDescriptiveStatisticsVisible = false;
            IsDistributionVisible = false;
            IsGroupedBarChartVisible = false;
            IsScatterplotVisible = false;
            IsPivotTableVisible = false;
            IsCorrelationStatisticsVisible = false;

            ColumnViewModel = columnViewModel;

            List<ColumnViewModel> selectedColumnViewModels = mainPageViewModel.ExplorationViewModel.SelectedColumnViewModels;

            List<ColumnViewModel> numericalColumns = selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).ToList();
            List<ColumnViewModel> categoricalColumns = selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ToList();

            switch (columnViewModel.Type)
            {
                case ColumnType.Categorical:
                    categoricalColumns.Add(columnViewModel);
                    break;
                case ColumnType.Numerical:
                    numericalColumns.Add(columnViewModel);
                    break;
            }

            List<GroupedRows> groupedRows = null;
            Int32 numericalCount = numericalColumns.Count;
            Int32 categoricalCount = categoricalColumns.Count;

            if (categoricalColumns.Count > 0)
            {
                groupedRows = SheetViewModel.GroupRecursive(mainPageViewModel.SheetViewModel.Sheet.Rows.ToList(), categoricalColumns, 0);
            }

            if (categoricalCount == 1 && numericalCount == 0)
            {
                IsBarChartVisible = true;

                Format(
                    pageView.BarChartTitle,
                    $"Frequency of <b>{columnViewModel.Column.Name}</b>"
                    );
                Int32 categoricalIndex = categoricalColumns[0].Index;
                BarChartRowSelecter = c => (r => r.Cells[categoricalIndex].Content == c);

                pageView.BarChart.HorizontalAxisLabel = columnViewModel.Column.Name;
                pageView.BarChart.VerticalAxisLabel = String.Format("Frequency");
                pageView.BarChart.Data = mainPageViewModel.SheetViewModel.CountByColumnViewModel(columnViewModel)
                    .OrderBy(t => t.Item1.Order)
                    .Select(t => new Tuple<Object, Double>(t.Item1, t.Item2));
                pageView.BarChart.Update();
            }
            else if (categoricalCount == 0 && numericalCount == 1)
            {
                DescriptiveStatisticsResult result = DescriptiveStatistics.Analyze(
                    mainPageViewModel.TableViewModel.RowViewModels.Select(r => (Double)r.Cells[columnViewModel.Index].Content)
                    );

                Format(
                    pageView.DescriptiveStatisticsTitle,
                    $"Descriptive Statistics of <b>{columnViewModel.Column.Name}</b>"
                    );
                IsDescriptiveStatisticsVisible = true;
                pageView.DescriptiveStatisticsView.DataContext = result;

                Format(
                    pageView.DistributionViewTitle,
                    $"Distribution of <b>{columnViewModel.Column.Name}</b>"
                    );
                IsDistributionVisible = true;
                pageView.DistributionView.Update(
                    result,
                    mainPageViewModel.TableViewModel.RowViewModels.Select(r => (Double)r.Cells[columnViewModel.Index].Content)
                    ); // 히스토그램 업데이트
            }
            else if (categoricalCount == 2 && numericalCount == 0)
            {
                IsGroupedBarChartVisible = true;

                Format(pageView.GroupedBarChartTitle, $"Frequency of <b>{categoricalColumns[1].Column.Name}</b> by <b>{categoricalColumns[0].Column.Name}</b>");
                Int32 categoricalIndex1 = categoricalColumns[0].Index,
                      categoricalIndex2 = categoricalColumns[1].Index;
                GroupedBarChartRowSelecter = (c1, c2) => (r => r.Cells[categoricalIndex1].Content == c1 && r.Cells[categoricalIndex2].Content == c2);
                pageView.GroupedBarChart.HorizontalAxisLabel = categoricalColumns[0].Column.Name;
                pageView.GroupedBarChart.VerticalAxisLabel = $"Frequency of {categoricalColumns[1].Column.Name}";
                pageView.GroupedBarChart.Data = groupedRows
                            .OrderBy(g => g.Keys[categoricalColumns[0]].Order * 10000 + g.Keys[categoricalColumns[1]].Order)
                            .Select(g => new Tuple<Object, Object, Double>(
                                g.Keys[categoricalColumns[0]],
                                g.Keys[categoricalColumns[1]],
                                g.Rows.Count
                            ));

                pageView.GroupedBarChart.Update();
            }
            else if (categoricalCount == 1 && numericalCount == 1)
            {
                IsBarChartVisible = true;

                Format(pageView.BarChartTitle, $"<b>{numericalColumns[0].HeaderName}</b> by <b>{categoricalColumns[0].Column.Name}</b>");
                Int32 categoricalIndex = categoricalColumns[0].Index;
                BarChartRowSelecter = c => (r => r.Cells[categoricalIndex].Content == c);
                pageView.BarChart.HorizontalAxisLabel = categoricalColumns[0].Column.Name;
                pageView.BarChart.VerticalAxisLabel = numericalColumns[0].HeaderName;
                pageView.BarChart.Data = groupedRows
                    .OrderBy(g => g.Keys[categoricalColumns[0]].Order)
                    .Select(g => new Tuple<Object, Double>(
                        g.Keys[categoricalColumns[0]],
                        numericalColumns[0].AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numericalColumns[0].Index].Content))
                        ));
                pageView.BarChart.Update();
            }
            else if (categoricalCount == 0 && numericalCount == 2)
            {
                CorrelationStatisticsResult result = CorrelationStatistics.Analyze(
                    numericalColumns[0].Column.Name,
                    numericalColumns[1].Column.Name,
                    mainPageViewModel.SheetViewModel.Sheet.Rows.Select(r => (Double)r.Cells[numericalColumns[0].Index].Content),
                    mainPageViewModel.SheetViewModel.Sheet.Rows.Select(r => (Double)r.Cells[numericalColumns[1].Index].Content)
                    );
                Format(pageView.CorrelationStatisticsTitle, $"Correlation between <b>{numericalColumns[0].Column.Name}</b> and <b>{numericalColumns[1].Column.Name}</b>");

                IsCorrelationStatisticsVisible = true;
                pageView.CorrelationStatisticsView.DataContext = result;

                Format(pageView.ScatterplotTitle, $"<b>{numericalColumns[0].Column.Name}</b> vs. <b>{numericalColumns[1].Column.Name}</b>");

                IsScatterplotVisible = true;
                pageView.Scatterplot.LegendVisibility = Visibility.Collapsed;
                pageView.Scatterplot.HorizontalAxisLabel = numericalColumns[0].Column.Name;
                pageView.Scatterplot.VerticalAxisLabel = numericalColumns[1].Column.Name;
                pageView.Scatterplot.Data = mainPageViewModel.SheetViewModel.Sheet.Rows
                    .Select(r => new Tuple<Object, Double, Double>(0, (Double)r.Cells[numericalColumns[0].Index].Content, (Double)r.Cells[numericalColumns[1].Index].Content));
                
                pageView.Scatterplot.Update();
            }
            else if (categoricalCount == 3 && numericalCount == 0)
            {
                IsPivotTableVisible = true;

                Format(pageView.PivotTableTitle, $"Frequency of <b>{categoricalColumns[2].Column.Name}</b> by <b>{categoricalColumns[0].Column.Name}</b> and <b>{numericalColumns[1].Column.Name}</b>");
                pivotTableViewModel.Preview(
                    selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ToList(), 
                    columnViewModel,
                    new List<ColumnViewModel>(),
                    groupedRows
                    );

                // 그룹 바차트를 여러개 그려야한다
            }
            else if (categoricalCount == 2 && numericalCount == 1)
            {
                // 테이블을 그린다
                IsPivotTableVisible = true;
                Format(pageView.PivotTableTitle, $"<b>{numericalColumns[0].HeaderName}</b> by <b>{categoricalColumns[0].Column.Name}</b> and <b>{categoricalColumns[1].Column.Name}</b>");

                pivotTableViewModel.Preview(
                    new List<ColumnViewModel>() { categoricalColumns[0] },
                    categoricalColumns[1],
                    numericalColumns,
                    groupedRows
                    );

                // 그룹 바 차트를 그린다
                IsGroupedBarChartVisible = true;

                Format(pageView.GroupedBarChartTitle, $"<b>{numericalColumns[0].HeaderName}</b> by <b>{categoricalColumns[0].Column.Name}</b> and <b>{categoricalColumns[1].Column.Name}</b>");
                Int32 categoricalIndex1 = categoricalColumns[0].Index,
                      categoricalIndex2 = categoricalColumns[1].Index;
                GroupedBarChartRowSelecter = (c1, c2) => (r => r.Cells[categoricalIndex1].Content == c1 && r.Cells[categoricalIndex2].Content == c2);

                pageView.GroupedBarChart.HorizontalAxisLabel = categoricalColumns[0].Column.Name;
                pageView.GroupedBarChart.VerticalAxisLabel = numericalColumns[0].HeaderName;
                pageView.GroupedBarChart.Data = groupedRows
                            .OrderBy(g => g.Keys[categoricalColumns[0]].Order * 10000 + g.Keys[categoricalColumns[1]].Order)
                            .Select(g => new Tuple<Object, Object, Double>(
                                g.Keys[categoricalColumns[0]],
                                String.Format("{0} {1}", categoricalColumns[1].Column.Name, g.Keys[categoricalColumns[1]]),
                                numericalColumns[0].AggregativeFunction.Aggregate(g.Rows.Select(row => (Double)row.Cells[numericalColumns[0].Index].Content))
                            ));

                pageView.GroupedBarChart.Update();
            }
            else if (categoricalCount == 1 && numericalCount == 2)
            {
                IsScatterplotVisible = true;
                Format(pageView.ScatterplotTitle, $"<b>{numericalColumns[0].Column.Name}</b> vs. <b>{numericalColumns[1].Column.Name}</b> colored by <b>{categoricalColumns[0].Column.Name}</b>");

                pageView.Scatterplot.LegendVisibility = Visibility.Visible;
                pageView.Scatterplot.HorizontalAxisLabel = numericalColumns[0].Column.Name;
                pageView.Scatterplot.VerticalAxisLabel = numericalColumns[1].Column.Name;
                pageView.Scatterplot.Data = mainPageViewModel.SheetViewModel.Sheet.Rows
                    .Select(r => new Tuple<Object, Double, Double>(
                        r.Cells[categoricalColumns[0].Index].Content,
                        (Double)r.Cells[numericalColumns[0].Index].Content, 
                        (Double)r.Cells[numericalColumns[1].Index].Content));

                pageView.Scatterplot.Update();

                // 테이블을 그린다
                IsPivotTableVisible = true;
                Format(pageView.PivotTableTitle, $"<b>{numericalColumns[0].HeaderName}</b> and <b>{numericalColumns[1].HeaderName}</b> by <b>{categoricalColumns[0].Column.Name}</b>");

                pivotTableViewModel.Preview(
                    categoricalColumns,
                    null,
                    numericalColumns,
                    groupedRows
                    );
            }
            else if (categoricalCount == 0 && numericalCount == 3)
            {
                IsBarChartVisible = true;
                // 지금 필요없다.
            }
            else if (categoricalCount >= 1 && numericalCount == 0)
            {
                // 테이블을 그린다
                IsPivotTableVisible = true;

                pivotTableViewModel.Preview(
                    categoricalColumns.Where((c, index) => index != categoricalColumns.Count - 1).ToList(),
                    categoricalColumns.Last(),
                    numericalColumns,
                    groupedRows
                    );
            }
            else if (categoricalCount >= 1 && numericalCount == 1)
            {
                // 테이블을 그린다
                IsPivotTableVisible = true;

                pivotTableViewModel.Preview(
                    categoricalColumns.Where((c, index) => index != categoricalColumns.Count - 1).ToList(),
                    categoricalColumns.Last(),
                    numericalColumns,
                    groupedRows
                    );
            }
            else if (categoricalCount >= 1 && numericalCount > 1)
            {
                // 테이블을 그린다
                IsPivotTableVisible = true;

                if (numericalCount * categoricalColumns.Last().Categories.Count <= 12)
                {
                    pivotTableViewModel.Preview(
                        categoricalColumns.Where((c, index) => index != categoricalColumns.Count - 1).ToList(),
                        categoricalColumns.Last(),
                        numericalColumns,
                        groupedRows
                        );
                }
                else
                {
                    pivotTableViewModel.Preview(
                        categoricalColumns,
                        null, // 여길 채워서 남은 카테고리컬 하나를 여기로 시각화 가능
                        numericalColumns,
                        groupedRows
                        );
                }
            }
            else
            {
                IsBarChartVisible = true;
            }
            
            pageView.UpdateCarousel();
        }

        public void Hide()
        {
            IsSummaryVisible = false;
            ColumnViewModel = null;
        }

        public void Tapped(PageView pageView)
        {
            mainPageViewModel.ExplorationViewModel.PageViewTapped(this, pageView);
        }

        public void GoUp()
        {
            pageView.GoUp();
            IsGroupedBy = false;
            pageView.UpdateCarousel();
        }

        public void GoDown()
        {
            pageView.GoDown();
            IsGroupedBy = true;
            pageView.UpdateCarousel();
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

        public void Format(TextBlock textBlock, String html)
        {
            Util.HtmlToTextBlockFormatter.Format(
                html,
                textBlock
            );

            Double fontSize = 42;

            for (; fontSize > 10; fontSize -= 1) {
                textBlock.FontSize = fontSize;
                textBlock.Measure(new Windows.Foundation.Size(5000, 5000));

                if (textBlock.ActualWidth < (Double)App.Current.Resources["ParagraphWidth"]) break;
            }
        }
    }
}
