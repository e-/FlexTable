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
    public partial class TableViewModel : NotifyViewModel
    {
        public enum TableViewState { AllRow, GroupedRow, SelectedRow, Animation};

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
        
        private List<RowViewModel> allRowViewModels;
        public List<RowViewModel> AllRowViewModels
        {
            get { return allRowViewModels; }
            set { allRowViewModels = value; OnPropertyChanged(nameof(AllRowViewModels)); }
        }

        private List<RowViewModel> groupedRowViewModels;
        public List<RowViewModel> GroupedRowViewModels
        {
            get { return groupedRowViewModels; }
            set { groupedRowViewModels = value; OnPropertyChanged(nameof(GroupedRowViewModels)); }
        }

        public IEnumerable<RowViewModel> ActivatedRowViewModels { get; set; }
        
        private TableViewState oldState = TableViewState.AllRow;
        public TableViewState OldState { get { return oldState; } }

        private TableViewState state = TableViewState.AllRow;
        public TableViewState State { get { return state; } set { oldState = state; state = value; } }

        private Boolean isIndexing;
        public Boolean IsIndexing { get { return isIndexing; } set { isIndexing = value; OnPropertyChanged("IsIndexing"); } }

        private ViewStatus previousViewStatus = new ViewStatus();

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
            State = TableViewState.AllRow;
            // 최대 row header 추가
            view.TableView.GuidelinePresenter.SetMaximumRowNumber(SheetViewModel.AllRowViewModels.Count);
            view.TableView.RowHeaderPresenter.SetRowMaximumNumber(SheetViewModel.AllRowViewModels.Count);

            view.TableView.TopColumnHeader.Reset();
            view.TableView.BottomColumnHeader.Reset();

            view.TableView.TopColumnHeader.Update();
            view.TableView.BottomColumnHeader.Update();

            PaddedSheetHeight = Math.Max(SheetViewModel.AllRowsSheetHeight, SheetViewHeight);
            PaddedSheetWidth = Math.Max(SheetViewModel.SheetWidth, SheetViewWidth);

            AllRowViewModels = new List<RowViewModel>(SheetViewModel.AllRowViewModels);
            ActivatedRowViewModels = AllRowViewModels;

            view.TableView.Initialize();
            view.TableView.ReflectState(null);

            view.TableView.ColumnIndexer.Update();
        }

        private AnimationHint stashedAnimationHint = null;
        private ViewStatus stashedViewStatus = null;

        public void StashViewStatus(ViewStatus viewStatus, AnimationHint.AnimationType animationType)
        {
            stashedAnimationHint = AnimationHint.Create(mainPageViewModel.SheetViewModel, this);
            stashedAnimationHint.Type = animationType;
            stashedViewStatus = viewStatus.Clone();
        }

        DispatcherTimer dispatcherTimer = null;
        const Double DeferredReflectionTimeInMS = 1000;

        public void Reflect(ViewStatus viewStatus)
        {
            // 애니메이션 로직이 들어가야함
            Double animationTimer = CreateTableAnimation(viewStatus);

            if (animationTimer > 0)
            {
                State = TableViewState.Animation;
                view.TableView.ReflectState(viewStatus);
            }

            if (dispatcherTimer != null && dispatcherTimer.IsEnabled) dispatcherTimer.Stop();
            dispatcherTimer = new DispatcherTimer();

            dispatcherTimer.Tick += (sender, e) =>
            {
                dispatcherTimer.Stop();
                Update(viewStatus);

                view.TableView.ReflectState(viewStatus);

                // column header 업데이트
                foreach (ColumnViewModel cvm in mainPageViewModel.SheetViewModel.ColumnViewModels)
                {
                    cvm.UpdateHeaderName();
                }

                view.TableView.TopColumnHeader.Update();
                view.TableView.BottomColumnHeader.Update();
                view.TableView.ColumnIndexer.Update();
                previousViewStatus = viewStatus;
            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(animationTimer > 0 ? animationTimer : 1); // 700);
            dispatcherTimer.Start();
        }
        
        void Update(ViewStatus viewStatus)
        {
            Int32 index = 0;
            if(SelectedRows != null) // 로우 선택 중
            {
                AllRowViewModels.Sort(new RowViewModelComparer(SheetViewModel, viewStatus));
                index = 0;
                foreach (RowViewModel rowViewModel in AllRowViewModels)
                {
                    rowViewModel.Index = index++;
                }

                State = TableViewState.SelectedRow;
            }
            else if (viewStatus.IsEmpty || 
                (viewStatus.IsNN && viewStatus.IsScatterplotVisible) || 
                (viewStatus.IsCNN && viewStatus.IsScatterplotVisible))
            {
                AllRowViewModels.Sort(new RowViewModelComparer(SheetViewModel, viewStatus));
                index = 0;
                foreach (RowViewModel rowViewModel in AllRowViewModels)
                {
                    rowViewModel.Index = index++;
                }
                ActivatedRowViewModels = AllRowViewModels;
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
                    GroupedRowViewModels = new List<RowViewModel>(SheetViewModel.GroupedRowViewModels);

                ActivatedRowViewModels = GroupedRowViewModels;
                State = TableViewState.GroupedRow;
            }

            viewStatus.ColorRowViewModels(allRowViewModels, groupedRowViewModels, SheetViewModel.GroupedRows);
        }

        uint ignoredPointerId;
        uint activatedPointerId; // for cancel indexing

        public void IndexColumn(uint id, Int32 index) //Double y)
        {
            if (state == TableViewState.SelectedRow) return;
            if (ignoredPointerId == id) return;

            Double totalHeight = SheetViewHeight;
            Int32 columnIndex = index; // (Int32)Math.Floor(y / totalHeight * SheetViewModel.ColumnViewModels.Count);

            if (columnIndex < 0 || columnIndex >= SheetViewModel.ColumnViewModels.Count) return;

            ColumnViewModel columnViewModel = SheetViewModel.ColumnViewModels.Where(cvm => !cvm.IsSelected).OrderBy(cvm => cvm.Order).ElementAt(index);

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
            if(this.SelectedRows == null)
            {
                StashViewStatus(mainPageViewModel.ExplorationViewModel.ViewStatus, AnimationHint.AnimationType.SelectRows);
            }
            this.SelectedRows = selectedRows;
            Reflect(mainPageViewModel.ExplorationViewModel.ViewStatus);
        }

        public void CancelPreviewRows()
        {
            SelectedRows = null;
            Reflect(mainPageViewModel.ExplorationViewModel.ViewStatus);
        }        

        public void SelectRowsByRange(Double startY, Double endY)
        {
            if(mainPageViewModel.ExplorationViewModel.SelectedPageViews.Count == 0)
            {
                return;
            }

            PageView topPageView = mainPageViewModel.ExplorationViewModel.SelectedPageViews.Last();
            PageViewModel topPageViewModel = topPageView.ViewModel;

            Double rowHeight = (Double)App.Current.Resources["RowHeight"];

            //startY = Math.Floor(startY / rowHeight) * rowHeight;
            //endY = Math.Ceiling(endY / rowHeight) * rowHeight;

            startY -= rowHeight / 2; // row의 중앙을 기준으로 테스트 하기 위함임
            endY -= rowHeight / 2;

            if(State == TableViewState.AllRow)
            {
                IEnumerable<Row> selectedRows = allRowViewModels.Where(rvm => startY <= rvm.Y && rvm.Y < endY).Select(rvm => rvm.Row);
                topPageView.SelectionChanged(null, selectedRows);
            }
            else if(State == TableViewState.GroupedRow)
            {
                IEnumerable<Row> selectedRows = groupedRowViewModels.Where(rvm => startY <= rvm.Y && rvm.Y < endY).SelectMany(rvm => rvm.Rows);
                topPageView.SelectionChanged(null, selectedRows);
            }
            else if(State == TableViewState.SelectedRow)
            {
                IEnumerable<Row> selectedRows = allRowViewModels.Where(rvm => startY <= rvm.Y && rvm.Y < endY).Select(rvm => rvm.Row);
                topPageView.SelectionChanged(null, selectedRows);
            }
            else
            {
                return;
            }
        }
    }
}
