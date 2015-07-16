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
        public Model.Sheet Sheet {
            get { return sheet; }
            set { 
                sheet = value;
                OnPropertyChanged("Sheet"); 
            }
        }

        private SheetViewModel sheetViewModel;
        public SheetViewModel SheetViewModel { get { return sheetViewModel; } set { sheetViewModel = value; OnPropertyChanged("SheetViewModel"); } }

        private SummaryViewModel summaryViewModel;
        public SummaryViewModel SummaryViewModel { get { return summaryViewModel; } set { summaryViewModel = value; OnPropertyChanged("SummaryViewModel"); } }

        private ChartViewModel chartViewModel;
        public ChartViewModel ChartViewModel { get { return chartViewModel; } set { chartViewModel = value; OnPropertyChanged("ChartViewModel"); } }

        private TableViewModel tableViewModel;
        public TableViewModel TableViewModel { get { return tableViewModel; } set { tableViewModel = value; OnPropertyChanged("TableViewModel"); } }
       
        public MainPageViewModel(IMainPage view)
        {
            this.view = view;

            SummaryViewModel = new SummaryViewModel(this);            
            ChartViewModel = new ChartViewModel(this);
            SheetViewModel = new SheetViewModel(this, view);
            TableViewModel = new TableViewModel(this, view);
        }

        public void Initialize()
        {
            sheetViewModel.Initialize(sheet);
            tableViewModel.UpdateRows();
            view.TableView.AddGuidelines(sheet.Rows.Count);
        }
     

        private Model.Column chartedColumn;
        private Int32 chartedColumnIndex;

        public void DrawChart(Int32 columnIndex)
        {
            /*Model.Column column = sheet.Columns[columnIndex];

            if(column.Type != Model.ColumnType.Numerical) return;

            if (chartedColumn != null) chartedColumn.IsDrawnOnChart = false;
            column.IsDrawnOnChart = true;
            chartedColumn = column;
            chartedColumnIndex = columnIndex;

            chartViewModel.Draw(
                groupedColumn, 
                rowViewModels.Select((rvm, index) => 
                    new Tuple<String, Double>(groupedColumn.Bins[index].Name, (Double)rvm.Row.Cells[columnIndex].Content)
                    ),
                column
            );*/
        }

        public void CancelGroupBy()
        {
            /*GroupedColumn.IsGroupedBy = false;
            GroupedColumn = null;

            // 먼저 column의 순서를 원래대로 

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
            
            rowHeaderViewModel.SetRowNumber(sheet.Rows.Count);

            // column header 움직이기
            view.UpdateColumnHeaders();*/
        }        

        public void HighlightColumn(Model.Column column)
        {
            /*foreach (Model.Column c in sheet.Columns) c.Highlighted = false;
            column.Highlighted = true;

            summaryViewModel.IsSelected = false; // reset selected because the selected column changed
            summaryViewModel.ShowSummary(column);
            HighlightedColumn = column;*/
        }

        public void UnhighlightColumn(Model.Column column)
        {
            /*column.Highlighted = false;
            indexedColumnIndex = -1;
            summaryViewModel.Hide();
            HighlightedColumn = null;*/
        }

        public void UpdateFiltering()
        {
            /*foreach (Model.Row row in sheet.Rows)
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
            }*/
        }
    }
}
