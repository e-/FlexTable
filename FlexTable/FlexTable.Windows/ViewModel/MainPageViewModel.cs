using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace FlexTable.ViewModel
{
    public class MainPageViewModel : NotifyViewModel
    {
        private IMainPage view;
        private Model.Sheet sheet;
        public Model.Sheet Sheet
        {
            get { return sheet; }
            set { 
                sheet = value;
                Initialize();
                OnPropertyChanged("Sheet"); 
            }
        }

        private Rect bounds;
        public Double Width { get { return bounds.Width; } }
        public Double Height { get { return bounds.Height; } }

        public Double SheetViewWidth { get { return bounds.Width / 2 - (Double)App.Current.Resources["RowHeaderWidth"]; } }
        public Double SheetViewHeight { get { return bounds.Height - (Double)App.Current.Resources["ColumnHeaderHeight"] * 2; } }

        private Double sheetWidth;
        public Double SheetWidth { get { return sheetWidth; } private set { sheetWidth = value; OnPropertyChanged("SheetWidth"); } }

        private Double sheetHeight;
        public Double SheetHeight { get { return sheetHeight; } private set { sheetHeight = value; OnPropertyChanged("SheetHeight"); } }
        
        private List<View.RowPresenter> rowPresenters = new List<View.RowPresenter>();
        public List<View.RowPresenter> RowPresenters { get { return rowPresenters; } }

        public Boolean IsIndexTooltipVisible { get; set; }
        public Double IndexTooltipY { get; set; }
        public String IndexTooltipContent { get; set; }

        private ObservableCollection<ViewModel.ColumnViewModel> columnViewModels = new ObservableCollection<ColumnViewModel>();
        public ObservableCollection<ViewModel.ColumnViewModel> ColumnViewModels { get { return columnViewModels; } }

        private List<ViewModel.RowViewModel> rowViewModels = new List<ViewModel.RowViewModel>();
        public List<ViewModel.RowViewModel> RowViewModels { get { return rowViewModels; } }

        private ViewModel.SummaryViewModel summaryViewModel;
        public ViewModel.SummaryViewModel SummaryViewModel { get { return summaryViewModel; } set { summaryViewModel = value; OnPropertyChanged("SummaryViewModel"); } }

        private ViewModel.RowHeaderViewModel rowHeaderViewModel;
        public ViewModel.RowHeaderViewModel RowHeaderViewModel { get { return rowHeaderViewModel; } set { rowHeaderViewModel = value; OnPropertyChanged("RowHeaderViewModel"); } }

        private Model.Column highlightedColumn;
        public Model.Column HighlightedColumn { get { return highlightedColumn; } set { highlightedColumn = value; OnPropertyChanged("HighlightedColumn"); } }

        public Double ScrollLeft { get; set; }
        public Double ScrollTop { get; set; }

        private Model.Column groupedColumn;

        public MainPageViewModel(IMainPage view)
        {
            this.view = view;
            SummaryViewModel = new ViewModel.SummaryViewModel(this);
            RowHeaderViewModel = new ViewModel.RowHeaderViewModel(this);
            
            bounds = Window.Current.Bounds;
            OnPropertyChanged("Width");
            OnPropertyChanged("Height");
            OnPropertyChanged("SheetViewWidth");
            OnPropertyChanged("SheetViewHeight");
        }

        public void Initialize()
        {
            SheetWidth = sheet.Columns.Select(c => c.Width).Sum() + (Double)App.Current.Resources["RowHeaderWidth"];
            SheetHeight = sheet.RowCount * (Double)App.Current.Resources["RowHeight"];

            /* 기본 컬럼 추가 */
            columnViewModels.Clear();
            foreach (Model.Column column in sheet.Columns)
            {
                columnViewModels.Add(new ViewModel.ColumnViewModel(this) { Column = column });
            }

            /* 기본 row 추가 */
            rowViewModels.Clear();
            foreach (Model.Row row in sheet.Rows)
            {
                ViewModel.RowViewModel rowViewModel = new ViewModel.RowViewModel(this) { Row = row };
                rowViewModels.Add(rowViewModel);

                View.RowPresenter rowPresenter = new View.RowPresenter(rowViewModel);
                rowPresenters.Add(rowPresenter);
                
                view.AddRow(rowPresenter);
                rowPresenter.Y = row.Y;
                rowPresenter.Update();
            }

            rowHeaderViewModel.SetMaximumRowNumber(sheet.RowCount);
        }

        public void GroupBy(Model.Column groupBy)
        {
            if (groupBy.Type != Model.ColumnType.Categorical)
            {
                Debug.WriteLine("Grouping rows by a numerical column is not supported now.");
                return;
            }

            groupedColumn = groupBy;

            /* 
             * 먼저 group by 컬럼을 맨 앞으로 
             * 두개 agg된 경우 생각 안하고 있음
             */

            IEnumerable<Model.Column> previousColumns = sheet.Columns.Where(c => c.Index < groupBy.Index);

            foreach (Model.Column column in previousColumns)
            {
                column.Index++;
            }
            groupBy.Index = 0;

            // Column의 x값을 업데이트하고 아래에서 Cell을 추가하므로 RowPresenter.UpdateCells() 를 호출하지 않아도 됨
            sheet.UpdateColumnX();

            
            foreach (Model.Bin bin in groupBy.Bins)
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

            foreach (Model.Bin bin in groupBy.Bins)
            {
                Model.Row row = new Model.Row();
                
                row.Index = bin.Index;
                foreach (Model.Column column in sheet.Columns)
                {
                    Model.Cell cell = new Model.Cell();

                    cell.Column = column; //Cell.Column은 OnPropertyChanged가 안됨.
                    if (column == groupBy)
                    {
                        cell.RawContent = bin.Name;
                        cell.Content = bin.Name;
                    }
                    else
                    {
                        Int32 index = sheet.Columns.IndexOf(column);
                        String aggr = Aggregate(column, bin.Rows.Select(r => r.Cells[index].Content), Model.AggregationType.Average);
                        cell.RawContent = aggr;
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

            rowHeaderViewModel.SetRowNumber(groupBy.Bins.Count);
            view.UpdateColumnHeaders();
            view.ScrollToColumn(groupBy);
        }

        public void ChangeAggregationType(Int32 columnIndex, Model.AggregationType aggregationType)
        {
            if (groupedColumn == null) return;

            Model.Column column = sheet.Columns[columnIndex];

            Int32 index = 0;
            foreach (Model.Bin bin in groupedColumn.Bins)
            {
                Model.Row row = rowViewModels[index].Row;

                String aggr = Aggregate(column, bin.Rows.Select(r => r.Cells[columnIndex].Content), aggregationType);
                row.Cells[columnIndex].RawContent = aggr;
                row.Cells[columnIndex].Content = aggr;
                index++;
            }
        }

        public String Aggregate(Model.Column column, IEnumerable<Object> values, Model.AggregationType aggregationType)
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
                        return Math.Round(values.Sum(v => (Double)v) / values.Count(), 2).ToString();
                    case Model.AggregationType.Maximum:
                        return values.Max().ToString();
                }
            }
            throw new Exception("Unknown Aggregation Type");
        }

        public void CancelGroupBy()
        {
            groupedColumn = null;

            /* 먼저 column의 순서를 원래대로 */

            for (Int32 i = 0; i < sheet.Columns.Count; ++i)
            {
                sheet.Columns[i].Index = i;
            }
            
            // index에 따라 컬럼 X 다시 계산
            sheet.UpdateColumnX();

            // 원래 있던 row들은 다 fadeout 시켜버림
            foreach (View.RowPresenter rowPresenter in rowPresenters)
            {
                rowPresenter.UpdateAndDestroy(delegate
                {
                    view.RemoveRow(rowPresenter);
                });
            }

            rowViewModels.Clear();
            rowPresenters.Clear();
            
            Int32 index = 0;
            foreach (Model.Row row in sheet.Rows)
            {
                row.Index = index++;
                ViewModel.RowViewModel rowViewModel = new ViewModel.RowViewModel(this) { Row = row };
                rowViewModels.Add(rowViewModel);

                View.RowPresenter rowPresenter = new View.RowPresenter(rowViewModel);
                rowPresenters.Add(rowPresenter);

                view.AddRow(rowPresenter);

                rowPresenter.Y = row.Y;
                //rowPresenter.Update();
            }
            
            rowHeaderViewModel.SetRowNumber(sheet.RowCount);

            // column header 움직이기
            view.UpdateColumnHeaders();
        }

        #region Draft

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

            foreach (Model.Row row in sorted)
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
            }
        }

        public void MarkColumnDisabled(Model.Column movingColumn)
        {
            if (!movingColumn.Enabled) return;

            movingColumn.Enabled = false;
            IEnumerable<Model.Column> nexts = sheet.Columns.Where(c => c.Index > movingColumn.Index);

            foreach (Model.Column column in nexts)
            {
                column.Index--;
            }
            movingColumn.Index = sheet.ColumnCount - 1;

            sheet.UpdateColumnX();

            foreach (View.RowPresenter rowPresenter in rowPresenters)
            {
                rowPresenter.UpdateCells();
            }

            view.UpdateColumnHeaders();
        }

        public void MarkColumnEnabled(Model.Column movingColumn)
        {
            if (movingColumn.Enabled) return;
            
            Int32 index = sheet.Columns.Count(c => c.Enabled);
            IEnumerable<Model.Column> nexts = sheet.Columns.Where(c => c.Index <= index && !c.Enabled);

            foreach (Model.Column column in nexts)
            {
                column.Index++;
            }
            movingColumn.Index = index;
            movingColumn.Enabled = true;

            sheet.UpdateColumnX();

            foreach (View.RowPresenter rowPresenter in rowPresenters)
            {
                rowPresenter.UpdateCells();
            }

            view.UpdateColumnHeaders();
        }


        Int32 indexedColumnIndex = -1;
        uint ignoredPointerId;
        uint activatedPointerId;

        public void IndexColumn(uint id, Double y)
        {
            if (ignoredPointerId == id) return;

            Double totalHeight = SheetViewHeight;
            Int32 columnIndex = (Int32)Math.Floor(y / totalHeight * sheet.ColumnCount);

            if (columnIndex < 0 || columnIndex >= sheet.ColumnCount) return;

            if (indexedColumnIndex != columnIndex)
            {
                Model.Column indexedColumn = sheet.Columns.First(c => c.Index == columnIndex);
                view.ScrollToColumn(indexedColumn);

                IsIndexTooltipVisible = true;
                IndexTooltipY = (columnIndex + 0.5) * (totalHeight / sheet.ColumnCount) - 15;
                IndexTooltipContent = indexedColumn.Name;
                OnPropertyChanged("IsIndexTooltipVisible");
                OnPropertyChanged("IndexTooltipY");
                OnPropertyChanged("IndexTooltipContent");

                HighlightColumn(indexedColumn);
            }
            indexedColumnIndex = columnIndex;
            activatedPointerId = id;
        }

        public void CancelIndexing()
        {
            foreach (Model.Column column in sheet.Columns) column.Highlighted = false;

            IsIndexTooltipVisible = false;
            OnPropertyChanged("IsIndexTooltipVisible");
            HighlightedColumn = null;
            indexedColumnIndex = -1;
            summaryViewModel.Hide();

            ignoredPointerId = activatedPointerId;
        }

        public void HighlightColumn(Model.Column column)
        {
            foreach (Model.Column c in sheet.Columns) c.Highlighted = false;
            column.Highlighted = true;

            summaryViewModel.IsSelected = false; // reset selected because the selected column changed
            summaryViewModel.ShowSummary(column);
            HighlightedColumn = column;
        }

        public void UnhighlightColumn(Model.Column column)
        {
            column.Highlighted = false;
            indexedColumnIndex = -1;
            summaryViewModel.Hide();
            HighlightedColumn = null;
        }



        public void UpdateFiltering()
        {
            foreach (Model.Row row in sheet.Rows)
            {
                row.IsFilteredOut = false;
            }

            foreach (Model.Column column in Sheet.Columns)
            {
                foreach (Model.Bin bin in column.Bins.Where(b => b.IsFilteredOut))
                {
                    foreach (Model.Row row in bin.Rows)
                    {
                        row.IsFilteredOut = true;
                    }
                }
            }

            Int32 index = 0;
            foreach (Model.Row row in sheet.Rows)
            {
                if (!row.IsFilteredOut)
                {
                    row.Index = index++;
                }
            }

            foreach (Model.Row row in sheet.Rows)
            {
                row.OnPropertyChanged("Y");
                row.OnPropertyChanged("IsFilteredOut");
            }

            foreach (View.RowPresenter rowPresenter in rowPresenters)
            {
                rowPresenter.Update();
            }
        }

        #endregion
    }
}
