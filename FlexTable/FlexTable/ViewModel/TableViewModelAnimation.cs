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

            public static AnimationScenario None = new AnimationScenario()
            {
                TotalAnimationDuration = -1,
                TableHeaderUpdateTime = -1
            };
        }

        private AnimationScenario PlayCollapseAnimation(IEnumerable<RowViewModel> previousRowViewModels, ViewStatus viewStatus, ColumnViewModel categorical,
            Double beforeX, Double afterX, ColumnViewModel pivotCategorical)
        {
            Canvas canvas = view.TableView.AnimatingRowCanvas;
            AnimationHint hint = stashedAnimationHint;
            
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

            storyboard.Begin();
            stashedViewStatus = null;

            return new AnimationScenario()
            {
                TotalAnimationDuration = 1400 + horizontalAnimationDuration,
                TableHeaderUpdateTime = 1400
            };
        }

        private AnimationScenario PlayExpandAnimation(IEnumerable<RowViewModel> previousRowViewModels, ViewStatus viewStatus, ColumnViewModel categorical,
            Double beforeX, Double afterX, ColumnViewModel pivotCategorical, Boolean requireDummy)
        {
            Canvas canvas = view.TableView.AnimatingRowCanvas;
            AnimationHint hint = stashedAnimationHint;

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
            storyboard.Begin();
            stashedViewStatus = null;
            return new AnimationScenario()
            {
                TotalAnimationDuration = 1000 + movingCellDuration,
                TableHeaderUpdateTime = 1000
            };
        }

        private AnimationScenario CreateTableAnimation(ViewStatus viewStatus)
        {
            if (stashedViewStatus == null) return AnimationScenario.None;
            AnimationHint hint = stashedAnimationHint;
            if (hint == null) return AnimationScenario.None;

            Canvas canvas = view.TableView.AnimatingRowCanvas;

            canvas.Children.Clear();

            if (stashedViewStatus.IsEmpty && viewStatus.IsC && hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.AllRowScrollViewer.ChangeView(0, 0, null, true);
                ColumnViewModel categorical = viewStatus.FirstCategorical;
                return PlayCollapseAnimation(SheetViewModel.AllRowViewModels, viewStatus, categorical,
                    hint.ColumnViewModelPosition[categorical], categorical.X, null);
            }
            else if (stashedViewStatus.IsCC && viewStatus.IsC && hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);
                ColumnViewModel categorical = stashedViewStatus.FirstCategorical;

                return PlayCollapseAnimation(stashedViewStatus.GroupedRowViewModels, viewStatus, categorical,
                    hint.ColumnViewModelPosition[categorical], categorical.X, stashedViewStatus.SecondCategorical);
            }
            else if (stashedViewStatus.IsC && viewStatus.IsCC && hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);
                ColumnViewModel categorical = viewStatus.SecondCategorical;

                return PlayExpandAnimation(viewStatus.GroupedRowViewModels, viewStatus, categorical, hint.ColumnViewModelPosition[categorical], categorical.X,
                    viewStatus.FirstCategorical, true);
            }
            else if (stashedViewStatus.IsC && viewStatus.IsEmpty && hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.AllRowScrollViewer.ChangeView(0, 0, null, true);
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);

                ColumnViewModel categorical = stashedViewStatus.FirstCategorical;

                return PlayExpandAnimation(SheetViewModel.AllRowViewModels, viewStatus, categorical, hint.ColumnViewModelPosition[categorical], categorical.X,
                    null, false);
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
                        Util.Animator.Generate(arp, "(Canvas.Top)", positionUpdatedRowViewModels.IndexOf(rvm) * (Double)App.Current.Resources["RowHeight"], 500, 200)
                    );

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 200));

                    index++;
                }

                storyboard.Begin();
                stashedViewStatus = null;
                return new AnimationScenario()
                {
                    TotalAnimationDuration = 700,
                    TableHeaderUpdateTime = 0
                };
            }
            
            return AnimationScenario.None;
        }
    }
}
