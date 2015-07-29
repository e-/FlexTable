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
using FlexTable.Model;
using Windows.UI.Xaml.Media.Animation;

namespace FlexTable.ViewModel
{
    public class ExplorationViewModel : NotifyViewModel
    {
        private ColumnViewModel columnViewModel;
        public ColumnViewModel ColumnViewModel { get { return columnViewModel; } set { columnViewModel = value; OnPropertyChanged("ColumnViewModel"); } }

        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        MetadataViewModel metadataViewModel;
        public MetadataViewModel MetadataViewModel { get { return metadataViewModel; } set { metadataViewModel = value; OnPropertyChanged("MetadataViewModel"); } }

        public View.PageView TopPageView { get { return view.ExplorationView.TopPageView; } }

        IMainPage view;

        public ExplorationViewModel(MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            MetadataViewModel = new MetadataViewModel(mainPageViewModel);
            this.view = view;
        }

        public void ShowSummary(ColumnViewModel columnViewModel)
        {
            PageViewModel pageViewModel = TopPageView.PageViewModel;
            ColumnViewModel = columnViewModel;

            pageViewModel.ShowSummary(columnViewModel);
        }

        public void PageViewTapped(PageViewModel pageViewModel, View.PageView pageView)
        {
            if (pageView == TopPageView)
            {
                // grouping
                if (columnViewModel != null && columnViewModel.Type == ColumnType.Categorical && !columnViewModel.IsGroupedBy)
                {
                    // Page View 아래로 보내기                    
                    pageViewModel.GoDown();

                    // 새로운 page 만들기
                    view.ExplorationView.AddNewPage();

                    // Preview 풀기
                    mainPageViewModel.TableViewModel.CancelIndexing();

                    // SheetViewModel 에서 grouping 하기, 이건 rowViewModels를 업데이트함
                    mainPageViewModel.SheetViewModel.Group(columnViewModel);

                    // Table View 업데이트
                    mainPageViewModel.TableViewModel.UpdateRows();

                    view.TableView.TopColumnHeader.Update();
                    view.TableView.BottomColumnHeader.Update();
                    view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());
                }
            }
            else
            {
                if (pageViewModel.ColumnViewModel != null && pageViewModel.ColumnViewModel.IsGroupedBy) // 그룹핑 취소
                {
                    // Page View 위로 올리기
                    pageViewModel.GoUp();

                    // Preview 풀기 (만약 되어있다면)
                    mainPageViewModel.TableViewModel.CancelIndexing();

                    // 기존 탑 페이지 지우고 이걸 탑 페이지로 지정
                    view.ExplorationView.RemoveTopPage(pageView);

                    // SheetViewModel 에서 grouping 하기, 이건 rowViewModels를 업데이트함
                    mainPageViewModel.SheetViewModel.Ungroup(pageViewModel.ColumnViewModel);

                    // Table View 업데이트
                    mainPageViewModel.TableViewModel.UpdateRows();

                    view.TableView.TopColumnHeader.Update();
                    view.TableView.BottomColumnHeader.Update();
                    view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());
                }
            }
        }

        public void StrokeAdded(InkStroke stroke)
        {
            /*
            Int32 index = 0;
            Rect rect = stroke.BoundingRect;
            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            foreach (Model.Bin bin in column.Bins.Select(b => b as Object).ToList())
            {
                Double x0 = LegendTextXGetter(bin, index),
                       y0 = LegendPatchYGetter(bin, index) + 10,
                       y1 = y0 + LegendPatchHeightGetter(bin, index) + 10;

                if (x0 <= center.X - mainPageViewModel.Width / 2 + ChartWidth && y0 <= center.Y && center.Y <= y1)
                {
                    bin.IsFilteredOut = !bin.IsFilteredOut;
                    break;
                }             
                index++;
            }

            d3.Scale.Ordinal xScale = new d3.Scale.Ordinal()
            {
                RangeStart = 70,
                RangeEnd = ChartWidth
            };
            foreach (Model.Bin bin in column.Bins.Where(b => !b.IsFilteredOut)) { xScale.Domain.Add(bin.Name); }
            XScale = xScale;

            Data = new d3.Selection.Data()
            {
                Real = column.Bins.Where(b => !b.IsFilteredOut).Select(b => b as Object).ToList()
            };

            LegendData = new d3.Selection.Data()
            {
                Real = column.Bins.Select(b => b as Object).ToList()
            };

            mainPageViewModel.UpdateFiltering();*/
        }
    }
}
