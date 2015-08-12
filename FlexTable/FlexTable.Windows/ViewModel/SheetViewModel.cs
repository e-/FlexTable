using FlexTable.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace FlexTable.ViewModel
{
    public class SheetViewModel : NotifyViewModel
    {
        private Model.Sheet sheet;
        public Model.Sheet Sheet { get { return sheet; } set { sheet = value; OnPropertyChanged("Sheet"); } }

        private Double sheetWidth;
        public Double SheetWidth { get { return sheetWidth; } private set { sheetWidth = value; OnPropertyChanged("SheetWidth"); } }

        private Double sheetHeight;
        public Double SheetHeight { get { return sheetHeight; } private set { sheetHeight = value; OnPropertyChanged("SheetHeight"); } }

        public Double AllRowsSheetHeight { get { return allRowViewModels.Count * (Double)App.Current.Resources["RowHeight"]; } }

        private ObservableCollection<ViewModel.ColumnViewModel> columnViewModels = new ObservableCollection<ColumnViewModel>();
        public ObservableCollection<ViewModel.ColumnViewModel> ColumnViewModels { get { return columnViewModels; } }

        // 모든 로우에 대한 정보 가지고 있음 속도 위함
        private List<ViewModel.RowViewModel> allRowViewModels = new List<ViewModel.RowViewModel>();
        public List<ViewModel.RowViewModel> AllRowViewModels { get { return allRowViewModels; } }

        // group된 일시적인 테이블
        private List<ViewModel.RowViewModel> temporaryRowViewModels = new List<ViewModel.RowViewModel>();
        public List<ViewModel.RowViewModel> TemporaryRowViewModels { get { return temporaryRowViewModels; } }

        /*private List<ViewModel.RowViewModel> rowViewModels;
        public List<ViewModel.RowViewModel> RowViewModels { get { return rowViewModels; } }*/

        private List<ViewModel.ColumnViewModel> groupedColumnViewModels = new List<ViewModel.ColumnViewModel>();
        public List<ViewModel.ColumnViewModel> GroupedColumnViewModels { get { return groupedColumnViewModels; } }


        private List<GroupedRows> groupingResult;
        public List<GroupedRows> GroupingResult { get { return groupingResult; } }

        ViewModel.MainPageViewModel mainPageViewModel;
        IMainPage view;

        public SheetViewModel(ViewModel.MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.view = view;
        }

        public void Initialize(Model.Sheet sheet)
        {
            Sheet = sheet;

            Int32 index;

            /* 기본 컬럼 추가 */
            columnViewModels.Clear();
            index = 0;
            foreach (Model.Column column in sheet.Columns)
            {
                columnViewModels.Add(new ViewModel.ColumnViewModel(mainPageViewModel) { 
                    Column = column,
                    Index = index
                });
                index++;
            }

            // 컬럼 타입 추측
            foreach (ColumnViewModel columnViewModel in columnViewModels)
            {
                index = columnViewModel.Index;
                columnViewModel.Type = Column.GuessColumnType(sheet.Rows.Select(r => r.Cells[index].RawContent));
                columnViewModel.ContainsString = Column.CheckStringValue(sheet.Rows.Select(r => r.Cells[index].RawContent));

                if (columnViewModel.Type == ColumnType.Categorical)
                {
                    List<String> uniqueValues = new List<String>();

                    foreach (Model.Row row in sheet.Rows)
                    {
                        String value = row.Cells[index].RawContent;
                        if (!uniqueValues.Contains(value))
                        {
                            uniqueValues.Add(value);
                        }
                    }

                    // uniqueValues의 순서 정해야 함.
                    if (columnViewModel.ContainsString)
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
                    
                    // 원래 cateogorical의 content는 string이 들어있을 텐데 이를 Category로 바꾼다. 즉 content는 Category 아니면 Double임
                    foreach (Model.Row row in sheet.Rows)
                    {
                        String value = row.Cells[index].RawContent;
                        row.Cells[index].Content = categories.Where(c => c.Value == value).First();
                    }
                }
                else
                {
                    foreach (Model.Row row in sheet.Rows)
                    {
                        String value = row.Cells[index].RawContent;
                        row.Cells[index].Content = Double.Parse(row.Cells[index].RawContent);
                    }
                }
            }

            // 추측한 컬럼 타입에 대해 순서 정함 (카테고리컬을 먼저 보여줌)
            index = 0;
            foreach (ColumnViewModel columnViewModel in columnViewModels.Where(c => c.Type == ColumnType.Categorical))
            {
                columnViewModel.Order = index++;
            }
            foreach (ColumnViewModel columnViewModel in columnViewModels.Where(c => c.Type == ColumnType.Numerical))
            {
                columnViewModel.Order = index++;
            }

            /* 기본 row 추가 */
            allRowViewModels.Clear();
            index = 0;
            foreach (Model.Row row in sheet.Rows)
            {
                ViewModel.RowViewModel rowViewModel = new ViewModel.RowViewModel(mainPageViewModel) { 
                    Index = index
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

            //mainPageViewModel.TableViewModel.UpdateCellXPosition();

            foreach (View.RowPresenter rowPresenter in mainPageViewModel.TableViewModel.RowPresenters)
            {
                rowPresenter.UpdateCellsWithoutAnimation();
            }

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

            //mainPageViewModel.TableViewModel.UpdateCellXPosition();
            foreach (View.RowPresenter rowPresenter in mainPageViewModel.TableViewModel.RowPresenters)
            {
                rowPresenter.UpdateCellsWithoutAnimation();
            }

            mainPageViewModel.View.TableView.TopColumnHeader.Update();
            mainPageViewModel.View.TableView.BottomColumnHeader.Update();
        }

        public void Ungroup(ViewModel.ColumnViewModel pivotColumnViewModel)
        {
            groupedColumnViewModels.Remove(pivotColumnViewModel);
            pivotColumnViewModel.IsGroupedBy = false;
            GroupUpdate();
        }

        public void Ungroup()
        {
            if (groupedColumnViewModels.Count > 0)
            {
                throw new Exception("Calling Ungroup method is permitted only when no column has been grouped");
            }
            GroupUpdate();
        }

        public void Group()
        {
            if (groupedColumnViewModels.Count > 0)
            {
                throw new Exception("Calling Group method is permitted only when no column has been grouped");
            }
            GroupUpdate();
        }

        public void Group(ViewModel.ColumnViewModel pivotColumnViewModel)
        {
            if (pivotColumnViewModel.Type != Model.ColumnType.Categorical)
            {
                throw new Exception("Grouping rows by a numerical column is not supported.");
            }

            groupedColumnViewModels.Add(pivotColumnViewModel);
            pivotColumnViewModel.IsGroupedBy = true;

            GroupUpdate();
        }

        public void GroupUpdate()
        {
            // column order 조정 group된 것을 맨 앞으로
            var ordered = columnViewModels.OrderBy(c => c.Order);
            Int32 order = 0;

            // 우선으로 그룹된 컬럼에 순서 할당
            foreach (ColumnViewModel groupedColumnViewModel in groupedColumnViewModels)
            {
                groupedColumnViewModel.Order = order++;
            }

            foreach (ColumnViewModel remainingColumnViewModel in columnViewModels.Except(groupedColumnViewModels).OrderBy(d => d.Order))
            {
                remainingColumnViewModel.Order = order++;
            }

            // Column의 x값을 업데이트하고 아래에서 Cell을 추가하므로 RowPresenter.UpdateCells() 를 호출하지 않아도 됨
            UpdateColumnX();

            // table에 추가하는 것은 tableViewModel이 할 것이고 여기는 rowViewModels만 만들어주면 됨
            temporaryRowViewModels.Clear();

            Int32 index = 0;

            if (groupedColumnViewModels.Count == 0)
            {
                RowViewModel rowViewModel = new RowViewModel(mainPageViewModel)
                {
                    Index = 0
                };

                foreach (ColumnViewModel columnViewModel in columnViewModels)
                {
                    Model.Cell cell = new Model.Cell();

                    cell.ColumnViewModel = columnViewModel;

                    if (columnViewModel.Type == ColumnType.Categorical)
                    {
                        Int32 uniqueCount = GetUniqueList(Sheet.Rows.Select(r => r.Cells[columnViewModel.Index].Content as Category)).Count;
                        cell.Content = String.Format("({0})", uniqueCount);
                        cell.RawContent = String.Format("({0})", uniqueCount);
                    }
                    else //numerical
                    {
                        Object aggregated = Aggregate(Sheet.Rows.Select(r => r.Cells[columnViewModel.Index].Content), columnViewModel.AggregationType);
                        cell.RawContent = aggregated.ToString();
                        cell.Content = aggregated;
                    }

                    rowViewModel.Cells.Add(cell);
                }

                temporaryRowViewModels.Add(rowViewModel);
            }
            else
            {
                groupingResult = GroupRecursive(sheet.Rows.ToList(), 0);
                
                foreach (GroupedRows groupedRows in groupingResult)
                {
                    RowViewModel rowViewModel = new RowViewModel(mainPageViewModel)
                    {
                        Index = index++
                    };

                    foreach (ColumnViewModel columnViewModel in columnViewModels)
                    {
                        Model.Cell cell = new Model.Cell();

                        cell.ColumnViewModel = columnViewModel;

                        if (groupedRows.Keys.ContainsKey(columnViewModel))
                        {
                            cell.Content = groupedRows.Keys[columnViewModel];
                            cell.RawContent = cell.Content.ToString();
                        }
                        else if (columnViewModel.Type == ColumnType.Categorical)
                        {
                            Int32 uniqueCount = GetUniqueList(groupedRows.Rows.Select(r => r.Cells[columnViewModel.Index].Content as Category)).Count;
                            cell.Content = String.Format("({0})", uniqueCount);
                            cell.RawContent = String.Format("({0})", uniqueCount);
                        }
                        else //numerical
                        {
                            Object aggregated = Aggregate(groupedRows.Rows.Select(r => r.Cells[columnViewModel.Index].Content), columnViewModel.AggregationType);
                            cell.RawContent = aggregated.ToString();
                            cell.Content = aggregated;
                        }

                        rowViewModel.Cells.Add(cell);
                    }

                    temporaryRowViewModels.Add(rowViewModel);
                }
            }
            SheetHeight = temporaryRowViewModels.Count * (Double)App.Current.Resources["RowHeight"];
        }

        public List<GroupedRows> GroupRecursive(List<Row> rows, Int32 pivotIndex)
        {            
            ColumnViewModel pivot = groupedColumnViewModels[pivotIndex];
            Dictionary<Category, List<Row>> dict = GetRowsByColumnViewModel(rows, pivot);
            
            if (pivotIndex < groupedColumnViewModels.Count - 1) // 그루핑을 더 해야함.
            {
                List<GroupedRows> groupedRowsList = new List<GroupedRows>();
                foreach(KeyValuePair<Category, List<Row>> kv in dict)
                {
                    List<GroupedRows> ret = GroupRecursive(kv.Value, pivotIndex + 1);

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
                foreach (KeyValuePair<Category, List<Row>> kv in dict)
                {
                    GroupedRows groupedRows = new GroupedRows();
                    groupedRows.Keys[pivot] = kv.Key;
                    groupedRows.Rows = kv.Value;

                    groupedRowsList.Add(groupedRows);
                }

                return groupedRowsList;
            }
        }

        public Dictionary<Category, Int32> GetUniqueList(IEnumerable<Category> values)
        {
            Dictionary<Category, Int32> dict = new Dictionary<Category, int>();
            
            foreach (Category value in values)
            {
                if (!dict.ContainsKey(value))
                    dict[value] = 0;

                dict[value]++;
            }

            return dict;
        }

        public List<Tuple<Category, Int32>> CountByColumnViewModel(ColumnViewModel columnViewModel)
        {
            return GetRowsByColumnViewModel(sheet.Rows, columnViewModel).Select(kv => new Tuple<Category, Int32>(kv.Key, kv.Value.Count)).ToList();
        }

        public Dictionary<Category, List<Model.Row>> GetRowsByColumnViewModel(IEnumerable<Row> rows, ColumnViewModel columnViewModel)
        {
            Dictionary<Category, List<Model.Row>> dict = new Dictionary<Category, List<Model.Row>>();
            
            foreach (Model.Row row in rows)
            {
                Category category = row.Cells[columnViewModel.Index].Content as Category;
                if(!dict.ContainsKey(category)) {
                    dict[category] = new List<Model.Row>();
                }

                dict[category].Add(row);
            }

            return dict;
        }

        public List<Tuple<Category, Category, Int32>> CountByDoubleColumnViewModel(ColumnViewModel secondary)
        {
            ColumnViewModel primary = groupedColumnViewModels.Last();
            Dictionary<Category, List<Model.Row>> dict = GetRowsByColumnViewModel(sheet.Rows, primary);
            List<Tuple<Category, Category, Int32>> result = new List<Tuple<Category, Category, Int32>>();

            foreach (KeyValuePair<Category, List<Model.Row>> kv in dict)
            {
                result.AddRange(GetRowsByColumnViewModel(kv.Value, secondary).Select(kv2 => new Tuple<Category, Category, Int32>(kv.Key, kv2.Key, kv2.Value.Count)));
            }
            return result;
        }

        public void ChangeAggregationType(Int32 columnIndex, Model.AggregationType aggregationType)
        {
            /*if (groupedColumn == null) return;

            Model.Column column = sheet.Columns[columnIndex];

            Int32 index = 0;
            foreach (Model.Bin bin in groupedColumn.Bins)
            {
                Model.Row row = rowViewModels[index].Row;
                column.AggregationType = aggregationType;

                Object aggr = Aggregate(column, bin.Rows.Select(r => r.Cells[columnIndex].Content), aggregationType);
                row.Cells[columnIndex].RawContent = aggr.ToString();
                row.Cells[columnIndex].Content = aggr;
                index++;
            }

            if (column == chartedColumn)
            {
                DrawChart(chartedColumnIndex);
            }*/
        }

        public Object Aggregate(IEnumerable<Object> values, Model.AggregationType aggregationType)
        {
            switch (aggregationType)
            {
                case Model.AggregationType.Average:
                    return Math.Round(values.Sum(v => (Double)v) / values.Count(), 2);
                case Model.AggregationType.Maximum:
                    return values.Max();
            }
            throw new Exception("Unknown Aggregation Type");
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

                columnViewModel.Width = Math.Max(view.DummyTextBlock.ActualWidth, 40);
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

            

        /*public void CreateColumnSummary()
        {
            for (Int32 i = 0; i < columnViewModels.Count; ++i)
            {
                ColumnViewModel columnViewModel = columnViewModels[i];
                if (columnViewModel.Type == ColumnType.Categorical) // bar chart
                {
                    columnViewModel.Bins = Column.GetFrequencyBins(rowViewModels, i);
                }
                else // histogram
                {
                    columnViewModel.Bins = new List<Bin>();
                }
            }
        }*/
    }
}
