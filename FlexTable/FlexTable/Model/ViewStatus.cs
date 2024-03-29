﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3.ColorScheme;
using d3.Scale;
using FlexTable.Util;
using FlexTable.View;
using FlexTable.ViewModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace FlexTable.Model
{
    public class ViewStatus
    {
        public static readonly Color DefaultCellForegroundColor = Colors.Black;
        public static readonly SolidColorBrush DefaultRowHeaderSolidColorBrush = new SolidColorBrush(Colors.Black);

        public static readonly Color Category10FirstColor = Category10.Colors[0];
        public static readonly Color Category10SecondColor = Category10.Colors[1];
        public static readonly Color Category10ThirdColor = Category10.Colors[2];

        public static readonly SolidColorBrush Category10FirstSolidColorBrush = new SolidColorBrush(Category10.Colors[0]);
        public static readonly SolidColorBrush Category10SecondSolidColorBrush = new SolidColorBrush(Category10.Colors[1]);
        public static readonly SolidColorBrush Category10ThirdSolidColorBrush = new SolidColorBrush(Category10.Colors[2]);

        private List<ColumnViewModel> selectedColumnViewModels = new List<ColumnViewModel>();
        public List<ColumnViewModel> SelectedColumnViewModels => selectedColumnViewModels;

        private IEnumerable<ColumnViewModel> numericalColumnViewModels;
        public IEnumerable<ColumnViewModel> NumericalColumnViewModels => numericalColumnViewModels;
        private IEnumerable<ColumnViewModel> categoricalColumnViewModels;
        public IEnumerable<ColumnViewModel> CategoricalColumnViewModels => categoricalColumnViewModels;

        private Int32 selectedCount;
        public Int32 SelectedCount => selectedCount;
        private Int32 numericalCount;
        public Int32 NumericalCount => numericalCount;
        private Int32 categoricalCount;
        public Int32 CategoricalCount => categoricalCount;

        public Boolean IsEmpty => SelectedCount == 0;

        public Boolean IsC => SelectedCount == 1 && CategoricalCount == 1;
        public Boolean IsN => SelectedCount == 1 && NumericalCount == 1;

        public Boolean IsCC => SelectedCount == 2 && CategoricalCount == 2;
        public Boolean IsCN => SelectedCount == 2 && CategoricalCount == 1 && NumericalCount == 1;
        public Boolean IsNN => SelectedCount == 2 && NumericalCount == 2;

        public Boolean IsCCC => SelectedCount == 3 && CategoricalCount == 3;
        public Boolean IsCCN => SelectedCount == 3 && CategoricalCount == 2 && NumericalCount == 1;
        public Boolean IsCNN => SelectedCount == 3 && CategoricalCount == 1 && NumericalCount == 2;
        public Boolean IsNNN => SelectedCount == 3 && NumericalCount == 3;

        public Boolean IsCnN0 => NumericalCount == 0;
        public Boolean IsCnN1 => NumericalCount == 1;
        public Boolean IsCnNn => NumericalCount >= 1;

        public Boolean IsCn => CategoricalCount >= 1;
        public Boolean IsOnlyCn => CategoricalCount >= 1 && NumericalCount == 0;

        private ColumnViewModel firstColumn, lastColumn, firstCategorical, secondCategorical, thirdCategorical, lastCategorical;
        private ColumnViewModel firstNumerical, secondNumerical, thirdNumerical;

        public ColumnViewModel FirstColumn => firstColumn;
        public ColumnViewModel LastColumn => lastCategorical;
        public ColumnViewModel FirstCategorical => firstCategorical;
        public ColumnViewModel SecondCategorical => secondCategorical;
        public ColumnViewModel ThirdCategorical => thirdCategorical;
        public ColumnViewModel LastCategorical => lastCategorical;
        public ColumnViewModel FirstNumerical => firstNumerical;
        public ColumnViewModel SecondNumerical => secondNumerical;
        public ColumnViewModel ThirdNumerical => thirdNumerical;

        public UIElement ActivatedChart { get; set; }
        public Boolean IsScatterplotVisible => ActivatedChart is Crayon.Chart.Scatterplot;
        public Boolean IsLineChartVisible => ActivatedChart is Crayon.Chart.LineChart;
        public Boolean IsBarChartVisible => ActivatedChart is Crayon.Chart.BarChart;
        public Boolean IsGroupedBarChartVisible => ActivatedChart is Crayon.Chart.GroupedBarChart;
        public Boolean IsPivotTableVisible => ActivatedChart is PivotTableView;
        public Boolean IsCorrelationStatisticsVisible => ActivatedChart is CorrelationStatisticsView;
        public Boolean IsDescriptiveStatisticsVisible => ActivatedChart is DescriptiveStatisticsView;
        public Boolean IsDistributionViewVisible => ActivatedChart is DistributionView;

        public Boolean IsAllRowViewModelVisible => IsEmpty || (IsNN && IsScatterplotVisible) || (IsCNN && IsScatterplotVisible);

        public List<RowViewModel> GroupedRowViewModels { get; set; }
        public List<GroupedRows> GroupedRows { get; set; }
        public TableViewModel.TableViewState TableViewState { get; set; }
        //public AnimationHint AnimationHint { get; set; }
        public IEnumerable<Row> SelectedRows { get; set; }

        public ViewStatus()
        {
            Refresh();
        }

        public void Refresh()
        {
            numericalColumnViewModels = selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical);
            categoricalColumnViewModels = selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical);
            selectedCount = selectedColumnViewModels.Count();
            numericalCount = numericalColumnViewModels.Count();
            categoricalCount = categoricalColumnViewModels.Count();

            firstColumn = selectedCount > 0 ? selectedColumnViewModels.First() : null;
            lastColumn = selectedCount > 0 ? selectedColumnViewModels.Last() : null;
            firstCategorical = categoricalCount > 0 ? selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).First() : null;
            secondCategorical = categoricalCount > 1 ? selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ElementAt(1) : null;
            thirdCategorical = categoricalCount > 2 ? selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ElementAt(2) : null;
            lastCategorical = CategoricalCount > 0 ? selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Last() : null;

            firstNumerical = NumericalCount > 0 ? selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).First() : null;
            secondNumerical = NumericalCount > 1 ? selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).ElementAt(1) : null;
            thirdNumerical = NumericalCount > 2 ? selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).ElementAt(2) : null;
        }

        public ViewStatus Clone()
        {
            ViewStatus cloned = new ViewStatus()
            {
                GroupedRowViewModels = GroupedRowViewModels != null ? new List<RowViewModel>(GroupedRowViewModels) : null,
                GroupedRows = GroupedRows != null ? new List<GroupedRows>(GroupedRows) : null,
                ActivatedChart = ActivatedChart
            };

            foreach(ColumnViewModel cvm in selectedColumnViewModels)
            {
                cloned.SelectedColumnViewModels.Add(cvm);
            }
            cloned.Refresh();
            return cloned;
        }

        public List<GroupedRows> Bin(ColumnViewModel selected, List<Row> rows)
        {
            Linear linear = new Linear()
            {
                DomainStart = rows.Select(r => (Double)r.Cells[selected.Index].Content).Min(),
                DomainEnd = rows.Select(r => (Double)r.Cells[selected.Index].Content).Max(),
            };

            linear.Nice();

            IEnumerable<Bin> bins = HistogramCalculator.Bin(
                linear.DomainStart,
                linear.DomainEnd,
                linear.Step,
                rows,
                selected
                );

            List<GroupedRows> groupedRows = new List<GroupedRows>();
            foreach (Bin bin in bins)
            {
                GroupedRows grs = new GroupedRows();
                grs.Keys[selected] = bin;
                grs.Rows = bin.Rows.ToList();
                groupedRows.Add(grs);
            }

            return groupedRows;
        }

        /// <summary>
        /// 현재 viewStatus와 sheetViewModel을 이용하여 grouepdRows와 groupedRowViewModels를 생성함
        /// 문제는 columnViewModel의 
        /// </summary>
        /// <param name="sheetViewModel"></param>
        public void Generate(SheetViewModel sheetViewModel)
        {
            List<ColumnViewModel> orderedColumnViewModels = sheetViewModel.ColumnViewModels; 

            Int32 index = 0;

            if (IsEmpty)
            {
                // 어차피 allRow가 보일 것이므로 RowViewModel 을 만들어 줄 필요는 없음 
            }
            else if (IsN) // 이 경우는 뉴메리컬 하나만 선택되어 비닝 된 결과가 보이는 경우이다.
            {
                ColumnViewModel selected = FirstColumn;
                GroupedRows = Bin(selected, sheetViewModel.FilteredRows);
                GroupedRowViewModels = new List<RowViewModel>();

                // 여기서 groupedRows가 소팅되어야함 
                // 그런데 여기서는 선택되지 않은 컬럼의 경우에는 어차피 어그리게이션되므로 소팅 순서가 의미가 없음 따라서 선택된 컬럼에 대해서만 소팅하면 된다

                GroupedRows.Sort(new GroupedRowComparer(sheetViewModel, this));

                foreach (GroupedRows groupedRows in GroupedRows)
                {
                    if (groupedRows.Rows.Count == 0) continue;
                    RowViewModel rowViewModel = new RowViewModel(sheetViewModel.MainPageViewModel)
                    {
                        Index = index++,
                        Rows = groupedRows.Rows
                    };

                    foreach (ColumnViewModel columnViewModel in orderedColumnViewModels)
                    {
                        Cell cell = new Cell();

                        cell.ColumnViewModel = columnViewModel;

                        if (columnViewModel == selected)
                        {
                            Bin bin = groupedRows.Keys[selected] as Bin;

                            String content = bin.ToString() + $" ({groupedRows.Rows.Count})";
                            cell.RawContent = content;
                            cell.Content = content;
                        }
                        else if (columnViewModel.Type == ColumnType.Categorical)
                        {
                            Int32 uniqueCount = groupedRows.Rows.Select(r => r.Cells[columnViewModel.Index].Content).Distinct().Count();
                            cell.Content = $"({uniqueCount})";
                            cell.RawContent = $"({uniqueCount})";
                        }
                        else //numerical
                        {
                            Object aggregated = columnViewModel.AggregativeFunction.Aggregate(groupedRows.Rows.Select(r => (Double)r.Cells[columnViewModel.Index].Content));
                            String formatted = Formatter.FormatAuto4((Double)aggregated);
                            cell.RawContent = formatted;
                            cell.Content = Double.Parse(formatted);
                        }

                        rowViewModel.Cells.Add(cell);
                    }

                    GroupedRowViewModels.Add(rowViewModel);
                }
            }
            else if (IsNN)
            {
                GroupedRowViewModels = new List<RowViewModel>();

                RowViewModel rowViewModel = new RowViewModel(sheetViewModel.MainPageViewModel)
                {
                    Index = 0,
                    Rows = sheetViewModel.FilteredRows
                };

                foreach (ColumnViewModel columnViewModel in orderedColumnViewModels)
                {
                    Cell cell = new Cell();

                    cell.ColumnViewModel = columnViewModel;

                    if (columnViewModel.Type == ColumnType.Categorical)
                    {
                        Int32 uniqueCount = sheetViewModel.FilteredRows.Select(r => r.Cells[columnViewModel.Index].Content).Distinct().Count();
                        cell.Content = $"({uniqueCount})";
                        cell.RawContent = $"({uniqueCount})";
                    }
                    else if (columnViewModel.Type == ColumnType.Numerical)
                    {
                        Object aggregated = columnViewModel.AggregativeFunction.Aggregate(sheetViewModel.FilteredRows.Select(r => (Double)r.Cells[columnViewModel.Index].Content));
                        String formatted = Formatter.FormatAuto4((Double)aggregated);
                        cell.RawContent = formatted;
                        cell.Content = Double.Parse(formatted);
                    }

                    rowViewModel.Cells.Add(cell);
                }

                GroupedRowViewModels.Add(rowViewModel);
            }
            else if(CategoricalCount > 0)// 이 경우는 categorical이든 datetime이든 뭔가로 그룹핑이 된 경우 
            {
                GroupedRows = GroupRecursive(
                    sheetViewModel.FilteredRows, 
                    CategoricalColumnViewModels.ToList(), 
                    0);

                GroupedRows.Sort(new GroupedRowComparer(sheetViewModel, this));
                GroupedRowViewModels = new List<RowViewModel>();

                foreach (GroupedRows groupedRows in GroupedRows)
                {
                    RowViewModel rowViewModel = new RowViewModel(sheetViewModel.MainPageViewModel)
                    {
                        Index = index++,
                        Rows = groupedRows.Rows
                    };

                    foreach (ColumnViewModel columnViewModel in orderedColumnViewModels)
                    {
                        Cell cell = new Cell();

                        cell.ColumnViewModel = columnViewModel;

                        if (groupedRows.Keys.ContainsKey(columnViewModel))
                        {
                            Object content = groupedRows.Keys[columnViewModel];
                            cell.Content = content;
                            cell.RawContent = cell.Content.ToString();
                        }
                        else if (columnViewModel.Type == ColumnType.Categorical)
                        {
                            Int32 uniqueCount = groupedRows.Rows.Select(r => r.Cells[columnViewModel.Index].Content).Distinct().Count();
                            cell.Content = $"({uniqueCount})";
                            cell.RawContent = $"({uniqueCount})";
                        }
                        else if (columnViewModel.Type == ColumnType.Numerical)
                        {
                            Object aggregated = columnViewModel.AggregativeFunction.Aggregate(groupedRows.Rows.Select(r => (Double)r.Cells[columnViewModel.Index].Content));
                            String formatted = Formatter.FormatAuto4((Double)aggregated);
                            cell.RawContent = formatted;
                            cell.Content = Double.Parse(formatted);
                        }

                        rowViewModel.Cells.Add(cell);
                    }

                    GroupedRowViewModels.Add(rowViewModel);
                }
            }
            else
            {
                ;
            }
        }

        public static List<GroupedRows> GroupRecursive(List<Row> rows, List<ColumnViewModel> groupedColumnViewModels, Int32 pivotIndex)
        {
            ColumnViewModel pivot = groupedColumnViewModels[pivotIndex];
            Dictionary<Object, List<Row>> dict = GetRowsByColumnViewModel(rows, pivot);

            if (pivotIndex < groupedColumnViewModels.Count - 1) // 그루핑을 더 해야함.
            {
                List<GroupedRows> groupedRowsList = new List<GroupedRows>();
                foreach (KeyValuePair<Object, List<Row>> kv in dict)
                {
                    List<GroupedRows> ret = GroupRecursive(kv.Value, groupedColumnViewModels, pivotIndex + 1);

                    foreach (GroupedRows groupedRows in ret)
                    {
                        groupedRows.Keys[pivot] = kv.Key;
                        groupedRowsList.Add(groupedRows);
                    }
                }

                return groupedRowsList;
            }
            else // 마지막임
            {
                List<GroupedRows> groupedRowsList = new List<GroupedRows>();
                foreach (KeyValuePair<Object, List<Row>> kv in dict)
                {
                    GroupedRows groupedRows = new GroupedRows();
                    groupedRows.Keys[pivot] = kv.Key;
                    groupedRows.Rows = kv.Value;

                    groupedRowsList.Add(groupedRows);
                }

                return groupedRowsList;
            }
        }

        public static Dictionary<Object, List<Row>> GetRowsByColumnViewModel(IEnumerable<Row> rows, ColumnViewModel columnViewModel)
        {
            Dictionary<Object, List<Row>> dict = new Dictionary<Object, List<Row>>();

            foreach (Row row in rows)
            {
                Object content = row.Cells[columnViewModel.Index].Content;
                if (!dict.ContainsKey(content))
                {
                    dict[content] = new List<Row>();
                }

                dict[content].Add(row);
            }

            return dict;
        }

        public ColumnViewModel GetColoredColumnViewModel()
        {
            Int32 categoricalCount = this.CategoricalCount;

            if (1 <= categoricalCount && categoricalCount <= 2) return CategoricalColumnViewModels.Last();
            return null;
        }

        public ColumnViewModel GetFirstColoredColumnViewModel()
        {
            if (NumericalCount >= 1) return FirstNumerical;
            return null;
        }

        public ColumnViewModel GetSecondColoredColumnViewModel()
        {
            if (NumericalCount >= 2) return SecondNumerical;
            return null;
        }

        public ColumnViewModel GetThirdColoredColumnViewModel()
        {
            if (NumericalCount >= 3) return ThirdNumerical;
            return null;
        }

        /// <summary>
        /// This function colors rowViewModels according to the current viewStatus. Only called when a column is just seledted or unselected.
        /// This function is not called when a column is being previewed.
        /// </summary>
        /// <param name="viewStatus"></param>
        /// <param name="allRowViewModels"></param>
        /// <param name="groupedRowViewModels"></param>
        public void ColorRowViewModels(
            List<RowViewModel> allRowViewModels,
            List<RowViewModel> groupedRowViewModels,
            List<GroupedRows> groupedRows
            )
        {
            Int32 index = 0;
            Dictionary<Row, Color> dictionary = new Dictionary<Row, Color>();

            if (IsEmpty)
            {
                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = DefaultCellForegroundColor;
            }        
            else if (IsC)
            {
                index = 0;
                ColumnViewModel categorical = FirstCategorical;
                foreach (GroupedRows grs in groupedRows)
                {
                    foreach (Row row in grs.Rows)
                    {
                        dictionary[row] = (row.Cells[categorical.Index].Content as Category).Color;
                    }
                    if(index < groupedRowViewModels.Count)
                        groupedRowViewModels[index].Color = (grs.Keys[categorical] as Category).Color;
                    index++;
                }

                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = dictionary.ContainsKey(rowViewModel.Row) ? dictionary[rowViewModel.Row] : DefaultCellForegroundColor;
            }
            else if (IsN)
            {
                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = Category10FirstColor;
                foreach (RowViewModel rowViewModel in groupedRowViewModels) rowViewModel.Color = Category10FirstColor;
            }
            else if (IsCC)
            {
                index = 0;
                ColumnViewModel categorical = SecondCategorical;
                foreach (GroupedRows grs in groupedRows)
                {
                    foreach (Row row in grs.Rows)
                    {
                        dictionary[row] = (row.Cells[categorical.Index].Content as Category).Color;
                    }
                    if (index < groupedRowViewModels.Count)
                        groupedRowViewModels[index].Color = (grs.Keys[categorical] as Category).Color;
                    index++;
                }

                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = dictionary.ContainsKey(rowViewModel.Row) ? dictionary[rowViewModel.Row] : DefaultCellForegroundColor;
            }
            else if (IsCN)
            {
                if (IsLineChartVisible)
                {
                    foreach (RowViewModel rowViewModel in groupedRowViewModels) rowViewModel.Color = Category10FirstColor;
                    foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = Category10FirstColor;
                }
                else
                {
                    index = 0;
                    ColumnViewModel categorical = FirstCategorical;
                    foreach (GroupedRows grs in groupedRows)
                    {
                        foreach (Row row in grs.Rows)
                        {
                            dictionary[row] = (row.Cells[categorical.Index].Content as Category).Color;
                        }
                        if (index < groupedRowViewModels.Count)
                            groupedRowViewModels[index].Color = (grs.Keys[categorical] as Category).Color;
                        index++;
                    }

                    foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = dictionary.ContainsKey(rowViewModel.Row) ? dictionary[rowViewModel.Row] : DefaultCellForegroundColor;
                }
            }
            else if (IsNN)
            {
                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = Category10FirstColor;
                foreach (RowViewModel rowViewModel in groupedRowViewModels) rowViewModel.Color = DefaultCellForegroundColor;
            }
            else if (IsCCN)
            {
                index = 0;
                ColumnViewModel categorical = SecondCategorical;
                foreach (GroupedRows grs in groupedRows)
                {
                    foreach (Row row in grs.Rows)
                    {
                        dictionary[row] = (row.Cells[categorical.Index].Content as Category).Color;
                    }
                    if (index < groupedRowViewModels.Count)
                        groupedRowViewModels[index].Color = (grs.Keys[categorical] as Category).Color;
                    index++;
                }

                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = dictionary.ContainsKey(rowViewModel.Row) ? dictionary[rowViewModel.Row] : DefaultCellForegroundColor;
            }
            else if (IsCNN)
            {
                // grouped by 는 색깔 안넣는걸로 
                ColumnViewModel categorical = LastCategorical;
                foreach (RowViewModel rowViewModel in groupedRowViewModels) rowViewModel.Color = (rowViewModel.Cells[categorical.Index].Content as Category).Color;

                // all row는 categorical 별로 색깔
                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = (rowViewModel.Row.Cells[categorical.Index].Content as Category).Color;
            }
        }
    }

    public class RowViewModelComparer : IComparer<RowViewModel>
    {
        ViewStatus ViewStatus;
        SheetViewModel SheetViewModel;
        IEnumerable<Row> FilteredRows;
        IEnumerable<Row> SelectedRows;

        Int32 selectedCount;
        IEnumerable<ColumnViewModel> sortedColumnViewModels;
        ColumnViewModel firstNumerical;
        IEnumerable<ColumnViewModel> sortOrder;
        Boolean isN;

        Dictionary<Row, Boolean> filteredRowDictionary = new Dictionary<Row, bool>();

        public RowViewModelComparer(SheetViewModel sheetViewModel, ViewStatus viewStatus, IEnumerable<Row> selectedRows)
        {
            SheetViewModel = sheetViewModel;
            ViewStatus = viewStatus;
            FilteredRows = sheetViewModel.FilteredRows;
            SelectedRows = selectedRows;

            selectedCount = ViewStatus.SelectedCount;
            sortedColumnViewModels = SheetViewModel.ColumnViewModels.Where(
                    cvm => cvm.SortOption != SortOption.None
                ).OrderByDescending(cvm => cvm.SortPriority);
            firstNumerical = ViewStatus.FirstNumerical;

            IEnumerable<ColumnViewModel> nonTailCategoricalColumnViewModels = ViewStatus.SelectedColumnViewModels.Where(
                cvm => cvm.Type == ColumnType.Categorical && cvm != ViewStatus.LastCategorical
            );

            IEnumerable<ColumnViewModel> tailCategoricalOrNumericalColumnViewModels = ViewStatus.SelectedColumnViewModels.Where(
                cvm => cvm.Type == ColumnType.Numerical || cvm == ViewStatus.LastCategorical
            ).OrderByDescending(cvm =>
                cvm.SortOption != SortOption.None ? cvm.SortPriority : (cvm.Type == ColumnType.Categorical ? 0 : -1)
            );

            sortOrder = nonTailCategoricalColumnViewModels.Concat(tailCategoricalOrNumericalColumnViewModels);
            isN = viewStatus.IsN;

            foreach(Row row in FilteredRows)
            {
                filteredRowDictionary[row] = true;
            }
        }

        Int32 GetSortDirection(ColumnViewModel cvm) => cvm.SortOption == SortOption.Descending ? -1 : 1;

        public int Compare(RowViewModel x, RowViewModel y)
        {
            if (filteredRowDictionary.ContainsKey(x.Row) != filteredRowDictionary.ContainsKey(y.Row))
            {
                if (filteredRowDictionary.ContainsKey(x.Row)) return -1;
                return 1;
            }

            if (SelectedRows != null && SelectedRows.Contains(x.Row) != SelectedRows.Contains(y.Row))
            {
                if (SelectedRows.Contains(x.Row)) return -1;
                return 1;
            }

            if (selectedCount == 0)
            {
                foreach (ColumnViewModel columnViewModel in sortedColumnViewModels)
                {
                    if (columnViewModel.Type == ColumnType.Numerical)
                    {
                        if ((Double)x.Cells[columnViewModel.Index].Content != (Double)y.Cells[columnViewModel.Index].Content)
                        {
                            return GetSortDirection(columnViewModel) *
                                ((Double)x.Cells[columnViewModel.Index].Content).CompareTo((Double)y.Cells[columnViewModel.Index].Content);
                        }
                    }
                    else
                    {
                        if (x.Cells[columnViewModel.Index].Content != y.Cells[columnViewModel.Index].Content)
                        {
                            return GetSortDirection(columnViewModel) *
                        (x.Cells[columnViewModel.Index].Content as Category).Order.CompareTo((y.Cells[columnViewModel.Index].Content as Category).Order);
                        }
                    }
                }

                return x.Row.Index - y.Row.Index;
            }

            // 하나라도 선택된 컬럼이 있다.

            if (isN)
            {
                Int32 index = firstNumerical.Index;
                return ((Double)x.Cells[index].Content).CompareTo((Double)y.Cells[index].Content) * firstNumerical.SortDirection;
            }
            
            foreach (ColumnViewModel columnViewModel in sortOrder)
            {
                if (columnViewModel.Type == ColumnType.Numerical)
                {
                    if ((Double)x.Cells[columnViewModel.Index].Content != (Double)y.Cells[columnViewModel.Index].Content)
                    {
                        return GetSortDirection(columnViewModel) *
                            ((Double)x.Cells[columnViewModel.Index].Content).CompareTo((Double)y.Cells[columnViewModel.Index].Content);
                    }
                }
                else
                {
                    if (x.Cells[columnViewModel.Index].Content != y.Cells[columnViewModel.Index].Content)
                    {
                        return GetSortDirection(columnViewModel) *
                    (x.Cells[columnViewModel.Index].Content as Category).Order.CompareTo((y.Cells[columnViewModel.Index].Content as Category).Order);
                    }
                }
            }

            return x.Row.Index - y.Row.Index;
        }
    }

    public class GroupedRowComparer : IComparer<GroupedRows>
    {
        ViewStatus ViewStatus;
        SheetViewModel SheetViewModel;

        public GroupedRowComparer(SheetViewModel sheetViewModel, ViewStatus viewStatus)
        {
            SheetViewModel = sheetViewModel;
            ViewStatus = viewStatus;
        }

        Int32 GetSortDirection(ColumnViewModel cvm) => cvm.SortOption == SortOption.Descending ? -1 : 1;

        public int Compare(GroupedRows x, GroupedRows y)
        {
            if (ViewStatus.SelectedCount == 0)
            {
                throw new Exception("No column selected, but sort applied to grouped rows");
            }

            // 하나라도 선택된 컬럼이 있다.

            if (ViewStatus.IsN)
            {
                ColumnViewModel numerical = ViewStatus.FirstNumerical;

                if (x.Keys.ContainsKey(numerical))
                {
                    Double xMin = (x.Keys[numerical] as Bin).Min,
                        yMin = (y.Keys[numerical] as Bin).Min;

                    if (xMin != yMin)
                        return xMin.CompareTo(yMin) * GetSortDirection(numerical);
                }
            }

            IEnumerable<ColumnViewModel> nonTailCategoricalColumnViewModels = ViewStatus.SelectedColumnViewModels.Where(
                cvm => cvm.Type == ColumnType.Categorical && cvm != ViewStatus.LastCategorical
            );

            IEnumerable<ColumnViewModel> tailCategoricalOrNumericalColumnViewModels = ViewStatus.SelectedColumnViewModels.Where(
                cvm => cvm.Type == ColumnType.Numerical || cvm == ViewStatus.LastCategorical
            ).OrderByDescending(cvm =>
                cvm.SortOption != SortOption.None ? cvm.SortPriority : (cvm.Type == ColumnType.Categorical ? 0 : -1)
            );

            IEnumerable<ColumnViewModel> sortOrder = nonTailCategoricalColumnViewModels.Concat(tailCategoricalOrNumericalColumnViewModels);

            foreach (ColumnViewModel columnViewModel in sortOrder)
            {
                if (x.Keys.ContainsKey(columnViewModel)) // 페이지가 여러개면 제일 상위 페이지 선택 컬럼을 기준으로 소팅하므로 이전에는 그 컬럼이 선택되지 않았을 수도 있음.
                {
                    if (columnViewModel.Type == ColumnType.Categorical)
                    {
                        if (x.Keys[columnViewModel] != y.Keys[columnViewModel])
                        {
                            return (x.Keys[columnViewModel] as Category).Order.CompareTo((y.Keys[columnViewModel] as Category).Order) * GetSortDirection(columnViewModel);
                        }
                    }
                }
                else if (columnViewModel.Type == ColumnType.Numerical)
                {
                    Double xValue = columnViewModel.AggregativeFunction.Aggregate(x.Rows.Select(r => (Double)r.Cells[columnViewModel.Index].Content));
                    Double yValue = columnViewModel.AggregativeFunction.Aggregate(y.Rows.Select(r => (Double)r.Cells[columnViewModel.Index].Content));

                    if (xValue != yValue)
                    {
                        return xValue.CompareTo(yValue) * GetSortDirection(columnViewModel);
                    }
                }
            }

            return 0;
        }
    }
}
