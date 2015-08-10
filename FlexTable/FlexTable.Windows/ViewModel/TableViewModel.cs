using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;

namespace FlexTable.ViewModel
{
    public class TableViewModel : NotifyViewModel
    {
        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel
        {
            get { return mainPageViewModel; }
        }
        public SheetViewModel SheetViewModel
        {
            get { return mainPageViewModel.SheetViewModel; }
        }

        private IMainPage view;

        public Double ScrollLeft { get; set; }
        public Double ScrollTop { get; set; }

        private Rect bounds;
        public Double Width { get { return bounds.Width; } }
        public Double Height { get { return bounds.Height; } }

        public Double SheetViewWidth { get { return bounds.Width / 2 - (Double)App.Current.Resources["RowHeaderWidth"]; } }
        public Double SheetViewHeight { get { return bounds.Height - (Double)App.Current.Resources["ColumnHeaderHeight"] * 2; } }

        private Double paddedSheetWidth;
        public Double PaddedSheetWidth { get { return paddedSheetWidth; } set { paddedSheetWidth = value; OnPropertyChanged("PaddedSheetWidth"); } }

        private Double paddedSheetHeight;
        public Double PaddedSheetHeight { get { return paddedSheetHeight; } set { paddedSheetHeight = value; OnPropertyChanged("PaddedSheetHeight"); } }

        private List<View.RowPresenter> allRowPresenters = new List<View.RowPresenter>();
        private List<View.RowPresenter> rowPresenters = new List<View.RowPresenter>();
        //public List<View.RowPresenter> RowPresenters { get { return rowPresenters; } }

        private Boolean isIndexTooltipVisible;
        public Boolean IsIndexTooltipVisible { get { return isIndexTooltipVisible; } set { isIndexTooltipVisible = value; OnPropertyChanged("IsIndexTooltipVisible"); } }

        private Double indexTooltipY;
        public Double IndexTooltipY { get { return indexTooltipY; } set { indexTooltipY = value; OnPropertyChanged("IndexTooltipY"); } }

        private String indexTooltipContent;
        public String IndexTooltipContent { get { return indexTooltipContent; } set { indexTooltipContent = value; OnPropertyChanged("IndexTooltipContent"); } }

        private ColumnViewModel indexedColumnViewModel;
        public ColumnViewModel IndexedColumnViewModel { get { return indexedColumnViewModel; } set { indexedColumnViewModel = value; OnPropertyChanged("IndexedColumnViewModel"); } }

        private List<ViewModel.RowViewModel> rowViewModels;
        public List<ViewModel.RowViewModel> RowViewModels { get { return rowViewModels; } }

        public TableViewModel(ViewModel.MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.view = view;

            bounds = Window.Current.Bounds;
            OnPropertyChanged("Width");
            OnPropertyChanged("Height");
            OnPropertyChanged("SheetViewWidth");
            OnPropertyChanged("SheetViewHeight");
        }

        public void Initialize()
        {
            // guideline 추가
            view.TableView.AddGuidelines(SheetViewModel.Sheet.Rows.Count);

            // 최대 row header 추가
            view.TableView.RowHeaderPresenter.SetMaximumRowNumber(SheetViewModel.AllRowViewModels.Count);
        }

        public void CreateAllRows()
        {
            PaddedSheetHeight = SheetViewModel.AllRowsSheetHeight > SheetViewHeight ? SheetViewModel.AllRowsSheetHeight : SheetViewHeight;
            PaddedSheetWidth = SheetViewModel.SheetWidth > SheetViewWidth ? SheetViewModel.SheetWidth : SheetViewWidth;

            foreach (ViewModel.RowViewModel rowViewModel in SheetViewModel.AllRowViewModels)
            {
                View.RowPresenter rowPresenter = new View.RowPresenter(rowViewModel);

                view.TableView.AllRowsTableCanvas.Children.Add(rowPresenter);
                rowPresenter.Y = rowViewModel.Y;
                rowPresenter.Update();

                allRowPresenters.Add(rowPresenter);
            }

            rowViewModels = SheetViewModel.AllRowViewModels;

            view.TableView.RowHeaderPresenter.SetRowNumber(SheetViewModel.AllRowViewModels.Count);
        }

        public void UpdateRows()
        {
            if (mainPageViewModel.ExplorationViewModel.SelectedColumnViewModels.Count == 0) // 아무 것도 선택되지 않으면 모든 로우 보여줘야함.
            {
                rowViewModels = SheetViewModel.AllRowViewModels;

                foreach (View.RowPresenter rowPresenter in allRowPresenters)
                {
                    rowPresenter.Visibility = Visibility.Visible;
                    rowPresenter.Y = rowPresenter.RowViewModel.Y;
                    rowPresenter.UpdateCellsWithoutAnimation();
                }

                view.TableView.ShowAllRowsCanvas();
                
                PaddedSheetHeight = SheetViewModel.AllRowsSheetHeight > SheetViewHeight ? SheetViewModel.AllRowsSheetHeight : SheetViewHeight;

                view.TableView.RowHeaderPresenter.SetRowNumber(rowViewModels.Count);
            }
            else
            {
                rowViewModels = SheetViewModel.TemporaryRowViewModels;

                view.TableView.ShowTableCanvas();

                rowPresenters.Clear();
                view.TableView.TableCanvas.Children.Clear();

                PaddedSheetHeight = SheetViewModel.SheetHeight > SheetViewHeight ? SheetViewModel.SheetHeight : SheetViewHeight;

                foreach (ViewModel.RowViewModel rowViewModel in SheetViewModel.TemporaryRowViewModels)
                {
                    View.RowPresenter rowPresenter = new View.RowPresenter(rowViewModel);
                    rowPresenters.Add(rowPresenter);

                    view.TableView.TableCanvas.Children.Add(rowPresenter);
                    rowPresenter.Y = rowViewModel.Y;
                    rowPresenter.Update();
                }

                view.TableView.RowHeaderPresenter.SetRowNumber(rowViewModels.Count);
            }
        }

        uint ignoredPointerId;
        uint activatedPointerId; // for cancel indexing

        public void IndexColumn(uint id, Double y)
        {
            if (ignoredPointerId == id) return;

            Double totalHeight = SheetViewHeight;
            Int32 columnIndex = (Int32)Math.Floor(y / totalHeight * SheetViewModel.ColumnViewModels.Count);

            if (columnIndex < 0 || columnIndex >= SheetViewModel.ColumnViewModels.Count) return;

            ColumnViewModel columnViewModel = SheetViewModel.ColumnViewModels.First(c => c.Order == columnIndex);

            if (indexedColumnViewModel != columnViewModel)
            {
                view.TableView.ScrollToColumnViewModel(columnViewModel);

                IndexedColumnViewModel = columnViewModel;

                IsIndexTooltipVisible = true;
                IndexTooltipY = (columnIndex + 0.5) * (totalHeight / SheetViewModel.ColumnViewModels.Count) - 15;
                IndexTooltipContent = columnViewModel.Column.Name;

                mainPageViewModel.ExplorationViewModel.ShowSummary(columnViewModel);
            }

            activatedPointerId = id;
        }

        public void CancelIndexing()
        {
            IsIndexTooltipVisible = false;
            IndexedColumnViewModel = null;
            view.ExplorationView.TopPageViewModel.Hide();

            ignoredPointerId = activatedPointerId;
        }

        public void PreviewRows(ColumnViewModel columnViewModel, Model.Category category)
        {
            Int32 index = 0;
            Double rowHeight = (Double)App.Current.Resources["RowHeight"];
            foreach (View.RowPresenter rowPresenter in allRowPresenters)
            {
                if (rowPresenter.RowViewModel.Cells[columnViewModel.Index].Content == category)
                {
                    rowPresenter.Visibility = Visibility.Visible;
                    rowPresenter.Y = (index++) * rowHeight;
                    rowPresenter.UpdateCellsWithoutAnimation();
                }
                else
                {
                    rowPresenter.Visibility = Visibility.Collapsed;
                }
            }

            Double sheetHeight = index * rowHeight;
            PaddedSheetHeight = sheetHeight > SheetViewHeight ? sheetHeight : SheetViewHeight;
            view.TableView.RowHeaderPresenter.SetRowNumber(index);
            view.TableView.ShowAllRowsCanvas();
        }

        public void CancelPreviewRows()
        {
            PaddedSheetHeight = SheetViewModel.SheetHeight > SheetViewHeight ? SheetViewModel.SheetHeight : SheetViewHeight;
            view.TableView.RowHeaderPresenter.SetRowNumber(rowViewModels.Count);
            view.TableView.ShowTableCanvas();
        }

/*        public void MarkColumnDisabled(Model.Column movingColumn)
        {
            if (!movingColumn.Enabled) return;

            movingColumn.Enabled = false;
            IEnumerable<Model.Column> nexts = sheet.Columns.Where(c => c.Index > movingColumn.Index);

            foreach (Model.Column column in nexts)
            {
                column.Index--;
            }
            movingColumn.Index = sheet.Columns.Count - 1;

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
        */
    }
}
