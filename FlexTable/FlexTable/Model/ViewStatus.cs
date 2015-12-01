﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3.Scale;
using FlexTable.Util;
using FlexTable.ViewModel;
using Windows.UI.Xaml;

namespace FlexTable.Model
{
    public class ViewStatus
    {
        private List<ColumnViewModel> selectedColumnViewModels = new List<ColumnViewModel>();
        public List<ColumnViewModel> SelectedColumnViewModels => selectedColumnViewModels;
        public IEnumerable<ColumnViewModel> NumericalColumnViewModels => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical);
        public IEnumerable<ColumnViewModel> CategoricalColumnViewModels => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical);

        private Int32 totalCount => selectedColumnViewModels.Count;
        private Int32 numericalCount => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Count();
        private Int32 categoricalCount => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Count();
         

        public Boolean IsEmpty => totalCount == 0;

        public Boolean IsC => totalCount == 1 && categoricalCount == 1;
        public Boolean IsN => totalCount == 1 && numericalCount == 1;

        public Boolean IsCC => totalCount == 2 && categoricalCount == 2;
        public Boolean IsCN => totalCount == 2 && categoricalCount == 1 && numericalCount == 1;
        public Boolean IsNN => totalCount == 2 && numericalCount == 2;

        public Boolean IsCCC => totalCount == 3 && categoricalCount == 3;
        public Boolean IsCCN => totalCount == 3 && categoricalCount == 2 && numericalCount == 1;
        public Boolean IsCNN => totalCount == 3 && categoricalCount == 1 && numericalCount == 2;
        public Boolean IsNNN => totalCount == 3 && numericalCount == 3;

        public Boolean IsCnN0 => numericalCount == 0;
        public Boolean IsCnN1 => numericalCount == 1;
        public Boolean IsCnNn => numericalCount >= 1;

        public ColumnViewModel FirstColumn => selectedColumnViewModels.First();
        public ColumnViewModel FirstCategorical => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).First();
        public ColumnViewModel SecondCategorical => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ElementAt(1);
        public ColumnViewModel FirstNumerical => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).First();
        public ColumnViewModel SecondNumerical => selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).ElementAt(1);

        public UIElement ActivatedChart { get; set; }
        public Boolean IsScatterplotVisible => ActivatedChart is Crayon.Chart.Scatterplot;
        public Boolean IsLineChartVisible => ActivatedChart is Crayon.Chart.LineChart;

        public List<RowViewModel> GroupedRowViewModels { get; set; }
        public List<GroupedRows> GroupedRows { get; set; }

        public ViewStatus Clone()
        {
            ViewStatus cloned = new ViewStatus();
            foreach(ColumnViewModel cvm in selectedColumnViewModels)
            {
                cloned.SelectedColumnViewModels.Add(cvm);
            }
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
                GroupedRows = Bin(selected, sheetViewModel.FilteredRows.ToList());
                GroupedRowViewModels = new List<RowViewModel>();

                // 여기서 groupedRows가 소팅되어야함 
                // 그런데 여기서는 선택되지 않은 컬럼의 경우에는 어차피 어그리게이션되므로 소팅 순서가 의미가 없음 따라서 선택된 컬럼에 대해서만 소팅하면 된다

                GroupedRows.Sort(new GroupedRowComparer(sheetViewModel, this));

                foreach (GroupedRows groupedRows in GroupedRows)
                {
                    if (groupedRows.Rows.Count == 0) continue;
                    RowViewModel rowViewModel = new RowViewModel(sheetViewModel.MainPageViewModel)
                    {
                        Index = index++
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
                    Index = 0
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
            else if(categoricalCount > 0)// 이 경우는 categorical이든 datetime이든 뭔가로 그룹핑이 된 경우 
            {
                GroupedRows = GroupRecursive(
                    sheetViewModel.FilteredRows.ToList(), 
                    SelectedColumnViewModels.Where(s => s.Type == ColumnType.Categorical).ToList(), 
                    0);

                GroupedRows.Sort(new GroupedRowComparer(sheetViewModel, this));
                GroupedRowViewModels = new List<RowViewModel>();

                foreach (GroupedRows groupedRows in GroupedRows)
                {
                    RowViewModel rowViewModel = new RowViewModel(sheetViewModel.MainPageViewModel)
                    {
                        Index = index++
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

    }

    public class RowViewModelComparer : IComparer<RowViewModel>
    {
        ViewStatus ViewStatus;
        SheetViewModel SheetViewModel;

        public RowViewModelComparer(SheetViewModel sheetViewModel, ViewStatus viewStatus)
        {
            SheetViewModel = sheetViewModel;
            ViewStatus = viewStatus;
        }

        Int32 GetSortDirection(ColumnViewModel cvm) => cvm.SortOption == SortOption.Descending ? -1 : 1;

        public int Compare(RowViewModel x, RowViewModel y)
        {
            IEnumerable<ColumnViewModel> sortAppliedColumnViewModels = SheetViewModel.ColumnViewModels.Where(cvm => cvm.SortOption != SortOption.None).OrderByDescending(cvm => cvm.SortPriority);

            foreach (ColumnViewModel columnViewModel in sortAppliedColumnViewModels)
            {
                if (x.Cells[columnViewModel.Index].Content != y.Cells[columnViewModel.Index].Content)
                {
                    if (columnViewModel.Type == ColumnType.Numerical)
                    {
                        return GetSortDirection(columnViewModel) *
                            ((Double)x.Cells[columnViewModel.Index].Content).CompareTo((Double)y.Cells[columnViewModel.Index].Content);
                    }
                    else if (columnViewModel.Type == ColumnType.Categorical)
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
            foreach (ColumnViewModel columnViewModel in SheetViewModel.ColumnViewModels.Where(cvm => cvm.SortOption != SortOption.None).OrderByDescending(scvm => scvm.SortPriority))
            {
                if (columnViewModel.Type == ColumnType.Categorical)
                {
                    if (x.Keys.ContainsKey(columnViewModel)) // 선택된거면 키에 있을 것
                    {
                        if (x.Keys[columnViewModel] != y.Keys[columnViewModel])
                        {
                            return (x.Keys[columnViewModel] as Category).Order.CompareTo((y.Keys[columnViewModel] as Category).Order) * GetSortDirection(columnViewModel);
                        }
                    }
                    else // 선택되지 않은거면 distinct한 value의 개수로 
                    {
                        Int32 xCount = x.Rows.Select(r => r.Cells[columnViewModel.Index].Content).Distinct().Count();
                        Int32 yCount = y.Rows.Select(r => r.Cells[columnViewModel.Index].Content).Distinct().Count();

                        if (xCount != yCount)
                        {
                            return xCount.CompareTo(yCount) * GetSortDirection(columnViewModel);
                        }
                    }
                }
                else if (columnViewModel.Type == ColumnType.Numerical)
                {
                    if (ViewStatus.SelectedColumnViewModels.Count == 1 && columnViewModel == ViewStatus.SelectedColumnViewModels[0]) // 이 경우는 뉴메리컬 하나만 선택되어 비닝 된 결과가 보이는 경우이다.
                    {
                        ColumnViewModel numerical = ViewStatus.SelectedColumnViewModels[0];
                        Double xMin = (x.Keys[numerical] as Bin).Min,
                               yMin = (y.Keys[numerical] as Bin).Min;

                        if (xMin != yMin)
                            return xMin.CompareTo(yMin) * GetSortDirection(numerical);
                    }
                    else
                    {
                        Double xValue = columnViewModel.AggregativeFunction.Aggregate(x.Rows.Select(r => (Double)r.Cells[columnViewModel.Index].Content));
                        Double yValue = columnViewModel.AggregativeFunction.Aggregate(y.Rows.Select(r => (Double)r.Cells[columnViewModel.Index].Content));

                        if (xValue != yValue)
                        {
                            return xValue.CompareTo(yValue) * GetSortDirection(columnViewModel);
                        }
                    }
                }
            }

            foreach (ColumnViewModel columnViewModel in ViewStatus.SelectedColumnViewModels)
            {
                if (columnViewModel.Type == ColumnType.Categorical)
                {
                    if (x.Keys[columnViewModel] != y.Keys[columnViewModel])
                    {
                        return (x.Keys[columnViewModel] as Category).Order.CompareTo((y.Keys[columnViewModel] as Category).Order) * GetSortDirection(columnViewModel);
                    }
                }
                else if (columnViewModel.Type == ColumnType.Numerical)
                {
                }
            }

            return 0;
        }
    }
}
