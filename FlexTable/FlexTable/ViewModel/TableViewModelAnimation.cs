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
        private Double CreateTableAnimation(ViewStatus viewStatus)
        {
            if (stashedViewStatus == null) return -1;
            AnimationHint hint = stashedAnimationHint;
            if (hint == null) return -1;

            Canvas canvas = view.TableView.AnimatingRowCanvas;

            canvas.Children.Clear();

            if (stashedViewStatus.IsEmpty && viewStatus.IsC && hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.AllRowScrollViewer.ChangeView(0, 0, null, true);

                Int32 index = 0;
                IEnumerable<Row> rows = viewStatus.GroupedRows[0].Rows;
                IEnumerable<RowViewModel> targetRowViewModels =
                    SheetViewModel.AllRowViewModels
                    .Where(rvm => rows.Contains(rvm.Row) && rvm.Y < mainPageViewModel.Bounds.Height)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

                viewStatus.ColorRowViewModels(allRowViewModels, viewStatus.GroupedRowViewModels, viewStatus.GroupedRows);

                ColumnViewModel categorical = viewStatus.FirstCategorical;

                Storyboard storyboard = new Storyboard();

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

                    Canvas.SetTop(arp, rvm.Y);

                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", viewStatus.GroupedRowViewModels[0].Color, 500, 0)
                    );

                    storyboard.Children.Add(
                        Util.Animator.Generate(arp, "(Canvas.Top)", 0, 500, 700)
                    );

                    DoubleAnimation ani = Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", categorical.X, 500, 1200);
                    storyboard.Children.Add(ani);

                    if (index == 0)
                    {
                        //storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500));
                    }
                    else
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 700));
                    }
                    index++;
                }

                storyboard.Begin();
                stashedViewStatus = null;
                return 1700;
            }
            else if (stashedViewStatus.IsC && viewStatus.IsEmpty && hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.AllRowScrollViewer.ChangeView(0, 0, null, true);
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);
                
                Int32 index = 0;
                IEnumerable<Row> rows = stashedViewStatus.GroupedRows[0].Rows;
                IEnumerable<RowViewModel> targetRowViewModels =
                    SheetViewModel.AllRowViewModels
                    .Where(rvm => rows.Contains(rvm.Row) && rvm.Y < mainPageViewModel.Bounds.Height)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

                //stashedViewStatus.ColorRowViewModels(allRowViewModels, stashedViewStatus.GroupedRowViewModels, stashedViewStatus.GroupedRows);

                ColumnViewModel categorical = stashedViewStatus.FirstCategorical;

                Storyboard storyboard = new Storyboard();

                Double movingCellDuration = 0;

                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = stashedViewStatus.GroupedRowViewModels[0],
                        X = hint.ColumnViewModelPosition[categorical],
                        ColumnViewModel = categorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 1;

                    arp.Update();
                    arp.CellPresenter.Foreground = new SolidColorBrush(stashedViewStatus.GroupedRowViewModels[0].Color);
                    canvas.Children.Add(arp);

                    if (hint.ColumnViewModelPosition[categorical] != categorical.X)
                    {
                        storyboard.Children.Add(
                            Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", categorical.X, 500, 200)
                        );
                        movingCellDuration = 500;
                    }

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 200 + movingCellDuration));
                }

                foreach (RowViewModel rvm in targetRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rvm,
                        X = categorical.X,
                        ColumnViewModel = categorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 0; 
                    arp.Update();
                    canvas.Children.Add(arp);

                    Canvas.SetTop(arp, 0);

                    arp.CellPresenter.Foreground = new SolidColorBrush(stashedViewStatus.GroupedRowViewModels[0].Color);

                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", ViewStatus.DefaultRowHeaderColor, 500, 700)
                    );

                    storyboard.Children.Add(
                        Util.Animator.Generate(arp, "(Canvas.Top)", rvm.Y, 500, 700)
                    );
                    
                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 700));
                    
                    index++;
                }

                storyboard.Begin();
                stashedViewStatus = null;
                return 1700;
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
                return 700;
            }
            else if (stashedViewStatus.IsC && viewStatus.IsCC && hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);
                IEnumerable<Row> rows = stashedViewStatus.GroupedRows[0].Rows;
                IEnumerable<RowViewModel> expandedRowViewModels =
                    viewStatus.GroupedRowViewModels
                    .Where(rvm => rows.Intersect(rvm.Rows).Count() > 0)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

                ColumnViewModel categorical = viewStatus.SecondCategorical;

                viewStatus.ColorRowViewModels(allRowViewModels, viewStatus.GroupedRowViewModels, viewStatus.GroupedRows);
      
                Storyboard storyboard = new Storyboard();

                Double movingCellDuration = 0;

                // dummy
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = stashedViewStatus.GroupedRowViewModels[0],
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

                    if (hint.ColumnViewModelPosition[categorical] != categorical.X)
                    {
                        storyboard.Children.Add(
                            Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", categorical.X, 500, 200)
                        );
                        movingCellDuration = 500;
                    }

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 200 + movingCellDuration));
                }

                Int32 index = 0;
                foreach (RowViewModel rvm in expandedRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = rvm,
                        X = categorical.X,
                        ColumnViewModel = categorical
                    };

                    AnimatingRowPresenter arp = new AnimatingRowPresenter()
                    {
                        AnimatingRowViewModel = arvm
                    };

                    arp.Opacity = 0; // index == 0 ? 1 : 0;
                    arp.Update();
                    canvas.Children.Add(arp);

                    Canvas.SetTop(arp, 0);// beforeRowViewModel.Y);
                    arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                    storyboard.Children.Add(
                        Util.Animator.Generate(arp, "(Canvas.Top)", rvm.Y, 500, 200 + movingCellDuration)
                    );

                    storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500, 200 + movingCellDuration));

                    index++;
                }

                storyboard.Begin();
                stashedViewStatus = null;
                return 700 + movingCellDuration;
            }
            else if (stashedViewStatus.IsCC && viewStatus.IsC && hint.Type == AnimationHint.AnimationType.DependOnViewStatus)
            {
                view.TableView.GroupedRowScrollViewer.ChangeView(0, 0, null, true);
                IEnumerable<Row> rows = viewStatus.GroupedRows[0].Rows;
                IEnumerable<RowViewModel> collapsingRowViewModels =
                    stashedViewStatus.GroupedRowViewModels
                    .Where(rvm => rows.Intersect(rvm.Rows).Count() > 0)
                    .OrderBy(rvm => rvm.Y)
                    .Take(50);

                ColumnViewModel categorical = stashedViewStatus.FirstCategorical;

                viewStatus.ColorRowViewModels(allRowViewModels, viewStatus.GroupedRowViewModels, viewStatus.GroupedRows);

                Storyboard storyboard = new Storyboard();

                Double movingCellDuration = 0;
                
                Int32 index = 0;
                foreach (RowViewModel rvm in collapsingRowViewModels)
                {
                    AnimatingRowViewModel arvm = new AnimatingRowViewModel()
                    {
                        RowViewModel = viewStatus.GroupedRowViewModels[0],
                        X = categorical.X,
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
                    //arp.CellPresenter.Foreground = new SolidColorBrush(rvm.Color);

                    storyboard.Children.Add(
                        Util.Animator.GenerateColorAnimation(arp.CellPresenter, "(TextBlock.Foreground).(SolidColorBrush.Color)", viewStatus.GroupedRowViewModels[0].Color, 500, 0)
                    );                    
 
                    storyboard.Children.Add(
                        Util.Animator.Generate(arp, "(Canvas.Top)", 0, 500, 500)
                    );

                    if(index > 0)
                        storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500, 500));

                    index++;
                }

                storyboard.Begin();
                stashedViewStatus = null;
                return 1000;
            }
            return -1;
        }
    }
}
