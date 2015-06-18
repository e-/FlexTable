﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace FlexTable.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
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

        private ObservableCollection<Model.RowHeader> rowHeaderItems = new ObservableCollection<Model.RowHeader>();
        public ObservableCollection<Model.RowHeader> RowHeaderItems { get { return rowHeaderItems; } }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private List<View.RowPresenter> rowPresenters = new List<View.RowPresenter>();
        public List<View.RowPresenter> RowPresenters { get { return rowPresenters; } }

        public Boolean IsIndexTooltipVisible { get; set; }
        public Double IndexTooltipY { get; set; }
        public String IndexTooltipContent { get; set; }

        public ObservableCollection<ViewModel.ColumnViewModel> columnViewModels = new ObservableCollection<ColumnViewModel>();
        public ObservableCollection<ViewModel.ColumnViewModel> ColumnViewModels { get { return columnViewModels; } }

        private ViewModel.SummaryViewModel summaryViewModel;
        public ViewModel.SummaryViewModel SummaryViewModel { get { return summaryViewModel; } set { summaryViewModel = value; OnPropertyChanged("SummaryViewModel"); } }

        private Model.Column highlightedColumn;
        public Model.Column HighlightedColumn { get { return highlightedColumn; } set { highlightedColumn = value; OnPropertyChanged("HighlightedColumn"); } }

        public Double ScrollLeft { get; set; }

        public MainPageViewModel(IMainPage view)
        {
            this.view = view;
            SummaryViewModel = new ViewModel.SummaryViewModel(this);
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
            rowHeaderItems.Clear();
            for (Int32 i = 1; i <= sheet.RowCount; ++i)
            {
                rowHeaderItems.Add(new Model.RowHeader() { Index = i });
            }
            columnViewModels.Clear();
            foreach (Model.Column column in sheet.Columns)
            {
                columnViewModels.Add(new ViewModel.ColumnViewModel(this) { Column = column });
            }
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
        public void IndexColumn(Double y)
        {
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
        }

        public void HighlightColumn(Model.Column column)
        {
            foreach (Model.Column c in sheet.Columns) c.Highlighted = false;
            column.Highlighted = true;

            summaryViewModel.ShowSummary(column);
            HighlightedColumn = column;
        }

        public void UnhighlightColumn(Model.Column column)
        {
            column.Highlighted = false;
        }

        public void CancelIndexing()
        {
            foreach (Model.Column column in sheet.Columns) column.Highlighted = false;

            IsIndexTooltipVisible = false;
            OnPropertyChanged("IsIndexTooltipVisible");
            HighlightedColumn = null;
            indexedColumnIndex = -1;
        }
    }
}
