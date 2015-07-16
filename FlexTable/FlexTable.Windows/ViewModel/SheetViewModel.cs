using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private ObservableCollection<ViewModel.ColumnViewModel> columnViewModels = new ObservableCollection<ColumnViewModel>();
        public ObservableCollection<ViewModel.ColumnViewModel> ColumnViewModels { get { return columnViewModels; } }

        private List<ViewModel.RowViewModel> rowViewModels = new List<ViewModel.RowViewModel>();
        public List<ViewModel.RowViewModel> RowViewModels { get { return rowViewModels; } }

        ViewModel.MainPageViewModel mainPageViewModel;

        public SheetViewModel(ViewModel.MainPageViewModel mainPageViewModel)
        {
            this.mainPageViewModel = mainPageViewModel;
        }

        public void Initialize(Model.Sheet sheet)
        {
            Sheet = sheet;

            SheetWidth = sheet.Columns.Select(c => c.Width).Sum() + (Double)App.Current.Resources["RowHeaderWidth"];
            SheetHeight = sheet.Rows.Count * (Double)App.Current.Resources["RowHeight"];

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

            /* 기본 row 추가 */
            rowViewModels.Clear();
            index = 0;
            foreach (Model.Row row in sheet.Rows)
            {
                ViewModel.RowViewModel rowViewModel = new ViewModel.RowViewModel(mainPageViewModel) { 
                    Row = row,
                    Index = index
                };
                rowViewModels.Add(rowViewModel);
                index++;
            }
        }
        
        public void Sort(Model.Column column, Boolean isDescending)
        {
            Int32 sortIndex = sheet.Columns.IndexOf(column);

            IOrderedEnumerable<Model.Row> sorted = null;
            if (isDescending)
            {
                sorted = sheet.Rows.ToList().OrderByDescending(r => r.Cells[sortIndex].Content);
            }
            else
            {
                sorted = sheet.Rows.ToList().OrderBy(r => r.Cells[sortIndex].Content);
            }
            Int32 index = 0;

            /*foreach (Model.Row row in sorted)
            {
                row.Index = index++;
            }

            foreach (Model.Row row in sheet.Rows)
            {
                row.OnPropertyChanged("Y");
            }

            foreach (View.RowPresenter rowPresenter in rowPresenters)
            {
                rowPresenter.Update();
            }*/
        }

        public void GroupBy(ViewModel.ColumnViewModel pivotColumnViewModel)
        {
            /*if (pivotColumnViewModel.Type != Model.ColumnType.Categorical)
            {
                Debug.WriteLine("Grouping rows by a numerical column is not supported now.");
                return;
            }

            GroupedColumn = pivotColumnViewModel;
            pivotColumnViewModel.IsGroupedBy = true;
            */

            /* 
             * 먼저 group by 컬럼을 맨 앞으로 
             * 두개 agg된 경우 생각 안하고 있음
             */

            /*IEnumerable<Model.Column> previousColumns = sheet.Columns.Where(c => c.Index < pivotColumnViewModel.Index);

            foreach (Model.Column column in previousColumns)
            {
                column.Index++;
            }
            pivotColumnViewModel.Index = 0;

            // Column의 x값을 업데이트하고 아래에서 Cell을 추가하므로 RowPresenter.UpdateCells() 를 호출하지 않아도 됨
            sheet.UpdateColumnX();


            foreach (Model.Bin bin in pivotColumnViewModel.Bins)
            {
                foreach (Model.Row row in bin.Rows)
                {
                    row.Index = bin.Index;
                    row.OnPropertyChanged("Y");
                }
            }

            foreach (View.RowPresenter rowPresenter in rowPresenters)
            {
                rowPresenter.UpdateAndDestroy(delegate
                {
                    view.RemoveRow(rowPresenter);
                });
            }

            rowViewModels.Clear();
            rowPresenters.Clear();

            foreach (Model.Bin bin in pivotColumnViewModel.Bins)
            {
                Model.Row row = new Model.Row();

                row.Index = bin.Index;
                foreach (Model.Column column in sheet.Columns)
                {
                    Model.Cell cell = new Model.Cell();

                    cell.Column = column; //Cell.Column은 OnPropertyChanged가 안됨.
                    if (column == pivotColumnViewModel)
                    {
                        cell.RawContent = bin.Name;
                        cell.Content = bin.Name;
                    }
                    else
                    {
                        Int32 index = sheet.Columns.IndexOf(column);
                        column.AggregationType = Model.AggregationType.Average;
                        Object aggr = Aggregate(column, bin.Rows.Select(r => r.Cells[index].Content), Model.AggregationType.Average);
                        cell.RawContent = aggr.ToString();
                        cell.Content = aggr;
                    }

                    row.Cells.Add(cell);
                }
                // cell 별로 뷰 업데이트 하지않으려면 일단 cell을 다 만들고 다음에 컨트롤을 만든다.
                ViewModel.RowViewModel rowViewModel = new ViewModel.RowViewModel(this) { Row = row };
                View.RowPresenter rowPresenter = new View.RowPresenter(rowViewModel);

                rowViewModels.Add(rowViewModel);
                rowPresenters.Add(rowPresenter);
                view.AddRow(rowPresenter);

                rowPresenter.Y = row.Y;
                rowPresenter.FadeIn();
            }

            rowHeaderViewModel.SetRowNumber(pivotColumnViewModel.Bins.Count);
            view.UpdateColumnHeaders();
            view.ScrollToColumn(pivotColumnViewModel);*/
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

        public Object Aggregate(Model.Column column, IEnumerable<Object> values, Model.AggregationType aggregationType)
        {
            if (column.Type == Model.ColumnType.Categorical)
            {
                return String.Format("({0})", values.Distinct().Count());
            }
            else
            {
                switch (aggregationType)
                {
                    case Model.AggregationType.Average:
                        return Math.Round(values.Sum(v => (Double)v) / values.Count(), 2);
                    case Model.AggregationType.Maximum:
                        return values.Max();
                }
            }
            throw new Exception("Unknown Aggregation Type");
        }
    }
}
