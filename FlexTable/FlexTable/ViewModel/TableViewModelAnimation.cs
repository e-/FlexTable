using FlexTable.Model;
using FlexTable.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media;
using FlexTable.Util;

namespace FlexTable.ViewModel
{
    public partial class TableViewModel
    {
        public class AnimationScenario
        {
            public Double TotalAnimationDuration { get; set; }
            public Double TableHeaderUpdateTime { get; set; }
            public Storyboard AnimationStoryboard { get; set; }
            public ColumnViewModel SpotlightColumnViewModel { get; set; }

            public static AnimationScenario None = new AnimationScenario()
            {
                TotalAnimationDuration = -1,
                TableHeaderUpdateTime = -1,
                AnimationStoryboard = null
            };
        }
                
        const Double DelayBeforeAnimation = 200;
        const Double UnitAnimationDuration = 300;


        private AnimationScenario PlayCollapseAnimation(List<RowViewModel> previousRowViewModels, List<RowViewModel> currentRowViewModels, ColumnViewModel columnViewModel) 
        {
            Double beforeX = columnViewModel.StashedX;
            Double afterX = columnViewModel.X;

            Storyboard storyboard = new Storyboard();

            Canvas canvas = view.TableView.AnimatingRowCanvas;

            RowViewModel rootRowViewModel = currentRowViewModels[0];
            IEnumerable<Row> rows = rootRowViewModel.Rows;
            IEnumerable<RowViewModel> collapsingRowViewModels =
                    previousRowViewModels
                    .Where(rvm => (rvm.Row == null ? rows.Intersect(rvm.Rows).Count() > 0 : rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

            if (collapsingRowViewModels.Count() < 5)
            {
                foreach (RowViewModel groupedRowViewModel in currentRowViewModels)
                {
                    IEnumerable<RowViewModel> candidate =
                    previousRowViewModels
                    .Where(rvm => (rvm.Row == null ? groupedRowViewModel.Rows.Intersect(rvm.Rows).Count() > 0 : groupedRowViewModel.Rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

                    if (candidate.Count() >= 5)
                    {
                        rows = groupedRowViewModel.Rows;
                        collapsingRowViewModels = candidate;
                        rootRowViewModel = groupedRowViewModel;
                        break;
                    }
                }
            }

            Double colorChangingAnimationDuration = 0;
            Double horizontalAnimationDuration = 0;

            if (collapsingRowViewModels.Any(rvm => rvm.StashedColor != rootRowViewModel.Color))
            {
                colorChangingAnimationDuration = UnitAnimationDuration;
            }

            if (beforeX != afterX)
            {
                horizontalAnimationDuration = UnitAnimationDuration;
            }


            // root
            {
                AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                {
                    RowViewModel = rootRowViewModel,
                    X = beforeX,
                    ColumnViewModel = columnViewModel
                };

                AnimatingRowPresenter arp = new AnimatingRowPresenter()
                {
                    AnimatingRowViewModel = arvm
                };

                arp.Opacity = 0;
                arp.Update();
                canvas.Children.Add(arp);
                arp.CellPresenter.Foreground = new SolidColorBrush(rootRowViewModel.Color);
                Canvas.SetTop(arp, rootRowViewModel.Y);

                storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, UnitAnimationDuration, DelayBeforeAnimation + colorChangingAnimationDuration));

                if (beforeX != afterX)
                {
                    storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration + colorChangingAnimationDuration));
                }
            }

            foreach (RowViewModel rvm in collapsingRowViewModels)
            {
                AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                {
                    RowViewModel = rvm,
                    X = beforeX,
                    ColumnViewModel = columnViewModel
                };

                AnimatingRowPresenter arp = new AnimatingRowPresenter()
                {
                    AnimatingRowViewModel = arvm
                };

                arp.Opacity = 1;
                arp.Update();
                canvas.Children.Add(arp);

                Canvas.SetTop(arp, rvm.StashedY);
                arp.CellPresenter.Foreground = new SolidColorBrush(rvm.StashedColor);

                if (colorChangingAnimationDuration > 0)
                {
                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", rootRowViewModel.Color, UnitAnimationDuration, DelayBeforeAnimation)
                    );
                }

                // 그 다음 세로로 모음
                storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", rootRowViewModel.Y, UnitAnimationDuration, DelayBeforeAnimation + colorChangingAnimationDuration));
                storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, UnitAnimationDuration , DelayBeforeAnimation + colorChangingAnimationDuration));

                // 그 다음 가로로 이동
                if (beforeX != afterX)
                {
                    storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration + colorChangingAnimationDuration));
                }
            }

            stashedViewStatus = null;

            return new AnimationScenario()
            {
                TotalAnimationDuration = DelayBeforeAnimation + UnitAnimationDuration + horizontalAnimationDuration + colorChangingAnimationDuration,
                TableHeaderUpdateTime = DelayBeforeAnimation + UnitAnimationDuration + colorChangingAnimationDuration,
                AnimationStoryboard = storyboard,
                SpotlightColumnViewModel = columnViewModel
            };
        }

        private AnimationScenario PlayExpandAnimation(List<RowViewModel> previousRowViewModels, List<RowViewModel> currentRowViewModels, ColumnViewModel columnViewModel) //, ViewStatus viewStatus)
        {
            Double beforeX = columnViewModel.StashedX;
            Double afterX = columnViewModel.X;

            Storyboard storyboard = new Storyboard();
            
            Canvas canvas = view.TableView.AnimatingRowCanvas;

            RowViewModel rootGroupedRowViewModel = previousRowViewModels[0];
            IEnumerable<Row> rows = rootGroupedRowViewModel.Rows;
            IEnumerable<RowViewModel> expandingRowViewModels =
                currentRowViewModels
                .Where(rvm => (rvm.Row == null ? rows.Intersect(rvm.Rows).Count() > 0 : rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                .OrderBy(rvm => rvm.Y)
                .Take(50);

            if (expandingRowViewModels.Count() < 5)
            {
                foreach (RowViewModel groupedRowViewModel in previousRowViewModels)
                {
                    IEnumerable<RowViewModel> candidate =
                    currentRowViewModels
                    .Where(rvm => (rvm.Row == null ? groupedRowViewModel.Rows.Intersect(rvm.Rows).Count() > 0 : groupedRowViewModel.Rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

                    if (candidate.Count() >= 5)
                    {
                        rootGroupedRowViewModel = groupedRowViewModel;
                        rows = groupedRowViewModel.Rows;
                        expandingRowViewModels = candidate;
                        break;
                    }
                }
            }

            Double colorChangingAnimationDuration = 0;
            Double horizontalAnimationDuration = 0;

            if (expandingRowViewModels.Any(rvm => rvm.Color != rootGroupedRowViewModel.StashedColor))
            {
                colorChangingAnimationDuration = UnitAnimationDuration;
            }

            if (beforeX != afterX)
            {
                horizontalAnimationDuration = UnitAnimationDuration;
            }

            // root
            {
                AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                {
                    RowViewModel = rootGroupedRowViewModel,
                    X = beforeX,
                    ColumnViewModel = columnViewModel
                };

                AnimatingRowPresenter arp = new AnimatingRowPresenter()
                {
                    AnimatingRowViewModel = arvm
                };

                arp.Opacity = 1;
                arp.Update();
                canvas.Children.Add(arp);
                Canvas.SetTop(arp, rootGroupedRowViewModel.StashedY);
                arp.CellPresenter.Foreground = new SolidColorBrush(rootGroupedRowViewModel.Color);

                storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, UnitAnimationDuration, DelayBeforeAnimation + colorChangingAnimationDuration));
            }

            foreach (RowViewModel rvm in expandingRowViewModels)
            {
                AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                {
                    RowViewModel = rvm,
                    X = beforeX,
                    ColumnViewModel = columnViewModel
                };

                AnimatingRowPresenter arp = new AnimatingRowPresenter()
                {
                    AnimatingRowViewModel = arvm
                };

                arp.Opacity = 0;
                arp.Update();
                canvas.Children.Add(arp);

                Canvas.SetTop(arp, rootGroupedRowViewModel.StashedY);
                arp.CellPresenter.Foreground = new SolidColorBrush(rootGroupedRowViewModel.StashedColor);

                if (colorChangingAnimationDuration > 0)
                {
                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", rvm.Color, UnitAnimationDuration, DelayBeforeAnimation)
                    );
                }

                storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", rvm.Y, UnitAnimationDuration, DelayBeforeAnimation + colorChangingAnimationDuration));
                storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, UnitAnimationDuration, DelayBeforeAnimation + colorChangingAnimationDuration));


                if (beforeX != afterX)
                {
                    storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration + colorChangingAnimationDuration));
                }
            }

            stashedViewStatus = null;
            return new AnimationScenario()
            {
                TotalAnimationDuration = DelayBeforeAnimation + UnitAnimationDuration + horizontalAnimationDuration + colorChangingAnimationDuration,
                TableHeaderUpdateTime = DelayBeforeAnimation + UnitAnimationDuration + colorChangingAnimationDuration,
                AnimationStoryboard = storyboard,
                SpotlightColumnViewModel = columnViewModel
            };
        }

        /// <summary>
        /// Animation은 viewStatus에 새로운 컬럼이 추가되거나 해제될 때 마지막에 보이던 TableView와 현재 보일 TableView를 잇는데 사용된다.
        /// prevTableView와 currentTableView를 부드럽게 잇는 테이블 애니메이션이 가능한 경우와 불가능한 경우는 아래와 같다
        /// 
        /// 가능한 경우:
        ///     allRows -> groupedRows: 항상 가능
        ///     groupedRows -> allRows: 항상 가능
        ///     groupedRows-> selectedRows: 항상 가능
        ///     selectedRows -> groupedRows: 항상 가능
        ///     groupedRows -> groupedRows: N -> CN 인 경우는 불가능함
        /// 
        /// 불가능한 경우:
        ///     allRows -> allRows: 이런 상황이 일어나는 경우가 없음
        ///     selectedRows -> selectedRows: 셀렉션이 추가되거나 제거되는 경우인데 이건 현재 애니메이션을 지원하지 않음
        ///     allRows -> selectedRows: 스캐터플롯에서 셀렉션을 추가하는 경우가 있을 수 있는데 현재 애니메이션 지원하지 않음
        ///     selectedRows -> allRows: 스캐터플롯에서 셀렉션이 제거되는 경우가 있을 수 있는데 현재 애니메이션 지원하지 않음
        /// </summary>
        /// <param name="viewStatus"></param>
        /// <returns></returns>
        private AnimationScenario CreateTableAnimation(ViewStatus viewStatus)
        {
            if (!Settings.Instance.AnimationEnabled) return AnimationScenario.None;

            if (stashedViewStatus == null) return AnimationScenario.None;

            //AnimationHint hint = stashedViewStatus.AnimationHint;

            Canvas canvas = view.TableView.AnimatingRowCanvas;
            canvas.Children.Clear();

            TableViewState previousState = stashedViewStatus.TableViewState,
                           currentState = viewStatus.TableViewState;

            switch (previousState)
            {
                case TableViewState.AllRow:
                    switch(currentState)
                    {
                        case TableViewState.AllRow:
                            return AnimationScenario.None;
                        case TableViewState.GroupedRow: // 처음 그룹바이가 일어나는 경우
                            return PlayCollapseAnimation(SheetViewModel.AllRowViewModels, viewStatus.GroupedRowViewModels, viewStatus.FirstColumn); //, viewStatus);
                        case TableViewState.SelectedRow:
                            return AnimationScenario.None;
                    }
                    break;
                case TableViewState.GroupedRow:
                    switch (currentState)
                    {
                        case TableViewState.AllRow: // 그룹바이가 풀리는 경우 (C -> Empty, CN -> CNN 등)
                            return PlayExpandAnimation(stashedViewStatus.GroupedRowViewModels, SheetViewModel.AllRowViewModels, 
                                stashedViewStatus.CategoricalCount > 0 ? stashedViewStatus.LastCategorical : stashedViewStatus.FirstNumerical); //, viewStatus);
                        case TableViewState.GroupedRow:
                            // TODO

                            break;
                        case TableViewState.SelectedRow:
                            IEnumerable<Row> selectedOnScreenRows = SheetViewModel.AllRowViewModels.Where(arvm => SelectedRows.Contains(arvm.Row) && arvm.Y < mainPageViewModel.Bounds.Height).Select(arvm => arvm.Row);

                            RowViewModel selectedRowViewModel = stashedViewStatus.GroupedRowViewModels
                                .OrderByDescending(rvm => rvm.Rows.Intersect(selectedOnScreenRows).Count())
                                .First();

                            return PlayExpandAnimation(new List<RowViewModel>() { selectedRowViewModel }, SheetViewModel.AllRowViewModels,
                                stashedViewStatus.CategoricalCount > 0 ? stashedViewStatus.LastCategorical : stashedViewStatus.FirstNumerical); //, viewStatus);
                    }
                    break;
                case TableViewState.SelectedRow:
                    switch (currentState)
                    {
                        case TableViewState.AllRow:
                            return AnimationScenario.None;
                        case TableViewState.GroupedRow:
                            IEnumerable<Row> unselectedOnScreenRows = SheetViewModel.AllRowViewModels.Where(
                                arvm => stashedViewStatus.SelectedRows.Contains(arvm.Row) && arvm.StashedY < mainPageViewModel.Bounds.Height
                                ).Select(arvm => arvm.Row);

                            RowViewModel collapsingRowViewModel = viewStatus.GroupedRowViewModels.OrderByDescending(rvm => rvm.Rows.Intersect(unselectedOnScreenRows).Count()).First();

                            return PlayCollapseAnimation(AllRowViewModels, new List<RowViewModel>() { collapsingRowViewModel },
                                stashedViewStatus.CategoricalCount > 0 ? viewStatus.LastCategorical : viewStatus.FirstNumerical); //, viewStatus);
                        case TableViewState.SelectedRow:
                            return AnimationScenario.None;
                    }
                    break;
            }

            return AnimationScenario.None;            
        }


        private AnimationScenario PlayCollapseAnimation(ViewStatus viewStatus, ColumnViewModel columnViewModel)
        {
            AnimationHint hint = null;// stashedViewStatus.AnimationHint;
            IEnumerable<RowViewModel> previousRowViewModels = (stashedViewStatus.TableViewState == TableViewState.AllRow) ? SheetViewModel.AllRowViewModels : stashedViewStatus.GroupedRowViewModels;
            Double beforeX = hint.ColumnViewModelPosition[columnViewModel];
            Double afterX = columnViewModel.X;
            Int32 index = 0;
            Storyboard storyboard = new Storyboard();
            Double horizontalAnimationDuration = 0;

            Canvas canvas = view.TableView.AnimatingRowCanvas;
            canvas.Children.Clear();

            Int32 rootIndex = 0;
            IEnumerable<Row> rows = viewStatus.GroupedRows[0].Rows;
            IEnumerable<RowViewModel> collapsingRowViewModels =
                    previousRowViewModels
                    .Where(rvm => (rvm.Row == null ? rows.Intersect(rvm.Rows).Count() > 0 : rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

            if (collapsingRowViewModels.Count() < 5)
            {
                index = 0;
                foreach (GroupedRows groupedRows in viewStatus.GroupedRows)
                {
                    IEnumerable<RowViewModel> candidate =
                    previousRowViewModels
                    .Where(rvm => (rvm.Row == null ? groupedRows.Rows.Intersect(rvm.Rows).Count() > 0 : groupedRows.Rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

                    if (candidate.Count() >= 5)
                    {
                        rows = groupedRows.Rows;
                        collapsingRowViewModels = candidate;
                        rootIndex = index;
                        break;
                    }
                    index++;
                }
            }

            viewStatus.ColorRowViewModels(allRowViewModels, viewStatus.GroupedRowViewModels, viewStatus.GroupedRows);

            if (viewStatus.IsAllRowViewModelVisible)
            {

            }
            else // grouped row visible
            {
                // root
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = viewStatus.GroupedRowViewModels[rootIndex],
                        X = beforeX,
                        ColumnViewModel = columnViewModel
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 0;
                    arp.Update();
                    canvas.Children.Add(arp);
                    arp.CellPresenter.Foreground = new SolidColorBrush(viewStatus.GroupedRowViewModels[rootIndex].Color);
                    Canvas.SetTop(arp, rootIndex * Const.RowHeight);

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration * 2));
                    if (beforeX != afterX)
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration * 3));
                    }
                }

                index = 0;
                foreach (RowViewModel rvm in collapsingRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rvm,
                        X = beforeX,
                        ColumnViewModel = columnViewModel
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 1;
                    arp.Update();
                    canvas.Children.Add(arp);

                    Canvas.SetTop(arp, rvm.StashedY);

                    // 먼저 색 변경
                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", viewStatus.GroupedRowViewModels[0].Color, UnitAnimationDuration, DelayBeforeAnimation)
                    );

                    // 그 다음 세로로 모음
                    storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", rootIndex * Const.RowHeight, UnitAnimationDuration * 2, DelayBeforeAnimation + UnitAnimationDuration));

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, UnitAnimationDuration * 2, DelayBeforeAnimation + UnitAnimationDuration));

                    // 그 다음 가로로 이동
                    if (beforeX != afterX)
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration * 3));
                        horizontalAnimationDuration = UnitAnimationDuration;
                    }
                    index++;
                }                
            }

            stashedViewStatus = null;

            return new AnimationScenario()
            {
                TotalAnimationDuration = DelayBeforeAnimation + UnitAnimationDuration * 3 + horizontalAnimationDuration,
                TableHeaderUpdateTime = DelayBeforeAnimation + UnitAnimationDuration * 3,
                AnimationStoryboard = storyboard
            };
        }

        private AnimationScenario PlayExpandAnimation(ViewStatus viewStatus, ColumnViewModel columnViewModel)
        {
            AnimationHint hint = null;// stashedViewStatus.AnimationHint;
            IEnumerable<RowViewModel> previousRowViewModels = (stashedViewStatus.TableViewState == TableViewState.AllRow) ? SheetViewModel.AllRowViewModels : stashedViewStatus.GroupedRowViewModels;
            IEnumerable<RowViewModel> currentRowViewModels = viewStatus.IsAllRowViewModelVisible ? SheetViewModel.AllRowViewModels : viewStatus.GroupedRowViewModels;
            Double beforeX = hint.ColumnViewModelPosition[columnViewModel];
            Double afterX = columnViewModel.X;
            Int32 index = 0;
            Storyboard storyboard = new Storyboard();
            Double horizontalAnimationDuration = 0;

            Canvas canvas = view.TableView.AnimatingRowCanvas;
            canvas.Children.Clear();

            RowViewModel rootGroupedRowViewModel = stashedViewStatus.GroupedRowViewModels[0];
            Int32 rootIndex = 0;
            IEnumerable<Row> rows = rootGroupedRowViewModel.Rows;
            IEnumerable<RowViewModel> expandingRowViewModels =
                currentRowViewModels
                .Where(rvm => (rvm.Row == null ? rows.Intersect(rvm.Rows).Count() > 0 : rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                .OrderBy(rvm => rvm.Y)
                .Take(50);

            if (expandingRowViewModels.Count() < 5)
            {
                index = 0;
                foreach (GroupedRows groupedRows in stashedViewStatus.GroupedRows)
                {
                    IEnumerable<RowViewModel> candidate =
                    currentRowViewModels
                    .Where(rvm => (rvm.Row == null ? groupedRows.Rows.Intersect(rvm.Rows).Count() > 0 : groupedRows.Rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

                    if (candidate.Count() >= 5)
                    {
                        rootGroupedRowViewModel = stashedViewStatus.GroupedRowViewModels[index];
                        rows = groupedRows.Rows;
                        expandingRowViewModels = candidate;
                        rootIndex = index;
                        break;
                    }
                    index++;
                }
            }

            viewStatus.ColorRowViewModels(allRowViewModels, viewStatus.GroupedRowViewModels, viewStatus.GroupedRows);


            if (viewStatus.IsAllRowViewModelVisible)
            {
                // root
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rootGroupedRowViewModel,
                        X = beforeX,
                        ColumnViewModel = columnViewModel
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 1;
                    arp.Update();
                    canvas.Children.Add(arp);
                    Canvas.SetTop(arp, rootGroupedRowViewModel.StashedY);
                    arp.CellPresenter.Foreground = new SolidColorBrush(rootGroupedRowViewModel.Color);

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, UnitAnimationDuration, DelayBeforeAnimation));
                }

                index = 0;
                foreach (RowViewModel rvm in expandingRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rvm,
                        X = beforeX,
                        ColumnViewModel = columnViewModel
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 0;
                    arp.Update();
                    canvas.Children.Add(arp);

                    Canvas.SetTop(arp, rootGroupedRowViewModel.StashedY);
                    arp.CellPresenter.Foreground = new SolidColorBrush(rootGroupedRowViewModel.Color);

                    storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", rvm.Y, UnitAnimationDuration, DelayBeforeAnimation));
                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, UnitAnimationDuration, DelayBeforeAnimation));

                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", rvm.Color, UnitAnimationDuration, DelayBeforeAnimation)
                    );

                    if (beforeX != afterX)
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration));
                        horizontalAnimationDuration = UnitAnimationDuration;
                    }
                    index++;
                }
            }
            else // grouped row visible
            {
            }

            stashedViewStatus = null;
            return new AnimationScenario()
            {
                TotalAnimationDuration = DelayBeforeAnimation + UnitAnimationDuration + horizontalAnimationDuration,
                TableHeaderUpdateTime = DelayBeforeAnimation + UnitAnimationDuration,
                AnimationStoryboard = storyboard
            };
        }
    }
}
