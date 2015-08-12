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
using Windows.UI.Xaml.Media.Animation;

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
        public List<View.RowPresenter> AllRowPresenters { get { return allRowPresenters; } }

        private List<View.RowPresenter> temporaryRowPresenters = new List<View.RowPresenter>();
        public List<View.RowPresenter> TemporaryRowPresenters { get { return temporaryRowPresenters; } }

        private List<View.RowPresenter> rowPresenters;
        public List<View.RowPresenter> RowPresenters { get { return rowPresenters; } }

        private Boolean isIndexing;
        public Boolean IsIndexing { get { return isIndexing; } set { isIndexing = value; OnPropertyChanged("IsIndexing"); } }

        private Double indexTooltipY;
        public Double IndexTooltipY { get { return indexTooltipY; } set { indexTooltipY = value; OnPropertyChanged("IndexTooltipY"); } }

        private String indexTooltipContent;
        public String IndexTooltipContent { get { return indexTooltipContent; } set { indexTooltipContent = value; OnPropertyChanged("IndexTooltipContent"); } }

        private ColumnViewModel indexedColumnViewModel;
        public ColumnViewModel IndexedColumnViewModel { get { return indexedColumnViewModel; } set { indexedColumnViewModel = value; OnPropertyChanged("IndexedColumnViewModel"); } }

        private Boolean isPreviewing = false;

        private List<ViewModel.RowViewModel> rowViewModels;
        public List<ViewModel.RowViewModel> RowViewModels { get { return rowViewModels; } }

        private ColumnViewModel sortBy;
        public ColumnViewModel SortBy { get { return sortBy; } set { sortBy = value; } }

        private Model.SortOption sortOption;
        public Model.SortOption SortOption { get { return sortOption; } set { sortOption = value; } }

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

            rowPresenters = allRowPresenters;
            rowViewModels = SheetViewModel.AllRowViewModels;

            view.TableView.RowHeaderPresenter.SetRowNumber(SheetViewModel.AllRowViewModels.Count);
        }

        public void UpdateRows()
        {
            if (mainPageViewModel.ExplorationViewModel.SelectedColumnViewModels.Count == 0) // 아무 것도 선택되지 않으면 모든 로우 보여줘야함.
            {
                rowViewModels = SheetViewModel.AllRowViewModels;
            }
            else
            {
                rowViewModels = SheetViewModel.TemporaryRowViewModels;
            }

            if (sortBy != null)
            {
                IOrderedEnumerable<RowViewModel> sorted = null;
                switch (sortOption)
                {
                    case Model.SortOption.Ascending:
                        sorted = rowViewModels.OrderBy(
                            r => r.Cells[sortBy.Index].Content is Model.Category ? r.Cells[sortBy.Index].Content.ToString() : r.Cells[sortBy.Index].Content
                            );
                        break;
                    case Model.SortOption.Descending:
                        sorted = rowViewModels.OrderByDescending(
                            r => r.Cells[sortBy.Index].Content is Model.Category ? r.Cells[sortBy.Index].Content.ToString() : r.Cells[sortBy.Index].Content
                            );
                        break;
                }

                Int32 index = 0;
                foreach (RowViewModel rowViewModel in sorted)
                {
                    rowViewModel.Index = index++;
                }
            }
            else
            {
                Int32 index = 0;
                foreach (RowViewModel rowViewModel in rowViewModels)
                {
                    rowViewModel.Index = index++;
                }
            }


            if (mainPageViewModel.ExplorationViewModel.SelectedColumnViewModels.Count == 0) // 아무 것도 선택되지 않으면 모든 로우 보여줘야함.
            {
                rowPresenters = allRowPresenters;

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
                view.TableView.ShowTableCanvas();
                rowPresenters = temporaryRowPresenters;
                temporaryRowPresenters.Clear();
                view.TableView.TableCanvas.Children.Clear();

                PaddedSheetHeight = SheetViewModel.SheetHeight > SheetViewHeight ? SheetViewModel.SheetHeight : SheetViewHeight;

                foreach (ViewModel.RowViewModel rowViewModel in SheetViewModel.TemporaryRowViewModels)
                {
                    View.RowPresenter rowPresenter = new View.RowPresenter(rowViewModel);
                    temporaryRowPresenters.Add(rowPresenter);

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
            if (isPreviewing) return;
            if (ignoredPointerId == id) return;

            Double totalHeight = SheetViewHeight;
            Int32 columnIndex = (Int32)Math.Floor(y / totalHeight * SheetViewModel.ColumnViewModels.Count);

            if (columnIndex < 0 || columnIndex >= SheetViewModel.ColumnViewModels.Count) return;

            ColumnViewModel columnViewModel = SheetViewModel.ColumnViewModels.First(c => c.Order == columnIndex);

            if (indexedColumnViewModel != columnViewModel)
            {
                view.TableView.ScrollToColumnViewModel(columnViewModel);

                IndexedColumnViewModel = columnViewModel;

                IsIndexing = true;
                IndexTooltipY = (columnIndex + 0.5) * (totalHeight / SheetViewModel.ColumnViewModels.Count) - 15;
                IndexTooltipContent = columnViewModel.Column.Name;

                mainPageViewModel.ExplorationViewModel.ShowSummary(columnViewModel);
            }

            activatedPointerId = id;
        }

        public void CancelIndexing()
        {
            IsIndexing = false;
            IndexedColumnViewModel = null;
            view.ExplorationView.TopPageViewModel.Hide();

            ignoredPointerId = activatedPointerId;
        }

        public void PreviewRows(ColumnViewModel columnViewModel, Model.Category category)
        {
            isPreviewing = true;

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
            isPreviewing = false;
            PaddedSheetHeight = SheetViewModel.SheetHeight > SheetViewHeight ? SheetViewModel.SheetHeight : SheetViewHeight;
            view.TableView.RowHeaderPresenter.SetRowNumber(rowViewModels.Count);
            view.TableView.ShowTableCanvas();
            view.TableView.TableScrollViewer.ChangeView(null, 0, null);
        }

        public void UpdateCellXPosition()
        {
            Storyboard sb = new Storyboard();

            foreach (ColumnViewModel columnViewModel in SheetViewModel.ColumnViewModels)
            {
                if (columnViewModel.IsXDirty)
                {
                    columnViewModel.IsXDirty = false;

                    foreach(View.RowPresenter rowPresenter in rowPresenters) {
                        
                        sb.Children.Add(
                            Util.Animator.Generate(rowPresenter.CellPresenters[columnViewModel.Index], "(Canvas.Left)", columnViewModel.X)
                            );
                    }
                }
            }

            sb.Begin();
        }

        public void Sort(ColumnViewModel columnViewModel, Model.SortOption sortOption)
        {
            sortBy = columnViewModel;
            this.sortOption = sortOption;

            UpdateRows();
        }
    }
}
