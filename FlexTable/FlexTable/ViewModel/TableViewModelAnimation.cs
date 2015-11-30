using FlexTable.Model;
using FlexTable.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace FlexTable.ViewModel
{
    public partial class TableViewModel
    {
        private Boolean CreateTableAnimation(ViewStatus previousViewStatus, ViewStatus viewStatus, AnimationHint hint)
        {
            if (previousViewStatus == null) return false;

            if(previousViewStatus.IsEmpty && viewStatus.IsC)
            {
                // 애니메이션 이전 로우 뷰 모델: SheetViewModel.AllRowViewModels
                // 애니메이션 이후 로우 뷰 모델: SheetViewModel.GroupedRowViewModels[0]

                IEnumerable<Row> rows = SheetViewModel.GroupingResult[0].Rows;
                IEnumerable<RowViewModel> targetRowViewModels =
                    SheetViewModel.AllRowViewModels
                    .Where(rvm => rows.Contains(rvm.Row))
                    .OrderBy(rvm => rvm.Y)
                    .Take(20);
                    

                ColumnViewModel categorical = viewStatus.FirstCategorical;
                Canvas canvas = view.TableView.AnimatingRowCanvas;

                canvas.Children.Clear();

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

                    arp.Opacity = 0.8;
                    arp.Update();
                    canvas.Children.Add(arp);
                    Canvas.SetTop(arp, rvm.Y);
                    storyboard.Children.Add(
                        Util.Animator.Generate(arp, "(Canvas.Top)", 0, 500)
                    );
                    DoubleAnimation ani = Util.Animator.Generate(arp.CellPresenter, "(Canvas.Left)", categorical.X, 500, 500);
                    storyboard.Children.Add(ani);

                    if (index == 0)
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 1, 500));
                    }
                    else
                    {
                        storyboard.Children.Add(Util.Animator.Generate(arp, "Opacity", 0, 500));
                    }
                    index++;
                }

                storyboard.Completed += delegate
                {
                    canvas.Children.Clear();
                };
                storyboard.Begin();

                return true;
            }
            return false;
        }
    }
}
