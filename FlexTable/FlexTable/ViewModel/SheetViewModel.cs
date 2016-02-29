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

        public Double AllRowsSheetHeight { get { return allRowViewModels.Count * Const.RowHeight; } }

        private List<ColumnViewModel> columnViewModels = new List<ColumnViewModel>();
        public List<ColumnViewModel> ColumnViewModels => columnViewModels;

        // 모든 로우에 대한 정보 가지고 있음 속도 위함
        private List<RowViewModel> allRowViewModels = new List<RowViewModel>();
        public List<RowViewModel> AllRowViewModels => allRowViewModels;

        public ObservableCollection<FilterViewModel> FilterViewModels { get; private set; } = new ObservableCollection<FilterViewModel>();

        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

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
            FilterViewModels.Clear();

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

                    if (columnViewModel.Column.CategoircalOrder != null) // 이미 지정된 순서가 있으면
                    {
                        List<String> order = columnViewModel.Column.CategoircalOrder;
                        uniqueValues = uniqueValues.OrderBy(u => order.IndexOf(u)).ToList();
                    }
                    else {
                        // uniqueValues의 순서 정해야 함.
                        if (columnViewModel.ContainString)
                        {
                            uniqueValues = uniqueValues.OrderBy(u => u).ToList();
                        }
                        else
                        {
                            uniqueValues = uniqueValues.OrderBy(u => Double.Parse(u)).ToList();
                        }
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
            SheetHeight = allRowViewModels.Count * Const.RowHeight;
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
                    view.DummyTextBlock.Text = $"{Const.Loader.GetString("Max")}({columnViewModel.Column.Name})";
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

            columnViewModel.IsHidden = true;
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

            columnViewModel.IsHidden = false;
            Int32 order = columnViewModels.Count(c => !c.IsHidden && c.Index < columnViewModel.Index);
            IEnumerable<ColumnViewModel> nexts = columnViewModels.Where(c => order <= c.Order && c.Order < columnViewModel.Order);

            foreach (ColumnViewModel cvm in nexts)
            {
                cvm.Order++;
            }

            columnViewModel.Order = order;
        }


        /// <summary>
        /// 새로운 컬럼이 선택되거나 필터링이 적용되거나 소팅이 적용되거나 등으로 인해 로우가 업데이트 되었을 때 호출됨.
        /// 프리뷰를 한다고 호출되지 않는다는 점에 주의
        /// </summary>
        /// <param name="viewStatus"></param>
        public void Reflect(ViewStatus viewStatus)
        {
            UpdateOrder(viewStatus);

            //SheetHeight = groupedRowViewModels.Count * Const.RowHeight;
        }

        public void UpdateOrder(ViewStatus viewStatus)
        {
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

            foreach (ColumnViewModel remainingColumnViewModel in columnViewModels.Except(viewStatus.SelectedColumnViewModels).Where(d => d.IsHidden).OrderBy(d => d.Index))
            {
                remainingColumnViewModel.Order = order++;
            }

            // Column의 x값을 업데이트함, 위 아래 컬럼 헤더를 위한것임. 
            UpdateColumnX();
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
