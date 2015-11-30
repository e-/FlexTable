using FlexTable.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using d3.Scale;
using FlexTable.Util;
using d3.ColorScheme;

namespace FlexTable.ViewModel
{
    public class SheetViewModel : NotifyViewModel
    {
        private Sheet sheet;
        public Sheet Sheet { get { return sheet; } set { sheet = value; OnPropertyChanged("Sheet"); } }

        private Double sheetWidth;
        public Double SheetWidth { get { return sheetWidth; } private set { sheetWidth = value; OnPropertyChanged("SheetWidth"); } }

        private Double sheetHeight;
        public Double SheetHeight { get { return sheetHeight; } private set { sheetHeight = value; OnPropertyChanged("SheetHeight"); } }

        public Double AllRowsSheetHeight { get { return allRowViewModels.Count * (Double)App.Current.Resources["RowHeight"]; } }

        private List<ColumnViewModel> columnViewModels = new List<ColumnViewModel>();
        public List<ColumnViewModel> ColumnViewModels => columnViewModels;

        // 모든 로우에 대한 정보 가지고 있음 속도 위함
        private List<RowViewModel> allRowViewModels = new List<RowViewModel>();
        public List<RowViewModel> AllRowViewModels => allRowViewModels;

        // group된 일시적인 테이블
        private List<RowViewModel> groupedRowViewModels = new List<RowViewModel>();
        public List<RowViewModel> GroupedRowViewModels => groupedRowViewModels;

        private List<GroupedRows> groupingResult;
        public List<GroupedRows> GroupingResult => groupingResult;

        public ObservableCollection<FilterViewModel> FilterViewModels { get; private set; } = new ObservableCollection<FilterViewModel>();

        MainPageViewModel mainPageViewModel;

        /// <summary>
        /// 여기도 소팅해야하나? 어차피 이 집합에 속하는지 안하는지만 테스트하고 차트 그릴때는 통계를 쓰는거라 렌더링하는 rowviewmodel이 아니면 소팅 필요없지않나
        /// </summary>
        public IEnumerable<Row> FilteredRows { get { return FilterViewModel.ApplyFilters(FilterViewModels, sheet.Rows); } }

        IMainPage view;

        public SheetViewModel(MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.view = view;
        }

        public static ColumnType GuessColumnType(IEnumerable<String> cellValues)
        {
            Boolean allDouble = true;
            Double result;
            List<String> differentValues = new List<String>();

            foreach (String value in cellValues)
            {
                if (differentValues.IndexOf(value) < 0)
                {
                    differentValues.Add(value);
                }

                if (!Double.TryParse(value, out result))
                {
                    allDouble = false;
                    break;
                }
            }

            // 문자가 하나라도 있으면 무조건 범주형
            if (!allDouble) return ColumnType.Categorical;

            if (differentValues.Count < 10) return ColumnType.Categorical;
            return ColumnType.Numerical;
        }

        public static Boolean CheckStringValue(IEnumerable<String> cellValues)
        {
            Boolean allDouble = true;
            Double result;

            foreach (String value in cellValues)
            {
                if (!Double.TryParse(value, out result))
                {
                    allDouble = false;
                    break;
                }
            }

            return !allDouble;
        }

        public void Initialize(Sheet sheet)
        {
            Sheet = sheet;

            Int32 index;

            /* 기본 컬럼 추가 */
            columnViewModels.Clear();
            index = 0;
            foreach (Column column in sheet.Columns)
            {
                columnViewModels.Add(new ColumnViewModel(mainPageViewModel) { 
                    Column = column,
                    Index = index
                });
                index++;
            }

            // 컬럼 타입 추측
            foreach (ColumnViewModel columnViewModel in columnViewModels)
            {
                index = columnViewModel.Index;
                columnViewModel.Type = columnViewModel.Column.Type;
                columnViewModel.CategoricalType = columnViewModel.Column.CategoricalType;
                columnViewModel.Unit = columnViewModel.Column.Unit;

                columnViewModel.ContainString = CheckStringValue(sheet.Rows.Select(r => r.Cells[index].RawContent));

                if (columnViewModel.Type == ColumnType.Categorical)
                {
                    List<String> uniqueValues = new List<String>();

                    foreach (Row row in sheet.Rows)
                    {
                        String value = row.Cells[index].RawContent;
                        if (!uniqueValues.Contains(value))
                        {
                            uniqueValues.Add(value);
                        }
                    }

                    // uniqueValues의 순서 정해야 함.
                    if (columnViewModel.ContainString)
                    {
                        uniqueValues = uniqueValues.OrderBy(u => u).ToList();
                    }
                    else
                    {
                        uniqueValues = uniqueValues.OrderBy(u => Double.Parse(u)).ToList();
                    }

                    // 카테고리 추가 후
                    List<Category> categories = uniqueValues.Select(u => new Category() { Value = u }).ToList();

                    Int32 categoryIndex = 0;
                    foreach (Category category in categories)
                    {
                        category.Order = categoryIndex;
                        category.Color = Category10.Colors[categoryIndex % Category10.Colors.Count];
                        categoryIndex++;
                    }

                    columnViewModel.Categories = categories;
                    
                    // 원래 cateogorical의 content는 string이 들어있을 텐데 이를 Category로 바꾼다. 즉 content는 Category 아니면 Double임 혹은 데이트타임일수도
                    foreach (Row row in sheet.Rows)
                    {
                        String value = row.Cells[index].RawContent;
                        row.Cells[index].Content = categories.Where(c => c.Value == value).First();
                    }
                }
                else if (columnViewModel.Type == ColumnType.Numerical)
                {
                    foreach (Row row in sheet.Rows)
                    {
                        String value = row.Cells[index].RawContent;
                        row.Cells[index].Content = Double.Parse(row.Cells[index].RawContent);
                    }
                }
            }

            // 추측한 컬럼 타입에 대해 순서 정함 (카테고리컬을 먼저 보여줌)
            // 그냥 그대로 보여주는것으로 변경

            index = 0;
            /*foreach (ColumnViewModel columnViewModel in columnViewModels.Where(c => c.Type == ColumnType.Categorical))
            {
                columnViewModel.Order = index++;
            }*/
            
            foreach (ColumnViewModel columnViewModel in columnViewModels/*.Where(c => c.Type != ColumnType.Categorical)*/)
            {
                columnViewModel.Order = index++;
            }

            /* 기본 row 추가 */
            allRowViewModels.Clear();
            index = 0;
            foreach (Row row in sheet.Rows)
            {
                RowViewModel rowViewModel = new RowViewModel(mainPageViewModel) { 
                    Index = index,
                    Row = row
                };
                Int32 index2 = 0;
                foreach (Cell cell in row.Cells)
                {
                    cell.ColumnViewModel = columnViewModels[index2++];
                    rowViewModel.Cells.Add(cell);
                }
                allRowViewModels.Add(rowViewModel);

                index++;
            }

            MeasureColumnWidth();
            UpdateColumnX();

            SheetWidth = columnViewModels.Select(c => c.Width).Sum() + (Double)App.Current.Resources["RowHeaderWidth"];
            SheetHeight = allRowViewModels.Count * (Double)App.Current.Resources["RowHeight"];
        }

        public void MeasureColumnWidth()
        {
            foreach (ColumnViewModel columnViewModel in columnViewModels)
            {
                String maxValue = (from rowViewModel in allRowViewModels
                                   orderby rowViewModel.Cells[columnViewModel.Index].RawContent.Count() descending
                                   select rowViewModel.Cells[columnViewModel.Index].RawContent).First();

                view.DummyTextBlock.Text = maxValue;
                view.DummyTextBlock.Measure(new Size(Double.MaxValue, Double.MaxValue));

                Double width = view.DummyTextBlock.ActualWidth;

                if (columnViewModel.Type == ColumnType.Numerical)
                {
                    view.DummyTextBlock.Text = String.Format("AVG({0})", columnViewModel.Column.Name);
                    view.DummyTextBlock.Measure(new Size(Double.MaxValue, Double.MaxValue));
                    if (width < view.DummyTextBlock.ActualWidth)
                        width = view.DummyTextBlock.ActualWidth;
                }
                else if (columnViewModel.Type == ColumnType.Categorical)
                {
                    view.DummyTextBlock.Text = columnViewModel.Column.Name;
                    view.DummyTextBlock.Measure(new Size(Double.MaxValue, Double.MaxValue));
                    if (width < view.DummyTextBlock.ActualWidth)
                        width = view.DummyTextBlock.ActualWidth;
                }
                columnViewModel.Width = width + 13 /* width of check icon */ + 10 /* width of sort caret*/;
            }
        }

        Int32 sortPriority = 0;

        public void Sort(ColumnViewModel columnViewModel, SortOption sortOption)
        {
            columnViewModel.SortOption = sortOption;
            columnViewModel.SortPriority = sortPriority++;
        }

        public void SetAside(ColumnViewModel columnViewModel)
        {
            if (columnViewModel.IsHidden) return;

            columnViewModel.Hide();
            IEnumerable<ColumnViewModel> nexts = columnViewModels.Where(c => c.Order > columnViewModel.Order);

            foreach (ColumnViewModel cvm in nexts)
            {
                cvm.Order--;
            }

            columnViewModel.Order = columnViewModels.Count - 1;
        }

        public void BringFront(ColumnViewModel columnViewModel)
        {
            if (!columnViewModel.IsHidden) return;

            columnViewModel.Show();
            Int32 order = columnViewModels.Count(c => !c.IsHidden && c.Index < columnViewModel.Index);
            IEnumerable<ColumnViewModel> nexts = columnViewModels.Where(c => order <= c.Order && c.Order < columnViewModel.Order);

            foreach (ColumnViewModel cvm in nexts)
            {
                cvm.Order++;
            }

            columnViewModel.Order = order;
        }
 

        public void UpdateGroup(ViewStatus viewStatus)
        {
            // column order 조정 group된 것을 맨 앞으로
            var ordered = columnViewModels.OrderBy(c => c.Order);
            Int32 order = 0;

            // 우선으로 그룹된 컬럼에 순서 할당
            foreach (ColumnViewModel groupedColumnViewModel in viewStatus.SelectedColumnViewModels.Where(s => s.Type == ColumnType.Categorical))
            {
                groupedColumnViewModel.Order = order++;
            }

            foreach (ColumnViewModel groupedColumnViewModel in viewStatus.SelectedColumnViewModels.Where(s => s.Type == ColumnType.Numerical))
            {
                groupedColumnViewModel.Order = order++;
            }

            foreach (ColumnViewModel remainingColumnViewModel in columnViewModels.Except(viewStatus.SelectedColumnViewModels).Where(d => !d.IsHidden).OrderBy(d => d.Index))
            {
                remainingColumnViewModel.Order = order++;
            }

            foreach (ColumnViewModel remainingColumnViewModel in columnViewModels.Except(viewStatus.SelectedColumnViewModels).Where(d => d.IsHidden).OrderBy(d => d.Order))
            {
                remainingColumnViewModel.Order = order++;
            }

            // Column의 x값을 업데이트함, 위 아래 컬럼 헤더를 위한것임. 
            UpdateColumnX();

            // table에 추가하는 것은 tableViewModel이 할 것이고 여기는 rowViewModels만 만들어주면 됨
            
            Int32 index = 0;

            // 여기서 상황별로 왼쪽에 보일 rowViewModel을 만들어 줘야함. 여기서 만들면 tableViewModel에서 받아다가 그림
            
            if (viewStatus.IsEmpty || viewStatus.IsNN || viewStatus.IsCNN)
            {
                // 어차피 allRow가 보일 것이므로 RowViewModel 을 만들어 줄 필요는 없음 
            }
            else if (viewStatus.IsN) // 이 경우는 뉴메리컬 하나만 선택되어 비닝 된 결과가 보이는 경우이다.
            {
                groupedRowViewModels.Clear();

                ColumnViewModel selected = viewStatus.FirstColumn;
                if (selected.SortOption == SortOption.None) Sort(selected, SortOption.Ascending);

                List<GroupedRows> binResult = Bin(selected, FilteredRows.ToList());

                // 여기서 groupedRows가 소팅되어야함 
                // 그런데 여기서는 선택되지 않은 컬럼의 경우에는 어차피 어그리게이션되므로 소팅 순서가 의미가 없음 따라서 선택된 컬럼에 대해서만 소팅하면 된다

                binResult.Sort(new GroupedRowComparer(this, viewStatus));

                foreach (GroupedRows groupedRows in binResult)
                {
                    if (groupedRows.Rows.Count == 0) continue;
                    RowViewModel rowViewModel = new RowViewModel(mainPageViewModel)
                    {
                        Index = index++
                    };

                    foreach (ColumnViewModel columnViewModel in ColumnViewModels)
                    {
                        Cell cell = new Cell();

                        cell.ColumnViewModel = columnViewModel;

                        if (columnViewModel == selected)
                        {
                            Bin bin = groupedRows.Keys[selected] as Bin;

                            String content = $"{Formatter.FormatAuto3(bin.Min)} - {Formatter.FormatAuto3(bin.Max)} ({groupedRows.Rows.Count})";
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
                            String formatted = Util.Formatter.FormatAuto4((Double)aggregated);
                            cell.RawContent = formatted;
                            cell.Content = Double.Parse(formatted);
                        }

                        rowViewModel.Cells.Add(cell);
                    }

                    groupedRowViewModels.Add(rowViewModel);
                }
            }
            else // 이 경우는 categorical이든 datetime이든 뭔가로 그룹핑이 된 경우 
            {
                groupedRowViewModels.Clear();

                groupingResult = GroupRecursive(FilteredRows.ToList(), viewStatus.SelectedColumnViewModels.Where(s => s.Type == ColumnType.Categorical).ToList() , 0);
                groupingResult.Sort(new GroupedRowComparer(this, viewStatus));

                foreach (GroupedRows groupedRows in groupingResult)
                {
                    RowViewModel rowViewModel = new RowViewModel(mainPageViewModel)
                    {
                        Index = index++
                    };

                    foreach (ColumnViewModel columnViewModel in ColumnViewModels)
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

                    groupedRowViewModels.Add(rowViewModel);
                }
            }

            SheetHeight = groupedRowViewModels.Count * (Double)App.Current.Resources["RowHeight"];
        }

        public static List<GroupedRows> GroupRecursive(List<Row> rows, List<ColumnViewModel> groupedColumnViewModels, Int32 pivotIndex)
        {            
            ColumnViewModel pivot = groupedColumnViewModels[pivotIndex];
            Dictionary<Object, List<Row>> dict = GetRowsByColumnViewModel(rows, pivot);
            
            if (pivotIndex < groupedColumnViewModels.Count - 1) // 그루핑을 더 해야함.
            {
                List<GroupedRows> groupedRowsList = new List<GroupedRows>();
                foreach(KeyValuePair<Object, List<Row>> kv in dict)
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
        
        public static List<GroupedRows> Bin(ColumnViewModel selected, List<Row> rows)
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
            foreach(Bin bin in bins)
            {
                GroupedRows grs = new GroupedRows();
                grs.Keys[selected] = bin;
                grs.Rows = bin.Rows.ToList();
                groupedRows.Add(grs);
            }

            return groupedRows;
        }

        public static Dictionary<Object, List<Row>> GetRowsByColumnViewModel(IEnumerable<Row> rows, ColumnViewModel columnViewModel)
        {
            Dictionary<Object, List<Row>> dict = new Dictionary<Object, List<Row>>();
            
            foreach (Row row in rows)
            {
                Object content = row.Cells[columnViewModel.Index].Content;
                if(!dict.ContainsKey(content)) {
                    dict[content] = new List<Row>();
                }

                dict[content].Add(row);
            }

            return dict;
        }

        
        public void UpdateColumnX()
        {
            Double total = 0;
            foreach (ColumnViewModel columnViewModel in columnViewModels.OrderBy(c => c.Order))
            {
                columnViewModel.X = total;
                total += columnViewModel.Width;
            }
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

            foreach(ColumnViewModel columnViewModel in sortAppliedColumnViewModels)
            {
                if(x.Cells[columnViewModel.Index].Content != y.Cells[columnViewModel.Index].Content)
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
                if(columnViewModel.Type == ColumnType.Categorical)
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

                        if(xCount != yCount)
                        {
                            return xCount.CompareTo(yCount) * GetSortDirection(columnViewModel);
                        }
                    }
                }
                else if(columnViewModel.Type == ColumnType.Numerical)
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
