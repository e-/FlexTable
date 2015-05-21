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

        public MainPageViewModel(IMainPage view)
        {
            this.view = view;
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
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShuffleRows()
        {
            Random random = new Random();
            for (Int32 i = 0; i < 1000; ++i)
            {
                Int32 x = random.Next(sheet.RowCount), y = random.Next(sheet.RowCount);
                Int32 temp;
                temp = sheet.Rows[x].Index;
                sheet.Rows[x].Index = sheet.Rows[y].Index;
                sheet.Rows[y].Index = temp;
            }

            foreach (Model.Row row in sheet.Rows)
            {
                row.OnPropertyChanged("Y");
            }

            foreach(View.RowPresenter rowPresenter in rowPresenters) {
                rowPresenter.Update();
            }
        }

        public void ShuffleColumns()
        {
            Random random = new Random();
            for (Int32 i = 0; i < 1000; ++i)
            {
                Int32 x = random.Next(sheet.ColumnCount), y = random.Next(sheet.ColumnCount);
                Int32 temp;
                temp = sheet.Columns[x].Index;
                sheet.Columns[x].Index = sheet.Columns[y].Index;
                sheet.Columns[y].Index = temp;
            }

            sheet.UpdateColumnX();

            foreach (View.RowPresenter rowPresenter in rowPresenters)
            {
                rowPresenter.UpdateCells();
            }
        }

        public void Sort(Int32 sortIndex, Boolean isDescending)
        {
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

        public void MoveColumnToLast(Model.Column movingColumn)
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
        }

        public void MoveColumnToEnabled(Model.Column movingColumn)
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
        }

        public void GoToColumn(Double y)
        {
            Double totalHeight = SheetViewHeight;
            Int32 columnIndex = (Int32)Math.Floor(y / totalHeight * sheet.ColumnCount);

            if (columnIndex < 0 || columnIndex >= sheet.ColumnCount) return;
            foreach (Model.Column column in sheet.Columns) column.Highlighted = false;
            sheet.Columns[columnIndex].Highlighted = true;
            view.ScrollToColumn(sheet.Columns[columnIndex]);

            IsIndexTooltipVisible = true;
            IndexTooltipY = (columnIndex + 0.5) * (totalHeight / sheet.ColumnCount) - 15;
            IndexTooltipContent = sheet.Columns[columnIndex].Name;
            OnPropertyChanged("IsIndexTooltipVisible");
            OnPropertyChanged("IndexTooltipY");
            OnPropertyChanged("IndexTooltipContent");
        }

        public void CancelIndexing()
        {
            foreach (Model.Column column in sheet.Columns) column.Highlighted = false;

            IsIndexTooltipVisible = false;
            OnPropertyChanged("IsIndexTooltipVisible");
        }
    }
}
