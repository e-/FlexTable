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

namespace FlexTable.ViewModel
{
    public partial class TableViewModel
    {
        public class AnimationScenario
        {
            public Double TotalAnimationDuration { get; set; }
            public Double TableHeaderUpdateTime { get; set; }
            public Storyboard AnimationStoryboard { get; set; }

            public static AnimationScenario None = new AnimationScenario()
            {
                TotalAnimationDuration = -1,
                TableHeaderUpdateTime = -1,
                AnimationStoryboard = null
            };
        }

        private AnimationScenario PlayCollapseAnimation(IEnumerable<RowViewModel> previousRowViewModels, ViewStatus viewStatus, ColumnViewModel categorical,
            Double beforeX, Double afterX, ColumnViewModel pivotCategorical)
        {
            Canvas canvas = view.TableView.AnimatingRowCanvas;
            AnimationHint hint = stashedViewStatus.AnimationHint;
            
            canvas.Children.Clear();

            Int32 index = 0;
            IEnumerable<Row> rows = viewStatus.GroupedRows[0].Rows;
            IEnumerable<RowViewModel> collapsingRowViewModels =
                previousRowViewModels
                .Where(rvm => (rvm.Row == null ? rows.Intersect(rvm.Rows).Count() > 0 : rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                .OrderBy(rvm => rvm.Y)
                .Take(50);

            viewStatus.ColorRowViewModels(allRowViewModels, viewStatus.GroupedRowViewModels, viewStatus.GroupedRows);
            
            Storyboard storyboard = new Storyboard();
            Double horizontalAnimationDuration = 0;

            foreach (RowViewModel rvm in collapsingRowViewModels)
            {
                AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                {
                    RowViewModel = rvm,
                    X = beforeX,
                    ColumnViewModel = categorical
                };

                AnimatingRowPresenter arp = new AnimatingRowPresenter()
                {
                    AnimatingRowViewModel = arvm
                };

                arp.Opacity = 1;
                arp.Update();
                canvas.Children.Add(arp);

                Canvas.SetTop(arp, rvm.Y);
                
                // 먼저 색 변경
                storyboard.Children.Add(
                    Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", viewStatus.GroupedRowViewModels[0].Color, 500, 200)
                );

                // 그 다음 세로로 모음
                storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", 0, 500, 900));
                
                if(index > 0)
                {
                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 900));
                }
                
                // 그 다음 가로로 이동
                if (beforeX != afterX)
                {
                    storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, 500, 1400));
                    horizontalAnimationDuration = 500;
                }
                index++;
            }

            if(pivotCategorical != null)
            {
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = viewStatus.GroupedRowViewModels[0],
                        X = hint.ColumnViewModelPosition[pivotCategorical],
                        ColumnViewModel = pivotCategorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 0;
                    arp.Update();
                    canvas.Children.Add(arp);

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 900));
                    storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", pivotCategorical.X, 500, 1400));
                }

                foreach (RowViewModel rvm in collapsingRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rvm,
                        X = hint.ColumnViewModelPosition[pivotCategorical],
                        ColumnViewModel = pivotCategorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 1;
                    arp.Update();
                    canvas.Children.Add(arp);
                    arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                    Canvas.SetTop(arp, rvm.Y);

                    // 먼저 색 변경
                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", ViewStatus.DefaultCellForegroundColor, 500, 200)
                    );

                    // 그 다음 세로로 모음
                    storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", 0, 500, 900));

                    if (index > 0)
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 900));
                    }

                    horizontalAnimationDuration = 500;
                    index++;
                }
            }

            stashedViewStatus = null;

            return new AnimationScenario()
            {
                TotalAnimationDuration = 1400 + horizontalAnimationDuration,
                TableHeaderUpdateTime = 1400,
                AnimationStoryboard = storyboard
            };
        }

        private AnimationScenario PlayExpandAnimation(IEnumerable<RowViewModel> previousRowViewModels, ViewStatus viewStatus, ColumnViewModel categorical,
            Double beforeX, Double afterX, ColumnViewModel pivotCategorical, Boolean requireDummy)
        {
            Canvas canvas = view.TableView.AnimatingRowCanvas;
            AnimationHint hint = stashedViewStatus.AnimationHint;

            canvas.Children.Clear();

            IEnumerable<Row> rows = stashedViewStatus.GroupedRows[0].Rows;
            IEnumerable<RowViewModel> expandedRowViewModels =
                previousRowViewModels
                .Where(rvm => (rvm.Row == null ? rows.Intersect(rvm.Rows).Count() > 0 : rows.Contains(rvm.Row)) && rvm.Y < mainPageViewModel.Bounds.Height)
                .OrderBy(rvm => rvm.Y)
                .Take(50);

            viewStatus.ColorRowViewModels(allRowViewModels, viewStatus.GroupedRowViewModels, viewStatus.GroupedRows);

            Storyboard storyboard = new Storyboard();

            Double movingCellDuration = 0;

            // dummy
            if(requireDummy) {
                AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                {
                    RowViewModel = stashedViewStatus.GroupedRowViewModels[0],
                    X = beforeX,
                    ColumnViewModel = categorical
                };

                AnimatingRowPresenter arp = new AnimatingRowPresenter()
                {
                    AnimatingRowViewModel = arvm
                };

                arp.Opacity = 1;
                arp.Update();
                canvas.Children.Add(arp);
                
                storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 500));
            }

            Int32 index = 0;
            foreach (RowViewModel rvm in expandedRowViewModels)
            {
                AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                {
                    RowViewModel = rvm,
                    X = beforeX,
                    ColumnViewModel = categorical
                };

                AnimatingRowPresenter arp = new AnimatingRowPresenter()
                {
                    AnimatingRowViewModel = arvm
                };

                arp.Opacity = requireDummy ? 0 : (index == 0 ? 1 : 0);
                arp.Update();
                canvas.Children.Add(arp);

                Canvas.SetTop(arp, 0);
                arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", rvm.Y, 500, 500));
                storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 500));

                storyboard.Children.Add(
                    Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", rvm.Color, 500, 500)
                );

                if (beforeX != afterX)
                {
                    storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, 500, 1000));
                    movingCellDuration = 500;
                }
                index++;
            }

            if(pivotCategorical != null)
            {
                index = 0;
                foreach (RowViewModel rvm in expandedRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rvm,
                        X = pivotCategorical.X,
                        ColumnViewModel = pivotCategorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = index == 0 ? 1 : 0;
                    arp.Update();
                    canvas.Children.Add(arp);

                    Canvas.SetTop(arp, 0);
                    //arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                    storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", rvm.Y, 500, 500));
                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 500));

                    /*storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", rvm.Color, 500, 200)
                    );*/
                    
                    index++;
                }
            }
            stashedViewStatus = null;
            return new AnimationScenario()
            {
                TotalAnimationDuration = 1000 + movingCellDuration,
                TableHeaderUpdateTime = 1000,
                AnimationStoryboard = storyboard
            };
        }

        const Double DelayBeforeAnimation = 300;
        const Double UnitAnimationDuration = 500;

        private AnimationScenario PlayCollapseAnimation(ViewStatus viewStatus, ColumnViewModel columnViewModel)
        {
            AnimationHint hint = stashedViewStatus.AnimationHint;
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
                    Canvas.SetTop(arp, rootIndex * (Double)App.Current.Resources["RowHeight"]);

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration * 2));
                    if (beforeX != afterX)
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration * 2));
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

                    Canvas.SetTop(arp, rvm.Y);

                    // 먼저 색 변경
                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", viewStatus.GroupedRowViewModels[0].Color, UnitAnimationDuration, DelayBeforeAnimation)
                    );

                    // 그 다음 세로로 모음
                    storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", rootIndex * (Double)App.Current.Resources["RowHeight"], UnitAnimationDuration * 2, DelayBeforeAnimation + UnitAnimationDuration));

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, UnitAnimationDuration * 2, DelayBeforeAnimation + UnitAnimationDuration));

                    // 그 다음 가로로 이동
                    if (beforeX != afterX)
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", afterX, UnitAnimationDuration, DelayBeforeAnimation + UnitAnimationDuration * 3));
                        horizontalAnimationDuration = UnitAnimationDuration;
                    }
                    index++;
                }

                /*if (pivotCategorical != null)
                {
                    {
                        AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                        {
                            RowViewModel = viewStatus.GroupedRowViewModels[0],
                            X = hint.ColumnViewModelPosition[pivotCategorical],
                            ColumnViewModel = pivotCategorical
                        };

                        AnimatingRowPresenter arp = new AnimatingRowPresenter()
                        {
                            AnimatingRowViewModel = arvm
                        };

                        arp.Opacity = 0;
                        arp.Update();
                        canvas.Children.Add(arp);

                        storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 900));
                        storyboard.Children.Add(Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", pivotCategorical.X, 500, 1400));
                    }

                    foreach (RowViewModel rvm in collapsingRowViewModels)
                    {
                        AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                        {
                            RowViewModel = rvm,
                            X = hint.ColumnViewModelPosition[pivotCategorical],
                            ColumnViewModel = pivotCategorical
                        };

                        AnimatingRowPresenter arp = new AnimatingRowPresenter()
                        {
                            AnimatingRowViewModel = arvm
                        };

                        arp.Opacity = 1;
                        arp.Update();
                        canvas.Children.Add(arp);
                        arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                        Canvas.SetTop(arp, rvm.Y);

                        // 먼저 색 변경
                        storyboard.Children.Add(
                            Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", ViewStatus.DefaultCellForegroundColor, 500, 200)
                        );

                        // 그 다음 세로로 모음
                        storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", 0, 500, 900));

                        if (index > 0)
                        {
                            storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 900));
                        }

                        horizontalAnimationDuration = 500;
                        index++;
                    }
                }*/
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
            AnimationHint hint = stashedViewStatus.AnimationHint;
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

            // dummy
            /*if (requireDummy)
            {
                AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                {
                    RowViewModel = stashedViewStatus.GroupedRowViewModels[0],
                    X = beforeX,
                    ColumnViewModel = categorical
                };

                AnimatingRowPresenter arp = new AnimatingRowPresenter()
                {
                    AnimatingRowViewModel = arvm
                };

                arp.Opacity = 1;
                arp.Update();
                canvas.Children.Add(arp);

                storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 500));
            }*/

            Boolean requireDummy = false;

            if (viewStatus.IsAllRowViewModelVisible)
            {
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

                    arp.Opacity = 0; // requireDummy ? 0 : (index == 0 ? 1 : 0);
                    arp.Update();
                    canvas.Children.Add(arp);

                    Canvas.SetTop(arp, rootIndex * (Double)App.Current.Resources["RowHeight"]);
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
                /*if (pivotCategorical != null)
                {
                    index = 0;
                    foreach (RowViewModel rvm in expandingRowViewModels)
                    {
                        AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                        {
                            RowViewModel = rvm,
                            X = pivotCategorical.X,
                            ColumnViewModel = pivotCategorical
                        };

                        AnimatingRowPresenter arp = new AnimatingRowPresenter()
                        {
                            AnimatingRowViewModel = arvm
                        };

                        arp.Opacity = index == 0 ? 1 : 0;
                        arp.Update();
                        canvas.Children.Add(arp);

                        Canvas.SetTop(arp, 0);
                        //arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                        storyboard.Children.Add(Util.Animator.Generate(arp, "(Canvas.Top)", rvm.Y, 500, 500));
                        storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 500));


                        index++;
                    }
                }*/

            stashedViewStatus = null;
            return new AnimationScenario()
            {
                TotalAnimationDuration = DelayBeforeAnimation + UnitAnimationDuration + horizontalAnimationDuration,
                TableHeaderUpdateTime = DelayBeforeAnimation + UnitAnimationDuration,
                AnimationStoryboard = storyboard
            };
        }

        /// <summary>
        /// AnimatingRowViewer은 즉시 Opacity가 1로 선택되므로 시작 딜레이 필요 없음
        /// </summary>
        /// <param name="viewStatus"></param>
        /// <returns></returns>
        private AnimationScenario CreateTableAnimation(ViewStatus viewStatus)
        {
            if (stashedViewStatus == null) return AnimationScenario.None;
            AnimationHint hint = stashedViewStatus.AnimationHint;
            if (hint == null) return AnimationScenario.None;

            Canvas canvas = view.TableView.AnimatingRowCanvas;

            canvas.Children.Clear();

            if (hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.AllRowScrollViewer.ChangeView(0, 0, null, true);
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);

                // 줄어드는 애니메이션
                if(stashedViewStatus.IsEmpty && viewStatus.SelectedCount > 0) // 첫 컬럼 선택
                {
                    ColumnViewModel columnViewModel = viewStatus.FirstColumn;
                    return PlayCollapseAnimation(viewStatus, columnViewModel);
                }
                else if(stashedViewStatus.SelectedCount > viewStatus.SelectedCount && !viewStatus.IsEmpty) // 첫 컬럼 이외에 해제한 경우
                {
                    ColumnViewModel columnViewModel = viewStatus.FirstColumn;
                    return PlayCollapseAnimation(SheetViewModel.AllRowViewModels, viewStatus, columnViewModel,
                        hint.ColumnViewModelPosition[columnViewModel], columnViewModel.X, null);
                }
                else if(stashedViewStatus.SelectedCount > 0 && viewStatus.IsEmpty) // 첫 컬럼 해제
                {
                    ColumnViewModel columnViewModel = stashedViewStatus.FirstColumn;
                    return PlayExpandAnimation(viewStatus, columnViewModel); // dummy:true
                }
                else if(stashedViewStatus.SelectedCount < viewStatus.SelectedCount) // 첫 컬럼 이외에 선택
                {
                    //늘어나는 애니메이션
                    ColumnViewModel columnViewModel = viewStatus.LastColumn;
                    return PlayExpandAnimation(viewStatus.GroupedRowViewModels, viewStatus, columnViewModel, 
                        hint.ColumnViewModelPosition[columnViewModel], columnViewModel.X, columnViewModel, true);
                }
                /*if (stashedViewStatus.IsEmpty && viewStatus.IsC)
                {
                    ColumnViewModel categorical = viewStatus.FirstCategorical;
                    return PlayCollapseAnimation(SheetViewModel.AllRowViewModels, viewStatus, categorical,
                        hint.ColumnViewModelPosition[categorical], categorical.X, null);
                }
                else if (stashedViewStatus.IsCC && viewStatus.IsC)
                {
                    ColumnViewModel categorical = stashedViewStatus.FirstCategorical;

                    return PlayCollapseAnimation(stashedViewStatus.GroupedRowViewModels, viewStatus, categorical,
                        hint.ColumnViewModelPosition[categorical], categorical.X, stashedViewStatus.SecondCategorical);
                }
                else if (stashedViewStatus.IsC && viewStatus.IsCC)
                {
                    ColumnViewModel categorical = viewStatus.SecondCategorical;

                    return PlayExpandAnimation(viewStatus.GroupedRowViewModels, viewStatus, categorical, hint.ColumnViewModelPosition[categorical], categorical.X,
                        viewStatus.FirstCategorical, true);
                }
                else if (stashedViewStatus.IsC && viewStatus.IsEmpty)
                {
                    ColumnViewModel categorical = stashedViewStatus.FirstCategorical;

                    return PlayExpandAnimation(SheetViewModel.AllRowViewModels, viewStatus, categorical, hint.ColumnViewModelPosition[categorical], categorical.X,
                        null, false);
                }*/
            }
            else if (hint.Type == AnimationHint.AnimationType.SelectRows && (viewStatus.IsC || viewStatus.IsCN) && SelectedRows != null && SelectedRows.Count() > 0)
            {
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);
                view.TableView.SelectedRowScrollViewer.ChangeView(0, 0, null, true);

                ColumnViewModel categorical = viewStatus.FirstCategorical;

                // 선택된 row중에 가장 많이 등장하는 category
                var mostFrequent = SelectedRows
                    .GroupBy(row => row.Cells[categorical.Index].Content)
                    .OrderByDescending(group => group.Count())
                    .First();

                Object category = mostFrequent.Key;
                IEnumerable<Row> selectedRows = mostFrequent;

                List<RowViewModel> positionUpdatedRowViewModels = new List<RowViewModel>(AllRowViewModels.Where(rvm => SelectedRows.Contains(rvm.Row)));

                positionUpdatedRowViewModels.Sort(new RowViewModelComparer(SheetViewModel, viewStatus));

                RowViewModel beforeRowViewModel = stashedViewStatus.GroupedRowViewModels.First(rvm => rvm.Cells[categorical.Index].Content == category);
                IEnumerable<RowViewModel> targetRowViewModels =
                    SheetViewModel.AllRowViewModels
                    .Where(rvm => selectedRows.Contains(rvm.Row))
                    .OrderBy(rvm => positionUpdatedRowViewModels.IndexOf(rvm))
                    .Take(50);

                Storyboard storyboard = new Storyboard();
                Double rowHeight = (Double)App.Current.Resources["RowHeight"];
                Int32 index = 0;
                foreach (RowViewModel rvm in targetRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rvm,
                        X = hint.ColumnViewModelPosition[categorical],
                        ColumnViewModel = categorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = index == 0 ? 1 : 0;
                    arp.Update();
                    canvas.Children.Add(arp);

                    Canvas.SetTop(arp, beforeRowViewModel.Y);
                    arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                    storyboard.Children.Add(
                        Util.Animator.Generate(arp, "(Canvas.Top)", positionUpdatedRowViewModels.IndexOf(rvm) * rowHeight, 500, 200)
                    );

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 200));

                    index++;
                }

                stashedViewStatus = null;
                return new AnimationScenario()
                {
                    TotalAnimationDuration = 700,
                    TableHeaderUpdateTime = 0,
                    AnimationStoryboard = storyboard
                };
            }
            else if (hint.Type == AnimationHint.AnimationType.UnselectRows && (viewStatus.IsC || viewStatus.IsCN) && stashedViewStatus.SelectedRows != null && stashedViewStatus.SelectedRows.Count() > 0)
            {
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);
                view.TableView.SelectedRowScrollViewer.ChangeView(0, 0, null, true);

                ColumnViewModel categorical = viewStatus.FirstCategorical;

                // 선택되었던 row들을 가장 많이 포함하는 groupingColumnViewModel을 고른다.
                RowViewModel pivotRowViewModel = viewStatus.GroupedRowViewModels
                    .OrderByDescending(rvm => rvm.Rows.Intersect(stashedViewStatus.SelectedRows).Count())
                    .First();

                List<RowViewModel> positionUpdatedRowViewModels = new List<RowViewModel>(AllRowViewModels.Where(rvm => stashedViewStatus.SelectedRows.Contains(rvm.Row)));
                positionUpdatedRowViewModels.Sort(new RowViewModelComparer(SheetViewModel, viewStatus));

                IEnumerable<RowViewModel> targetRowViewModels =
                    SheetViewModel.AllRowViewModels
                    .Where(rvm => pivotRowViewModel.Rows.Contains(rvm.Row))
                    .Take(50);

                Storyboard storyboard = new Storyboard();
                Double rowHeight = (Double)App.Current.Resources["RowHeight"];
                Int32 index = 0;

                /*{
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = pivotRowViewModel,
                        X = categorical.X,
                        ColumnViewModel = categorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 0;
                    arp.Update();
                    Canvas.SetTop(arp, pivotRowViewModel.Y);
                    canvas.Children.Add(arp);
                    arp.CellPresenter.Foreground = new SolidColorBrush(pivotRowViewModel.Color);

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 200));
                }*/

                foreach (RowViewModel rvm in targetRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rvm,
                        X = hint.ColumnViewModelPosition[categorical],
                        ColumnViewModel = categorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 1;
                    arp.Update();
                    canvas.Children.Add(arp);

                    Canvas.SetTop(arp, positionUpdatedRowViewModels.IndexOf(rvm) * rowHeight);
                    arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                    storyboard.Children.Add(
                        Util.Animator.Generate(arp, "(Canvas.Top)", pivotRowViewModel.Y, 500, 200)
                    );
                    if (index > 0)
                        storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 200));

                    index++;
                }

                stashedViewStatus = null;
                return new AnimationScenario()
                {
                    TotalAnimationDuration = 700,
                    TableHeaderUpdateTime = 0,
                    AnimationStoryboard = storyboard
                };
            }

            return AnimationScenario.None;
        }
    }
}
