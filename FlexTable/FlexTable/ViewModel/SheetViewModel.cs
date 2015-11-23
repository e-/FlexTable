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

        //ViewStatus viewStatus = new ViewStatus();

        private List<GroupedRows> groupingResult;
        public List<GroupedRows> GroupingResult => groupingResult;

        public Boolean IsAllRowsVisible { get; set; } = false;
        MainPageViewModel mainPageViewModel;

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
                        category.Order = categoryIndex++;
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

            //rowViewModels = allRowViewModels;

            MeasureColumnWidth();
            UpdateColumnX();

            SheetWidth = columnViewModels.Select(c => c.Width).Sum() + (Double)App.Current.Resources["RowHeaderWidth"];
            SheetHeight = allRowViewModels.Count * (Double)App.Current.Resources["RowHeight"];
        }

        public void SetAside(ColumnViewModel columnViewModel)
        {
            if (columnViewModel.IsHidden) return;

            columnViewModel.Hide();
            IEnumerable<ColumnViewModel> nexts = columnViewModels.Where(c => c.Order > columnViewModel.Order);

            foreach (ColumnViewModel cvm in nexts)
            {
                cvm.Order--;
                cvm.IsXDirty = true;
            }

            columnViewModel.Order = columnViewModels.Count - 1;
            columnViewModel.IsXDirty = true;

            UpdateColumnX();

            // TODO

            /*
            foreach (View.RowPresenter rowPresenter in mainPageViewModel.TableViewModel.RowPresenters)
            {
                rowPresenter.UpdateCellsWithoutAnimation();
            }
            */

            mainPageViewModel.View.TableView.TopColumnHeader.Update();
            mainPageViewModel.View.TableView.BottomColumnHeader.Update();
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
                cvm.IsXDirty = true;
            }

            columnViewModel.Order = order;
            columnViewModel.IsXDirty = true;

            UpdateColumnX();

            // TODO

            /*
            foreach (View.RowPresenter rowPresenter in mainPageViewModel.TableViewModel.RowPresenters)
            {
                rowPresenter.UpdateCellsWithoutAnimation();
            }
            */

            mainPageViewModel.View.TableView.TopColumnHeader.Update();
            mainPageViewModel.View.TableView.BottomColumnHeader.Update();
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

            var orderedColumnViewModels = columnViewModels.OrderBy(cvm => cvm.Order);

            // Column의 x값을 업데이트함, 위 아래 컬럼 헤더를 위한것임. 
            UpdateColumnX();

            // table에 추가하는 것은 tableViewModel이 할 것이고 여기는 rowViewModels만 만들어주면 됨
            groupedRowViewModels.Clear();

            Int32 index = 0;

            // 여기서 상황별로 왼쪽에 보일 rowViewModel을 만들어 줘야함. 여기서 만들면 tableViewModel에서 받아다가 그림

            IsAllRowsVisible = false;
            if (viewStatus.SelectedColumnViewModels.Count == 0 ||
                viewStatus.SelectedColumnViewModels.Count >= 2 && viewStatus.SelectedColumnViewModels.Count(s => s.Type != ColumnType.Numerical) == 0 ||
                viewStatus.SelectedColumnViewModels.Count == 3 && viewStatus.SelectedColumnViewModels.Count(s => s.Type == ColumnType.Numerical) == 2
                )
            {
                IsAllRowsVisible = true;
                foreach(RowViewModel rowViewModel in allRowViewModels)
                {
                    groupedRowViewModels.Add(rowViewModel);
                }
            }
            else if (viewStatus.SelectedColumnViewModels.Count == 1 && viewStatus.SelectedColumnViewModels[0].Type == ColumnType.Numerical) // 이 경우는 뉴메리컬 하나만 선택되어 비닝 된 결과가 보이는 경우이다.
            {
                ColumnViewModel selected = viewStatus.SelectedColumnViewModels[0];

                List<GroupedRows> binResult = Bin(selected, Sheet.Rows);

                foreach (GroupedRows groupedRows in binResult)
                {
                    if (groupedRows.Rows.Count == 0) continue;
                    RowViewModel rowViewModel = new RowViewModel(mainPageViewModel)
                    {
                        Index = index++
                    };

                    foreach (ColumnViewModel columnViewModel in orderedColumnViewModels)
                    {
                        Cell cell = new Cell();

                        cell.ColumnViewModel = columnViewModel;

                        if (columnViewModel == selected)
                        {
                            String content = $"{groupedRows.Keys[selected]} ({groupedRows.Rows.Count})";
                            cell.RawContent = content;
                            cell.Content = content;
                        }
                        else if (columnViewModel.Type == ColumnType.Categorical)
                        {
                            Int32 uniqueCount = Sheet.Rows.Select(r => r.Cells[columnViewModel.Index].Content).Distinct().Count();
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
                groupingResult = GroupRecursive(sheet.Rows, viewStatus.SelectedColumnViewModels.Where(s => s.Type == ColumnType.Categorical).ToList() , 0);
                
                foreach (GroupedRows groupedRows in groupingResult)
                {
                    RowViewModel rowViewModel = new RowViewModel(mainPageViewModel)
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
                            String formatted = Util.Formatter.FormatAuto4((Double)aggregated);
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

            List<Tuple<Double, Double, Int32>> bins = Util.HistogramCalculator.Bin(
                linear.DomainStart,
                linear.DomainEnd,
                linear.Step,
                rows.Select(r => (Double)r.Cells[selected.Index].Content)
                );

            List<GroupedRows> groupedRows = new List<GroupedRows>();
            foreach(Tuple<Double, Double, Int32> bin in bins)
            {
                GroupedRows grs = new GroupedRows();
                grs.Keys[selected] = $"{Util.Formatter.FormatAuto3(bin.Item1)} - {Util.Formatter.FormatAuto3(bin.Item2)}";
                groupedRows.Add(grs);
            }

            foreach(Row row in rows)
            {
                Int32 index = (Int32)Math.Floor(((Double)row.Cells[selected.Index].Content - linear.DomainStart) / linear.Step);
                if (index >= bins.Count) index = bins.Count - 1;

                groupedRows[index].Rows.Add(row);
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
}
