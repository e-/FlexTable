using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using FlexTable.Model;
using Windows.UI.Xaml;
using FlexTable.View;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Text;
using System.Diagnostics;
using FlexTable.Crayon.Chart;
using d3.ColorScheme;

namespace FlexTable.ViewModel
{

    public partial class PageViewModel : NotifyViewModel
    {
        public const Int32 BarChartMaximumRecordNumber = 12;
        public const Int32 GroupedBarChartMaximumRecordNumber = 48;
        public const Int32 LineChartMaximumSeriesNumber = BarChartMaximumRecordNumber;
        public const Int32 LineChartMaximumPointNumberInASeries = 20;
        public const Int32 ScatterplotMaximumCategoryNumber = BarChartMaximumRecordNumber;

        public enum PageViewState { Empty, Previewing, Selected, Undoing }

        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

        PivotTableViewModel pivotTableViewModel;
        public PivotTableViewModel PivotTableViewModel => pivotTableViewModel;

        public PageViewState OldState { get; set; } = PageViewState.Empty;

        private PageViewState state = PageViewState.Empty;
        public PageViewState State
        {
            get { return state; }
            set
            {
                OldState = state;
                state = value;
                OnPropertyChanged(nameof(IsPreviewing));
                OnPropertyChanged(nameof(IsUndoing));
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public Boolean IsPreviewing { get { return state == PageViewState.Previewing; } }
        public Boolean IsUndoing { get { return state == PageViewState.Undoing; } }
        public Boolean IsEmpty { get { return state == PageViewState.Empty; } }
        public Boolean IsSelected { get { return state == PageViewState.Selected; } }

        private Boolean isBarChartVisible = false;
        public Boolean IsBarChartVisible { get { return isBarChartVisible; } set { isBarChartVisible = value; OnPropertyChanged(nameof(IsBarChartVisible)); } }

        private Boolean isLineChartVisible = false;
        public Boolean IsLineChartVisible { get { return isLineChartVisible; } set { isLineChartVisible = value; OnPropertyChanged(nameof(IsLineChartVisible)); } }

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

        private Boolean isBarChartWarningVisible = false;
        public Boolean IsBarChartWarningVisible { get { return isBarChartWarningVisible; } set { isBarChartWarningVisible = value; OnPropertyChanged(nameof(IsBarChartWarningVisible)); } }

        private Boolean isLineChartWarningVisible = false;
        public Boolean IsLineChartWarningVisible { get { return isLineChartWarningVisible; } set { isLineChartWarningVisible = value; OnPropertyChanged(nameof(IsLineChartWarningVisible)); } }

        private Boolean isGroupedBarChartWarningVisible = false;
        public Boolean IsGroupedBarChartWarningVisible { get { return isGroupedBarChartWarningVisible; } set { isGroupedBarChartWarningVisible = value; OnPropertyChanged(nameof(IsGroupedBarChartWarningVisible)); } }

        private Boolean isScatterplotWarningVisible = false;
        public Boolean IsScatterplotWarningVisible { get { return isScatterplotWarningVisible; } set { isScatterplotWarningVisible = value; OnPropertyChanged(nameof(IsScatterplotWarningVisible)); } }

        private Boolean isPrimaryUndoMessageVisible = true;
        public Boolean IsPrimaryUndoMessageVisible { get { return isPrimaryUndoMessageVisible; } set { isPrimaryUndoMessageVisible = value; OnPropertyChanged(nameof(IsPrimaryUndoMessageVisible)); } }

        PageView pageView;

        public ViewStatus ViewStatus { get; set; } // 현재 페이지 뷰의 viewStatus
        
        public PageViewModel(MainPageViewModel mainPageViewModel, PageView pageView)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.pageView = pageView;
            this.pivotTableViewModel = new PivotTableViewModel(mainPageViewModel, pageView.PivotTableView);
        }        

        /// <summary>
        /// 상태가 바뀌었으면 일단 exploration에 보고를 한 후 거기서 reflect 메소드를 호출해서 이 페이지 뷰와 뷰모델에 반영한다.
        /// </summary>
        /// <param name="pageView"></param>
        /// <param name="isUndo"></param>
        public void StateChanged(PageView pageView)
        {
            mainPageViewModel.ExplorationViewModel.PageViewStateChanged(this, pageView);
        }
        
        /// <summary>
        /// 페이지뷰의 상태가 바꾸면 우선 exploration view에 보고를 한 후 거기서 reflect를 호출함. exploration view에 호출하지 않아도 될 때만 직접 reflect를 해야함
        /// </summary>
        /// <param name="trackPreviousParagraph"></param>
        public void Reflect(ReflectReason reason)
        {
            Boolean trackPreviousParagraph = reason.TrackPreviousParagraph(); // HasFlag(ReflectType2.TrackPreviousParagraph);
            Boolean chartTypeChanged = reason.ChartTypeChanged(); // reflectType.HasFlag(ReflectType2.OnCreate);
            Boolean selectionChanged = chartTypeChanged || reason.SelectionTypeChanged(); // reflectType.HasFlag(ReflectType2.OnSelectionChanged);

            if (chartTypeChanged)
            {
                IsBarChartVisible = false;
                IsLineChartVisible = false;
                IsDescriptiveStatisticsVisible = false;
                IsDistributionVisible = false;
                IsGroupedBarChartVisible = false;
                IsScatterplotVisible = false;
                IsPivotTableVisible = false;
                IsCorrelationStatisticsVisible = false;

                IsBarChartWarningVisible = false;
                IsLineChartWarningVisible = false;
                IsGroupedBarChartWarningVisible = false;
                IsScatterplotWarningVisible = false;

                //pageView.BreadcrumbView.Update();
            }

            List<GroupedRows> groupedRows = ViewStatus.GroupedRows;
            Object firstChartTag = "dummy tag wer";
            Boolean useTransition = !chartTypeChanged && selectionChanged || reason == ReflectReason.ColumnChanged; /*(selectionChanged && !chartTypeChanged) 
                || (reason != ReflectReason2.FilterOut && reason != ReflectReason2.PreviewRequested 
                && reason != ReflectReason2.ColumnViewModelUnselected && reason != ReflectReason2.Undo);*/

            if (ViewStatus.IsC)
            {
                if (chartTypeChanged)
                {
                    DrawFrequencyHistogram(ViewStatus.FirstCategorical, groupedRows);
                    DrawPivotTable();
                }

                if (selectionChanged)
                {
                    SetFrequencyHistogramSelection(ViewStatus.FirstCategorical, pageView.SelectedRows);
                }

                pageView.BarChart.Update(useTransition);
            }
            else if (ViewStatus.IsN)
            {
                if (chartTypeChanged)
                {
                    DrawDistributionHistogram(ViewStatus.FirstNumerical);
                    DrawDescriptiveStatistics(ViewStatus.FirstNumerical);
                    DrawPivotTable();
                }

                if (selectionChanged)
                {
                    SetDistributionHistogramSelection(ViewStatus.FirstNumerical, pageView.SelectedRows);
                }

                pageView.DistributionView.Histogram.Update(useTransition);
            }
            else if (ViewStatus.IsCN)
            {
                if (chartTypeChanged)
                {
                    DrawBarChart(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, groupedRows);
                    DrawLineChart(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, groupedRows);
                    DrawPivotTable();
                }

                if (selectionChanged)
                {
                    SetBarChartSelection(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, pageView.SelectedRows);
                    SetLineChartSelection(ViewStatus.FirstNumerical, pageView.SelectedRows);
                }

                pageView.BarChart.Update(useTransition);
                pageView.LineChart.Update(useTransition);

                firstChartTag = ViewStatus.FirstCategorical.CategoricalType == CategoricalType.Ordinal ? pageView.LineChart.Tag : pageView.BarChart.Tag;
            }
            else if (ViewStatus.IsCC)
            {
                if (chartTypeChanged)
                {
                    DrawGroupedBarChart(ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, groupedRows);
                    DrawPivotTable();
                }

                if (selectionChanged)
                {
                    SetGroupedBarChartSelection(ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, pageView.SelectedRows);
                }

                pageView.GroupedBarChart.Update(useTransition);
            }
            else if (ViewStatus.IsNN)
            {
                if (chartTypeChanged)
                {
                    DrawCorrelatonStatistics(ViewStatus.FirstNumerical, ViewStatus.SecondNumerical);
                    DrawScatterplot(ViewStatus.FirstNumerical, ViewStatus.SecondNumerical);
                    if (ViewStatus.FirstNumerical.Unit == ViewStatus.SecondNumerical.Unit)
                    {
                        DrawGroupedBarChartNN(ViewStatus.FirstNumerical, ViewStatus.SecondNumerical, groupedRows);
                    }
                }

                if(selectionChanged)
                {
                    SetScatterplotSelection(pageView.SelectedRows);
                    if (ViewStatus.FirstNumerical.Unit == ViewStatus.SecondNumerical.Unit)
                    {
                        SetGroupedBarChartNNSelection(ViewStatus.FirstNumerical, ViewStatus.SecondNumerical, pageView.SelectedRows);
                    }
                }

                pageView.Scatterplot.Update(useTransition);
                if (ViewStatus.FirstNumerical.Unit == ViewStatus.SecondNumerical.Unit)
                {
                    pageView.GroupedBarChart.Update(useTransition);
                }
            }
            else if (ViewStatus.IsCCC)
            {
                DrawPivotTable();
            }
            else if (ViewStatus.IsCCN)
            {
                if (chartTypeChanged)
                {
                    DrawGroupedBarChart(ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, ViewStatus.FirstNumerical, groupedRows);
                    DrawLineChart(ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, ViewStatus.FirstNumerical, groupedRows);
                    DrawPivotTable();
                }

                if (selectionChanged)
                {
                    SetGroupedBarChartSelection(ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, ViewStatus.FirstNumerical, pageView.SelectedRows);
                    SetLineChartSelection(ViewStatus.FirstNumerical, pageView.SelectedRows);
                }

                pageView.GroupedBarChart.Update(useTransition);
                pageView.LineChart.Update(useTransition);
            }
            else if (ViewStatus.IsCNN)
            {
                if (chartTypeChanged)
                {
                    DrawScatterplot(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, ViewStatus.SecondNumerical);

                    if (ViewStatus.FirstNumerical.Unit == ViewStatus.SecondNumerical.Unit)
                    {
                        DrawGroupedBarChartCNN(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, ViewStatus.SecondNumerical, groupedRows);
                        DrawLineChartCNN(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, ViewStatus.SecondNumerical, groupedRows);
                    }

                    DrawPivotTable();
                }

                if (selectionChanged)
                {
                    SetScatterplotSelection(pageView.SelectedRows);

                    if (ViewStatus.FirstNumerical.Unit == ViewStatus.SecondNumerical.Unit)
                    {
                        SetGroupedBarChartCNNSelection(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, ViewStatus.SecondNumerical, pageView.SelectedRows);
                        SetLineChartCNNSelection(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, ViewStatus.SecondNumerical, pageView.SelectedRows);
                    }
                }

                pageView.Scatterplot.Update(useTransition);
                if (ViewStatus.FirstNumerical.Unit == ViewStatus.SecondNumerical.Unit)
                {
                    pageView.GroupedBarChart.Update(useTransition);
                    pageView.LineChart.Update(useTransition);
                }
            }
            else if (ViewStatus.IsNNN)
            {
                // ???
                IsBarChartVisible = true;
                // 지금 필요없다.
            }
            else if (ViewStatus.IsCnN0)
            {
                DrawPivotTable();
            }
            else if (ViewStatus.IsCnN1)
            {
                DrawPivotTable();
            }
            else if (ViewStatus.IsCnNn)
            {
                DrawPivotTable();
            }
            else
            {
                IsBarChartVisible = true;
            }

            pageView.UpdateCarousel(false/*trackPreviousParagraph*/, null);// firstChartTag?.ToString());
        }

        
        
        #region Visualizaiton Generator

        void DrawFrequencyHistogram(ColumnViewModel categorical, List<GroupedRows> groupedRows)
        {
            IsBarChartVisible = true;

            pageView.BarChartTitle.Children.Clear();
            AddEditableTitle(pageView.BarChartTitle, Const.Loader.GetString("FrequencyTitle"), categorical);
            
            Int32 index = categorical.Index;

            pageView.BarChart.YStartsFromZero = true;
            pageView.BarChart.HorizontalAxisTitle = categorical.Name;
            pageView.BarChart.VerticalAxisTitle = Const.Loader.GetString("Frequency");
            pageView.BarChart.Data = groupedRows
                .Select(grs => new BarChartDatum()
                {
                    Key = grs.Keys[categorical],
                    Value = grs.Rows.Count,
                    EnvelopeValue = grs.Rows.Count,
                    Rows = null,
                    EnvelopeRows = grs.Rows,
                    ColumnViewModel = categorical
                })
                .Take(BarChartMaximumRecordNumber).ToList();
            if (groupedRows.Count > BarChartMaximumRecordNumber) IsBarChartWarningVisible = true;
        }

        void SetFrequencyHistogramSelection(ColumnViewModel categorical, IEnumerable<Row> selectedRows)
        {
            foreach(BarChartDatum barChartDatum in pageView.BarChart.Data)
            {
                if (selectedRows.Count() == 0)
                {
                    barChartDatum.Rows = null;
                    barChartDatum.Value = barChartDatum.EnvelopeValue;
                }
                else
                {
                    barChartDatum.Rows = barChartDatum.EnvelopeRows.Intersect(selectedRows).ToList();
                    barChartDatum.Value = barChartDatum.Rows.Count();
                }
            }
        }

        void DrawDescriptiveStatistics(ColumnViewModel numerical)
        {
            DescriptiveStatisticsViewModel result = DescriptiveStatistics.Analyze(
                mainPageViewModel,
                    mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical.Index].Content)
                    );

            pageView.DescriptiveStatisticsTitle.Children.Clear();
            AddEditableTitle(pageView.DescriptiveStatisticsTitle, Const.Loader.GetString("DescriptiveStatisticsTitle"), numerical);

            IsDescriptiveStatisticsVisible = true;
            pageView.DescriptiveStatisticsView.DataContext = result;            
        }

        void DrawDistributionHistogram(ColumnViewModel numerical)
        {
            pageView.DistributionViewTitle.Children.Clear();

            AddEditableTitle(pageView.DistributionViewTitle, Const.Loader.GetString("DistributionTitle"), numerical);
            
            IsDistributionVisible = true;
            pageView.DistributionView.Feed(mainPageViewModel.SheetViewModel.FilteredRows, numerical);
        }

        void SetDistributionHistogramSelection(ColumnViewModel numerical, IEnumerable<Row> selectedRows)
        {
            foreach (BarChartDatum barChartDatum in pageView.DistributionView.Histogram.Data)
            {
                if (selectedRows.Count() == 0)
                {
                    barChartDatum.Rows = null;
                    barChartDatum.Value = barChartDatum.EnvelopeValue;
                }
                else
                {
                    barChartDatum.Rows = barChartDatum.EnvelopeRows.Intersect(selectedRows).ToList();
                    barChartDatum.Value = barChartDatum.Rows.Count();
                }
            }
        }

        void DrawGroupedBarChart(ColumnViewModel categorical1, ColumnViewModel categorical2, List<GroupedRows> groupedRows)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            AddEditableTitle(pageView.GroupedBarChartTitle, Const.Loader.GetString("ChartTitleCC"), categorical1, categorical2);

            pageView.GroupedBarChart.YStartsFromZero = true;
            pageView.GroupedBarChart.HorizontalAxisTitle = categorical1.Name;
            pageView.GroupedBarChart.VerticalAxisTitle = Const.Loader.GetString("Frequency");

            if (groupedRows.Count > GroupedBarChartMaximumRecordNumber)
            {
                groupedRows = groupedRows.Take(GroupedBarChartMaximumRecordNumber).ToList();
                IsGroupedBarChartWarningVisible = true;
            }

            var validLegends = groupedRows.GroupBy(g => g.Keys[categorical2]).Select(g => g.Key);

            if (validLegends.Count() > BarChartMaximumRecordNumber)
            {
                // number of categories in legend
                validLegends = validLegends.Take(BarChartMaximumRecordNumber);
                IsGroupedBarChartWarningVisible = true;
            }

            pageView.GroupedBarChart.Data = groupedRows
                        .GroupBy(g => g.Keys[categorical1])
                        .Select(gs =>
                        {
                            GroupedBarChartDatum datum = new GroupedBarChartDatum()
                            {
                                ColumnViewModel = categorical1,
                                Key = gs.Key
                            };

                            datum.Children = gs.Where(g => validLegends.Contains(g.Keys[categorical2]))
                                .Select(g => new BarChartDatum()
                                {
                                    Key = g.Keys[categorical2],
                                    Value = g.Rows.Count,
                                    EnvelopeValue = g.Rows.Count,
                                    ColumnViewModel = categorical2,
                                    Parent = datum,
                                    Rows = null,
                                    EnvelopeRows = g.Rows,
                                    Order = categorical2.Categories.IndexOf(g.Keys[categorical2] as Category) * categorical2.SortDirection
                                }).ToList();
                            return datum;
                        })
                        .Where(datum => datum.Children != null && datum.Children.Count > 0)
                        .ToList();            
        }

        void SetGroupedBarChartSelection(ColumnViewModel categorical1, ColumnViewModel categorical2, IEnumerable<Row> selectedRows)
        {
            if (selectedRows.Count() == 0)
            {
                foreach (GroupedBarChartDatum groupedBarChartDatum in pageView.GroupedBarChart.Data)
                {
                    foreach(BarChartDatum barChartDatum in groupedBarChartDatum.Children)
                    {
                        barChartDatum.Rows = null;
                        barChartDatum.Value = barChartDatum.EnvelopeValue;
                    }
                }
            }
            else
            {
                foreach (GroupedBarChartDatum groupedBarChartDatum in pageView.GroupedBarChart.Data)
                {
                    foreach (BarChartDatum barChartDatum in groupedBarChartDatum.Children)
                    {
                        barChartDatum.Rows = barChartDatum.EnvelopeRows.Intersect(selectedRows).ToList();
                        barChartDatum.Value = barChartDatum.Rows.Count();
                    }
                }
            }
        }

        void DrawGroupedBarChart(ColumnViewModel categorical1, ColumnViewModel categorical2, ColumnViewModel numerical, List<GroupedRows> groupedRows)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            AddEditableTitle(pageView.GroupedBarChartTitle, Const.Loader.GetString("ChartTitleCCN"), categorical1, categorical2, numerical);

            pageView.GroupedBarChart.YStartsFromZero = false;
            pageView.GroupedBarChart.HorizontalAxisTitle = categorical1.Name;
            pageView.GroupedBarChart.VerticalAxisTitle = numerical.HeaderNameWithUnit;

            if (groupedRows.Count > GroupedBarChartMaximumRecordNumber)
            {
                groupedRows = groupedRows.Take(GroupedBarChartMaximumRecordNumber).ToList();
                IsGroupedBarChartWarningVisible = true;
            }

            var validLegends = groupedRows.GroupBy(g => g.Keys[categorical2]).Select(g => g.Key);

            if (validLegends.Count() > BarChartMaximumRecordNumber)
            {
                // number of categories in legend
                validLegends = validLegends.Take(BarChartMaximumRecordNumber);
                IsGroupedBarChartWarningVisible = true;
            }

            pageView.GroupedBarChart.Data = groupedRows
                        .GroupBy(g => g.Keys[categorical1])
                        .Select(gs =>
                        {
                            GroupedBarChartDatum datum = new GroupedBarChartDatum()
                            {
                                ColumnViewModel = categorical1,
                                Key = gs.Key
                            };

                            datum.Children = gs.Where(g => validLegends.Contains(g.Keys[categorical2]))
                            .Select(g => new BarChartDatum()
                            {
                                Key = g.Keys[categorical2],
                                Value = numerical.AggregativeFunction.Aggregate(g.Rows.Select(row => (Double)row.Cells[numerical.Index].Content)),
                                EnvelopeValue = numerical.AggregativeFunction.Aggregate(g.Rows.Select(row => (Double)row.Cells[numerical.Index].Content)),
                                ColumnViewModel = categorical2,
                                Parent = datum,
                                Rows = null,
                                EnvelopeRows = g.Rows,
                                Order = categorical2.Categories.IndexOf(g.Keys[categorical2] as Category) * categorical2.SortDirection
                            }).ToList();
                            return datum;
                        })
                        .Where(datum => datum.Children != null && datum.Children.Count > 0)
                        .ToList();
        }

        void SetGroupedBarChartSelection(ColumnViewModel categorical1, ColumnViewModel categorical2, ColumnViewModel numerical, IEnumerable<Row> selectedRows)
        {
            if(selectedRows.Count() == 0)
            {
                foreach(GroupedBarChartDatum gbcd in pageView.GroupedBarChart.Data)
                {
                    foreach(BarChartDatum bcd in gbcd.Children)
                    {
                        bcd.Rows = null;
                        bcd.Value = bcd.EnvelopeValue;
                    }
                }
            }
            else
            {
                foreach (GroupedBarChartDatum gbcd in pageView.GroupedBarChart.Data)
                {
                    foreach (BarChartDatum bcd in gbcd.Children)
                    {
                        bcd.Rows = bcd.EnvelopeRows.Intersect(selectedRows).ToList();
                        bcd.Value = numerical.AggregativeFunction.Aggregate(bcd.Rows.Select(row => (Double)row.Cells[numerical.Index].Content));
                    }
                }
            }
        }

        void DrawBarChart(ColumnViewModel categorical, ColumnViewModel numerical, List<GroupedRows> groupedRows)
        {
            IsBarChartVisible = true;

            pageView.BarChartTitle.Children.Clear();
            AddEditableTitle(pageView.BarChartTitle, Const.Loader.GetString("ChartTitleCN"), categorical, numerical);

            pageView.BarChart.YStartsFromZero = false;
            pageView.BarChart.HorizontalAxisTitle = categorical.Name;
            pageView.BarChart.VerticalAxisTitle = numerical.HeaderNameWithUnit;
            pageView.BarChart.Data = groupedRows
                .Select(g => new BarChartDatum()
                {
                    Key = g.Keys[categorical],
                    Value = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                    EnvelopeValue = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                    Rows = null,
                    EnvelopeRows = g.Rows,
                    ColumnViewModel = categorical
                })
                .Take(BarChartMaximumRecordNumber).ToList();

            if (groupedRows.Count > BarChartMaximumRecordNumber) IsBarChartWarningVisible = true;
        }

        void SetBarChartSelection(ColumnViewModel categorical, ColumnViewModel numerical, IEnumerable<Row> selectedRows)
        {
            foreach (BarChartDatum barChartDatum in pageView.BarChart.Data)
            {
                if (selectedRows.Count() == 0)
                {
                    barChartDatum.Rows = null;
                    barChartDatum.Value = barChartDatum.EnvelopeValue;
                }
                else
                {
                    barChartDatum.Rows = barChartDatum.EnvelopeRows.Intersect(selectedRows).ToList();
                    barChartDatum.Value = numerical.AggregativeFunction.Aggregate(barChartDatum.Rows.Select(r => (Double)r.Cells[numerical.Index].Content));
                }
            }
        }

        void DrawLineChart(ColumnViewModel categorical, ColumnViewModel numerical, List<GroupedRows> groupedRows) // 라인 하나
        {
            //라인 차트로 보고 싶을 때
            IsLineChartVisible = true;


            pageView.LineChartTitle.Children.Clear();
            AddEditableTitle(pageView.LineChartTitle, Const.Loader.GetString("ChartTitleCN"), categorical, numerical);

            pageView.LineChart.YStartsFromZero = false;
            pageView.LineChart.HorizontalAxisTitle = categorical.Name;
            pageView.LineChart.VerticalAxisTitle = numerical.HeaderNameWithUnit;
            pageView.LineChart.AutoColor = false;

            LineChartDatum datum = new LineChartDatum()
            {
                ColumnViewModel = numerical,
                Key = numerical.AggregatedName
            };

            datum.DataPoints = groupedRows
                .Select(g => new DataPoint() {
                    Item1 = g.Keys[categorical],
                    Item2 = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                    EnvelopeItem2 = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                    Parent = datum,
                    EnvelopeRows = g.Rows,
                    Rows = null
                })
                .Take(LineChartMaximumPointNumberInASeries)
                .ToList();

            pageView.LineChart.Data = new List<LineChartDatum>() { datum };
            if (groupedRows.Count > LineChartMaximumPointNumberInASeries) IsLineChartWarningVisible = true;
        }

        void SetLineChartSelection(ColumnViewModel numerical, IEnumerable<Row> selectedRows)
        {
            if (selectedRows.Count() == 0)
            {
                foreach (LineChartDatum lineChartDatum in pageView.LineChart.Data)
                {
                    foreach (DataPoint dataPoint in lineChartDatum.DataPoints)
                    {
                        dataPoint.Rows = null;
                        dataPoint.Item2 = dataPoint.EnvelopeItem2;
                    }
                }
            }
            else
            {
                foreach (LineChartDatum lineChartDatum in pageView.LineChart.Data)
                {
                    foreach (DataPoint dataPoint in lineChartDatum.DataPoints)
                    {
                        dataPoint.Rows = dataPoint.EnvelopeRows.Intersect(selectedRows).ToList();
                        dataPoint.Item2 = numerical.AggregativeFunction.Aggregate(dataPoint.Rows.Select(r => (Double)r.Cells[numerical.Index].Content));
                    }
                }
            }
        }


        void DrawLineChart(ColumnViewModel categorical1, ColumnViewModel categorical2, ColumnViewModel numerical, List<GroupedRows> groupedRows)
        {
            Debug.Assert(categorical1.Type == ColumnType.Categorical);
            Debug.Assert(categorical2.Type == ColumnType.Categorical);
            Debug.Assert(numerical.Type == ColumnType.Numerical);
            // 그룹 라인 차트를 그린다.
            IsLineChartVisible = true;

            pageView.LineChartTitle.Children.Clear();
            AddEditableTitle(pageView.LineChartTitle, Const.Loader.GetString("ChartTitleCCN"), categorical1, categorical2, numerical);
            
            pageView.LineChart.YStartsFromZero = false;
            pageView.LineChart.HorizontalAxisTitle = categorical1.Name;
            pageView.LineChart.VerticalAxisTitle = numerical.HeaderNameWithUnit;
            pageView.LineChart.AutoColor = true;

            var rows =
                groupedRows
                .GroupBy(grs => grs.Keys[categorical2]) // 먼저 묶고 
                .Select(group =>
                {
                    LineChartDatum datum = new LineChartDatum()
                    {
                        ColumnViewModel = categorical2,
                        Key = group.Key
                    };

                    datum.DataPoints = group.Select(g => new DataPoint()
                    {
                        Item1 = g.Keys[categorical1],
                        Item2 = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                        EnvelopeItem2 = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                        Parent = datum,
                        Rows = null,
                        EnvelopeRows = g.Rows
                    }).ToList();

                    return datum;
                });

            if (rows.Count() > LineChartMaximumSeriesNumber) IsLineChartWarningVisible = true;
            if (rows.Max(row => row.DataPoints.Count) > LineChartMaximumPointNumberInASeries) IsLineChartWarningVisible = true;

            rows = rows.Take(LineChartMaximumSeriesNumber);
            pageView.LineChart.Data = rows.ToList();
        }

        void DrawScatterplot(ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            IsScatterplotVisible = true;

            pageView.ScatterplotTitle.Children.Clear();
            AddEditableTitle(pageView.ScatterplotTitle, Const.Loader.GetString("ChartTitleNvsN"), numerical1, numerical2);

            pageView.Scatterplot.LegendVisibility = Visibility.Collapsed;
            pageView.Scatterplot.HorizontalAxisTitle = numerical1.Name + numerical1.UnitString;
            pageView.Scatterplot.VerticalAxisTitle = numerical2.Name + numerical2.UnitString;
            pageView.Scatterplot.AutoColor = false;
            pageView.Scatterplot.Data = mainPageViewModel.SheetViewModel.FilteredRows
                .Select(r => new ScatterplotDatum()
                {
                    Key = 0,
                    Value1 = (Double)r.Cells[numerical1.Index].Content,
                    Value2 = (Double)r.Cells[numerical2.Index].Content,
                    Row = r,
                    State = ScatterplotState.Default,
                    ColumnViewModel = null
                })
                .ToList();
        }

        void DrawScatterplot(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            IsScatterplotVisible = true;

            pageView.ScatterplotTitle.Children.Clear();
            AddEditableTitle(pageView.ScatterplotTitle, Const.Loader.GetString("ChartTitleCNvsN"), categorical, numerical1, numerical2);
                        
            pageView.Scatterplot.LegendVisibility = Visibility.Visible;
            pageView.Scatterplot.HorizontalAxisTitle = numerical1.Name + numerical1.UnitString;
            pageView.Scatterplot.VerticalAxisTitle = numerical2.Name + numerical2.UnitString;
            pageView.Scatterplot.AutoColor = true;

            var data = mainPageViewModel.SheetViewModel.FilteredRows
                .Select(r => new ScatterplotDatum()
                {
                    Key = r.Cells[categorical.Index].Content,
                    Value1 = (Double)r.Cells[numerical1.Index].Content,
                    Value2 = (Double)r.Cells[numerical2.Index].Content,
                    Row = r,
                    State = ScatterplotState.Default,
                    ColumnViewModel = categorical
                });
                   
            if (data.Select(d => d.Key).Distinct().Count() > ScatterplotMaximumCategoryNumber) {
                IsScatterplotWarningVisible = true;
                var categories = data.Select(d => d.Key).Distinct().Take(ScatterplotMaximumCategoryNumber).ToList();
                data = data.Where(d => categories.IndexOf(d.Key) >= 0);
            }

            pageView.Scatterplot.Data = data.ToList();
        }        

        void SetScatterplotSelection(IEnumerable<Row> selectedRows)
        {
            if (selectedRows.Count() == 0)
            {
                foreach (ScatterplotDatum sd in pageView.Scatterplot.Data)
                {
                    sd.State = ScatterplotState.Default;
                }
            }
            else
            {
                foreach (ScatterplotDatum sd in pageView.Scatterplot.Data)
                {
                    sd.State = selectedRows.Contains(sd.Row) ? ScatterplotState.Selected : ScatterplotState.Unselected;
                }
            }
        }

        void DrawGroupedBarChartNN(ColumnViewModel numerical1, ColumnViewModel numerical2, List<GroupedRows> groupedRows)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            AddEditableTitle(pageView.GroupedBarChartTitle, Const.Loader.GetString("ChartTitleNN"), numerical1, numerical2);
            
            pageView.GroupedBarChart.YStartsFromZero = true;
            pageView.GroupedBarChart.HorizontalAxisTitle = "";
            pageView.GroupedBarChart.VerticalAxisTitle = $"{numerical1.UnitString}";

            Category category1 = new Category() { Value = numerical1.Name, Color = Category10.Colors[0], IsVirtual = true },
                     category2 = new Category() { Value = numerical2.Name, Color = Category10.Colors[1], IsVirtual = true }, 
                     dummyKey = new Category() { Value = $"{numerical1.Name} and {numerical2.Name} ", IsVirtual = true };

            GroupedBarChartDatum datum = new GroupedBarChartDatum()
            {
                ColumnViewModel = null,
                Key = dummyKey
            };

            datum.Children = new List<BarChartDatum>()
                            {
                                new BarChartDatum()
                                {
                                    ColumnViewModel = null,
                                    Key = category1,
                                    Parent = datum,
                                    Value = numerical1.AggregativeFunction.Aggregate(mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical1.Index].Content)),
                                    EnvelopeValue = numerical1.AggregativeFunction.Aggregate(mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical1.Index].Content)),
                                    EnvelopeRows = mainPageViewModel.SheetViewModel.FilteredRows,
                                    Rows = null
                                },
                                new BarChartDatum()
                                {
                                    ColumnViewModel = null,
                                    Key = category2,
                                    Parent = datum,
                                    Value = numerical1.AggregativeFunction.Aggregate(mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical2.Index].Content)),
                                    EnvelopeValue = numerical1.AggregativeFunction.Aggregate(mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical2.Index].Content)),
                                    EnvelopeRows = mainPageViewModel.SheetViewModel.FilteredRows,
                                    Rows = null
                                }
                            };

            pageView.GroupedBarChart.Data = new List<GroupedBarChartDatum>() { datum };
        }

        void SetGroupedBarChartNNSelection(ColumnViewModel numerical1, ColumnViewModel numerical2, IEnumerable<Row> selectedRows)
        {
            foreach(GroupedBarChartDatum datum in pageView.GroupedBarChart.Data)
            {
                if(selectedRows.Count() == 0)
                {
                    datum.Children[0].Rows = null;
                    datum.Children[0].Value = datum.Children[0].EnvelopeValue;

                    datum.Children[1].Rows = null;
                    datum.Children[1].Value = datum.Children[1].EnvelopeValue;
                }
                else
                {
                    datum.Children[0].Rows = datum.Children[0].EnvelopeRows.Intersect(selectedRows);
                    datum.Children[0].Value = numerical1.AggregativeFunction.Aggregate(datum.Children[0].Rows?.Select(r => (Double)r.Cells[numerical1.Index].Content));

                    datum.Children[1].Rows = datum.Children[1].EnvelopeRows.Intersect(selectedRows);
                    datum.Children[1].Value = numerical2.AggregativeFunction.Aggregate(datum.Children[1].Rows?.Select(r => (Double)r.Cells[numerical2.Index].Content));
                }
            }
        }
        

        void DrawGroupedBarChartCNN(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2, List<GroupedRows> groupedRows)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            AddEditableTitle(pageView.GroupedBarChartTitle, Const.Loader.GetString("ChartTitleCNN"), categorical, numerical1, numerical2);

            pageView.GroupedBarChart.YStartsFromZero = false;
            pageView.GroupedBarChart.HorizontalAxisTitle = categorical.Name;
            pageView.GroupedBarChart.VerticalAxisTitle = $"{numerical1.Name} {Const.Loader.GetString("And")} {numerical2.Name} {numerical1.UnitString}";

            Category category1 = new Category() { Value = numerical1.Name, Color = Category10.Colors[0], IsVirtual = true },
                     category2 = new Category() { Value = numerical2.Name, Color = Category10.Colors[1], IsVirtual = true };

            var data = groupedRows
                        .Select(g =>
                        {
                            GroupedBarChartDatum datum = new GroupedBarChartDatum()
                            {
                                ColumnViewModel = categorical,
                                Key = g.Keys[categorical]
                            };

                            datum.Children = new List<BarChartDatum>()
                            {
                                new BarChartDatum()
                                {
                                    ColumnViewModel = null,
                                    Key = category1,
                                    Parent = datum,
                                    Value = numerical1.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical1.Index].Content)),
                                    EnvelopeValue = numerical1.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical1.Index].Content)),
                                    Rows = null,
                                    EnvelopeRows = g.Rows
                                },
                                new BarChartDatum()
                                {
                                    ColumnViewModel = null,
                                    Key = category2,
                                    Parent = datum,
                                    Value = numerical2.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical2.Index].Content)),
                                    EnvelopeValue = numerical2.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical2.Index].Content)),
                                    Rows = null,
                                    EnvelopeRows = g.Rows
                                }
                            };
                            return datum;
                        });

            if (data.Count() > GroupedBarChartMaximumRecordNumber) IsGroupedBarChartWarningVisible = true;
            pageView.GroupedBarChart.Data = data.Take(GroupedBarChartMaximumRecordNumber).ToList();
        }

        void SetGroupedBarChartCNNSelection(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2, IEnumerable<Row> selectedRows)
        {
            foreach(GroupedBarChartDatum datum in pageView.GroupedBarChart.Data)
            {
                if(selectedRows.Count() == 0)
                {
                    datum.Children[0].Rows = null;
                    datum.Children[0].Value = datum.Children[0].EnvelopeValue;

                    datum.Children[1].Rows = null;
                    datum.Children[1].Value = datum.Children[1].EnvelopeValue;
                }
                else
                {
                    datum.Children[0].Rows = datum.Children[0].EnvelopeRows.Intersect(selectedRows).ToList();
                    datum.Children[0].Value = numerical1.AggregativeFunction.Aggregate(datum.Children[0].Rows.Select(r => (Double)r.Cells[numerical1.Index].Content));

                    datum.Children[1].Rows = datum.Children[1].EnvelopeRows.Intersect(selectedRows).ToList();
                    datum.Children[1].Value = numerical2.AggregativeFunction.Aggregate(datum.Children[1].Rows.Select(r => (Double)r.Cells[numerical2.Index].Content));
                }
            }
        }

        void DrawLineChartCNN(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2, List<GroupedRows> groupedRows)
        {
            IsLineChartVisible = true;

            pageView.LineChartTitle.Children.Clear();
            AddEditableTitle(pageView.LineChartTitle, Const.Loader.GetString("ChartTitleCNN"), categorical, numerical1, numerical2);

            pageView.LineChart.YStartsFromZero = false;
            pageView.LineChart.HorizontalAxisTitle = categorical.Name;
            pageView.LineChart.VerticalAxisTitle = $"{numerical1.Name} {Const.Loader.GetString("And")} {numerical2.Name} {numerical1.UnitString}";
           
            
            Int32 index = 0;
            List<LineChartDatum> data = new List<LineChartDatum>();

            foreach (ColumnViewModel numerical in new List<ColumnViewModel>() { numerical1, numerical2 })
            {
                Category category = new Category() { Value = numerical.Name, Color = Category10.Colors[index], IsVirtual = true };
                LineChartDatum datum = new LineChartDatum()
                {
                    ColumnViewModel = numerical,
                    Key = category
                };

                datum.DataPoints = groupedRows.Select(g => new DataPoint()
                {
                    Item1 = g.Keys[categorical],
                    Item2 = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                    EnvelopeItem2 = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                    Parent = datum,
                    Rows = null,
                    EnvelopeRows = g.Rows
                }).ToList();

                data.Add(datum);
                index++;
            }

            if (data.Count() > LineChartMaximumSeriesNumber) IsLineChartWarningVisible = true;
            if (data.Max(row => row.DataPoints.Count) > LineChartMaximumPointNumberInASeries) IsLineChartWarningVisible = true;

            data = data.Take(LineChartMaximumSeriesNumber).ToList();
            pageView.LineChart.Data = data;
        }

        void SetLineChartCNNSelection(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2, IEnumerable<Row> selectedRows)
        {
            Int32 index = 0;

            if (selectedRows.Count() == 0)
            {
                foreach (ColumnViewModel numerical in new List<ColumnViewModel>() { numerical1, numerical2 })
                {
                    foreach (DataPoint dp in pageView.LineChart.Data[index].DataPoints)
                    {
                        dp.Item2 = dp.EnvelopeItem2;
                        dp.Rows = null;
                    }
                    index++;
                }
            }
            else
            {
                foreach (ColumnViewModel numerical in new List<ColumnViewModel>() { numerical1, numerical2 })
                {
                    foreach (DataPoint dp in pageView.LineChart.Data[index].DataPoints)
                    {
                        dp.Rows = dp.EnvelopeRows.Intersect(selectedRows).ToList();
                        dp.Item2 = numerical.AggregativeFunction.Aggregate(dp.Rows.Select(r => (Double)r.Cells[numerical.Index].Content));
                    }
                    index++;
                }
            }
        }

        void DrawCorrelatonStatistics(ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            CorrelationStatisticsViewModel result = CorrelationStatistics.Analyze(
                mainPageViewModel,
                numerical1.Name,
                numerical2.Name,
                mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical1.Index].Content),
                mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical2.Index].Content)
                );

            pageView.CorrelationStatisticsTitle.Children.Clear();
            AddEditableTitle(pageView.CorrelationStatisticsTitle, Const.Loader.GetString("CorrelationalStatisticsTitle"), numerical1, numerical2);

            IsCorrelationStatisticsVisible = true;
            pageView.CorrelationStatisticsView.DataContext = result;
        }
        #endregion
    }
}
