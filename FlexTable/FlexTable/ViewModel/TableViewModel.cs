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
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using FlexTable.View;
using FlexTable.Model;
using System.Threading;
using Windows.UI.Core;
using System.Collections.ObjectModel;

namespace FlexTable.ViewModel
{
    public class TableViewModel : NotifyViewModel
    {
        public enum TableViewState { AllRow, GroupedRow, SelectedRow};

        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;
        public SheetViewModel SheetViewModel => mainPageViewModel.SheetViewModel;

        private IMainPage view;

        public Double ScrollLeft { get; set; }
        public Double ScrollTop { get; set; }
       
        public Double Width => mainPageViewModel.Bounds.Width;
        public Double Height => mainPageViewModel.Bounds.Height;

        public Double SheetViewWidth => mainPageViewModel.Bounds.Width / 2 - (Double)App.Current.Resources["RowHeaderWidth"];
        public Double SheetViewHeight => mainPageViewModel.Bounds.Height - (Double)App.Current.Resources["ColumnHeaderHeight"] * 2;

        private Double paddedSheetWidth;
        public Double PaddedSheetWidth { get { return paddedSheetWidth; } set { paddedSheetWidth = value; OnPropertyChanged(nameof(PaddedSheetWidth)); } }

        private Double paddedSheetHeight;
        public Double PaddedSheetHeight { get { return paddedSheetHeight; } set { paddedSheetHeight = value; OnPropertyChanged(nameof(PaddedSheetHeight)); } }
        
        private ObservableCollection<RowViewModel> allRowViewModels;
        public ObservableCollection<RowViewModel> AllRowViewModels
        {
            get { return allRowViewModels; }
            set { allRowViewModels = value; OnPropertyChanged(nameof(AllRowViewModels)); }
        }

        private ObservableCollection<RowViewModel> groupedRowViewModels;
        public ObservableCollection<RowViewModel> GroupedRowViewModels
        {
            get { return groupedRowViewModels; }
            set { groupedRowViewModels = value; OnPropertyChanged(nameof(GroupedRowViewModels)); }
        }

        /*private ObservableCollection<RowViewModel> selectedRowViewModels;
        public ObservableCollection<RowViewModel> SelectedRowViewModels
        {
            get { return selectedRowViewModels; }
            set { selectedRowViewModels = value; OnPropertyChanged(nameof(SelectedRowViewModels)); }
        }*/
        
        private TableViewState oldState = TableViewState.AllRow;
        public TableViewState OldState { get { return oldState; } }

        private TableViewState state = TableViewState.AllRow;
        public TableViewState State { get { return state; } set { oldState = state; state = value; } }

        private Boolean isIndexing;
        public Boolean IsIndexing { get { return isIndexing; } set { isIndexing = value; OnPropertyChanged("IsIndexing"); } }
        
        private ColumnViewModel sortBy;
        public ColumnViewModel SortBy { get { return sortBy; } set { sortBy = value; } }

        private SortOption sortOption;
        public SortOption SortOption { get { return sortOption; } set { sortOption = value; } }

        public IEnumerable<Row> SelectedRows { get; set; } = null;

        public TableViewModel(MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.view = view;

            OnPropertyChanged("Width");
            OnPropertyChanged("Height");
            OnPropertyChanged("SheetViewWidth");
            OnPropertyChanged("SheetViewHeight");
        }

        public void Initialize()
        {
            // 최대 row header 추가
            view.TableView.GuidelinePresenter.SetMaximumRowNumber(SheetViewModel.AllRowViewModels.Count);
            view.TableView.RowHeaderPresenter.SetRowMaximumNumber(SheetViewModel.AllRowViewModels.Count);

            view.TableView.TopColumnHeader.Update();
            view.TableView.BottomColumnHeader.Update();

            PaddedSheetHeight = Math.Max(SheetViewModel.AllRowsSheetHeight, SheetViewHeight);
            PaddedSheetWidth = Math.Max(SheetViewModel.SheetWidth, SheetViewWidth);

            AllRowViewModels = new ObservableCollection<RowViewModel>(SheetViewModel.AllRowViewModels);

            view.TableView.Initialize();
            view.TableView.ReflectState();

            view.TableView.ColumnIndexer.Update();
        }

        DispatcherTimer dispatcherTimer = null;
        const Double DeferredReflectionTimeInMS = 1;
        public void Reflect(ViewStatus viewStatus)
        {
            if (dispatcherTimer != null && dispatcherTimer.IsEnabled) dispatcherTimer.Stop();
            dispatcherTimer = new DispatcherTimer();

            dispatcherTimer.Tick += (sender, e) =>
            {
                dispatcherTimer.Stop();
                Update(viewStatus);
                
                // column header 업데이트
                foreach (ColumnViewModel cvm in mainPageViewModel.SheetViewModel.ColumnViewModels)
                {
                    cvm.IsSelected = viewStatus.SelectedColumnViewModels.IndexOf(cvm) >= 0;
                    cvm.UpdateHeaderName();
                }

                view.TableView.TopColumnHeader.Update();
                view.TableView.BottomColumnHeader.Update();
            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(DeferredReflectionTimeInMS);
            dispatcherTimer.Start();
        }
        
        void Update(ViewStatus viewStatus)
        {
            if(SelectedRows != null) // 로우 선택 중
            {
                //List<RowViewModel> rvms = new List<RowViewModel>();

                /*foreach (Row row in selectedRows)
                {
                    RowViewModel rowViewModel = new RowViewModel(mainPageViewModel) { Row = row };
                    foreach (Cell cell in row.Cells.OrderBy(c => c.ColumnViewModel.Order))
                    {
                        rowViewModel.Cells.Add(cell);
                    }
                    rvms.Add(rowViewModel);
                }*/

                //SelectedRowViewModels = new ObservableCollection<RowViewModel>(rvms);


                State = TableViewState.SelectedRow;
            }
            else if (viewStatus.SelectedColumnViewModels.Count == 0 ||
                viewStatus.SelectedColumnViewModels.Count >= 2 && viewStatus.SelectedColumnViewModels.Count(s => s.Type != ColumnType.Numerical) == 0 ||
                viewStatus.SelectedColumnViewModels.Count == 3 && viewStatus.SelectedColumnViewModels.Count(s => s.Type == ColumnType.Numerical) == 2
            ) // 아무 것도 선택되지 않으면 모든 로우 보여줘야함.
            {
                // 근데 어차피 이건 바뀌지 않으므로 대입해서 업데이트 할 필요도 없음
                State = TableViewState.AllRow;
            }
            else 
            {
                Boolean isDirty = false;
                if (GroupedRowViewModels == null) isDirty = true;
                else if (GroupedRowViewModels.Count != SheetViewModel.GroupedRowViewModels.Count) isDirty = true;
                else
                {
                    Int32 count = GroupedRowViewModels.Count, i;
                    for (i = 0; i < count; ++i)
                    {
                        if (GroupedRowViewModels[i] != SheetViewModel.GroupedRowViewModels[i])
                        {
                            isDirty = true;
                            break;
                        }
                    }
                }

                if(isDirty)
                    GroupedRowViewModels = new ObservableCollection<RowViewModel>(SheetViewModel.GroupedRowViewModels);

                State = TableViewState.GroupedRow;
            }


            view.TableView.ReflectState();

            /*if (sortBy != null)
            {
                IOrderedEnumerable<RowViewModel> sorted = null;
                switch (sortOption)
                {
                    case SortOption.Ascending:
                        sorted = rowViewModels.OrderBy(
                            r => r.Cells[sortBy.Index].Content is Category ? r.Cells[sortBy.Index].Content.ToString() : r.Cells[sortBy.Index].Content
                            );
                        break;
                    case SortOption.Descending:
                        sorted = rowViewModels.OrderByDescending(
                            r => r.Cells[sortBy.Index].Content is Category ? r.Cells[sortBy.Index].Content.ToString() : r.Cells[sortBy.Index].Content
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
            }*/

           
            /*
            if (SheetViewModel.IsAllRowsVisible) // 아무 것도 선택되지 않으면 모든 로우 보여줘야함.
            {
                rowPresenters = allRowPresenters;

                foreach (RowPresenter rowPresenter in allRowPresenters)
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
                view.TableView.ShowGroupByTableCanvas();
                rowPresenters = groupByRowPresenters;

                PaddedSheetHeight = SheetViewModel.SheetHeight > SheetViewHeight ? SheetViewModel.SheetHeight : SheetViewHeight;

                Int32 index = 0;

                foreach (RowViewModel rowViewModel in SheetViewModel.GroupByRowViewModels)
                {
                    RowPresenter rowPresenter = groupByRowPresenters[index];
                    rowPresenter.DataContext = rowViewModel;
                    rowPresenter.Visibility = Visibility.Visible;

                    rowPresenter.Y = rowViewModel.Y;
                    rowPresenter.Update();

                    index++;
                }

                for(;index < groupByRowPresenters.Count; ++index)
                {
                    groupByRowPresenters[index].Visibility = Visibility.Collapsed;
                }

                view.TableView.RowHeaderPresenter.SetRowNumber(rowViewModels.Count);
            }    */       
        }

        uint ignoredPointerId;
        uint activatedPointerId; // for cancel indexing

        public void IndexColumn(uint id, Int32 order) //Double y)
        {
            if (state == TableViewState.SelectedRow) return;
            if (ignoredPointerId == id) return;

            Double totalHeight = SheetViewHeight;
            Int32 columnIndex = order; // (Int32)Math.Floor(y / totalHeight * SheetViewModel.ColumnViewModels.Count);

            if (columnIndex < 0 || columnIndex >= SheetViewModel.ColumnViewModels.Count) return;

            ColumnViewModel columnViewModel = SheetViewModel.ColumnViewModels.First(c => c.Order == columnIndex);

            if (view.TableView.ColumnHighlighter.ColumnViewModel != columnViewModel)
            {
                view.TableView.ScrollToColumnViewModel(columnViewModel);

                view.TableView.ColumnHighlighter.ColumnViewModel = columnViewModel;
                view.TableView.ColumnHighlighter.Update();

                IsIndexing = true;

                mainPageViewModel.ExplorationViewModel.PreviewColumn(columnViewModel);
            }

            activatedPointerId = id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancelPreviewing">cancelIndexing이 호출되는 경우는 1) 컬럼을 선택할 때 처럼 단순히 하이라이터를 숨기고 싶은 경우 2) 소팅 처럼 하이라이터를 숨길 뿐만 아니라 프리뷰도 그만둘 경우. 
        /// 전자의 경우 프리뷰를 취소할 필요는 없으므로 false가 되고 후자의 경우 프리뷰도 그만둬야 하므로 true를 넘김
        /// </param>
        public void CancelIndexing(bool cancelPreviewing)
        {
            IsIndexing = false;

            view.TableView.ColumnHighlighter.ColumnViewModel = null;
            view.TableView.ColumnHighlighter.Update();

            mainPageViewModel.ExplorationViewModel.CancelPreviewColumn();

            ignoredPointerId = activatedPointerId;
        }

        public void PreviewRows(IEnumerable<Row> selectedRows)
        {
            this.SelectedRows = selectedRows;

            Reflect(mainPageViewModel.ExplorationViewModel.ViewStatus);
        }

        public void CancelPreviewRows()
        {
            SelectedRows = null;
            Reflect(mainPageViewModel.ExplorationViewModel.ViewStatus);
        }        

        public void Sort(ColumnViewModel columnViewModel, SortOption sortOption)
        {
            // 하나의 컬럼으로만 소트가 가능하다 현재는
            sortBy = columnViewModel;
            this.sortOption = sortOption;

            foreach (ColumnViewModel cvm in SheetViewModel.ColumnViewModels)
            {
                cvm.IsAscendingSorted = false;
                cvm.IsDescendingSorted= false;
            }

            if (sortOption == SortOption.Ascending)
            {
                columnViewModel.IsAscendingSorted = true;
            }
            else if(sortOption == SortOption.Descending)
            {
                columnViewModel.IsDescendingSorted = true;
            }
            Reflect(mainPageViewModel.ExplorationViewModel.ViewStatus);
        }

        public void OnAggregativeFunctionChanged(ColumnViewModel columnViewModel)
        {
            // 위 아래 컬럼 헤더 업데이트 (앞에 min, max 붙는 부분)
            // 는 바인딩으로 자동으로 됨
            // 여기서 주의점은 나중에 그룹바이시 컬럼 하나에 대해서만 min, max를 하는 경우 min, max 자체를 생략해야 한다는 점이다

            mainPageViewModel.ExplorationViewModel.SelectedPageViews.Last().PageViewModel.Reflect(true);

            mainPageViewModel.SheetViewModel.UpdateGroup(mainPageViewModel.ExplorationViewModel.ViewStatus);
            Reflect(mainPageViewModel.ExplorationViewModel.ViewStatus);
        }
    }
}
