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
using FlexTable.View;

namespace FlexTable.ViewModel
{
    public class ExplorationViewModel : NotifyViewModel
    {
        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        MetadataViewModel metadataViewModel;
        public MetadataViewModel MetadataViewModel { get { return metadataViewModel; } set { metadataViewModel = value; OnPropertyChanged("MetadataViewModel"); } }

        private ViewStatus initialViewStatus = new ViewStatus(); 
        public ViewStatus ViewStatus => selectedPageViews.Count == 0 ? initialViewStatus : selectedPageViews.Last().PageViewModel.ViewStatus; // 이 view status는 이미 선택된 컬럼들을 가지는 view status 임

        public PageView TopPageView { get { return view.ExplorationView.TopPageView; } }

        public PageViewModel DummyPageViewModel { get; set; } // for surpressing initial pageview binding warnings

        private Double pageHeight;
        public Double PageHeight { get { return pageHeight; } set { pageHeight = value; OnPropertyChanged("PageHeight"); } }

        private Double pageWidth;
        public Double PageWidth { get { return pageWidth; } set { pageWidth = value; OnPropertyChanged("PageWidth"); } }

        List<PageView> selectedPageViews = new List<PageView>();
        ColumnViewModel previewingColumnViewModel = null;
        IMainPage view;

        public ExplorationViewModel(MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            MetadataViewModel = new MetadataViewModel(mainPageViewModel);
            this.view = view;

            PageHeight = mainPageViewModel.Bounds.Height / 2;
            PageWidth = mainPageViewModel.Bounds.Width / 2;
        }

        public void PreviewColumn(ColumnViewModel columnViewModel)
        {
            previewingColumnViewModel = columnViewModel;

            // 현재 선택된 view status를 가져옴
            ViewStatus selectedViewStatus = ViewStatus.Clone();
            
            // 새 컬럼 추가
            selectedViewStatus.SelectedColumnViewModels.Add(columnViewModel);
            
            // 탑 페이지 뷰 모델 가져와서
            PageViewModel pageViewModel = TopPageView.PageViewModel;

            // 복제한 view status를 추가 한 다음 
            pageViewModel.ViewStatus = selectedViewStatus;

            // 이걸로 pageView를 채움
            pageViewModel.Reflect();
        }

        public void PageViewTapped(PageViewModel pageViewModel, PageView pageView)
        {
            if (pageView == TopPageView && previewingColumnViewModel != null && ViewStatus.SelectedColumnViewModels.IndexOf(previewingColumnViewModel) < 0)
            {
                // 현재 탭된 컬럼의 viewStatus에는 previewingColumn이 추가되어 있는 상태임.
                selectedPageViews.Add(pageView);
                
                // column header 업데이트
                foreach (ColumnViewModel cvm in mainPageViewModel.SheetViewModel.ColumnViewModels)
                {
                    cvm.UpdateHeaderName();
                }

                // Page View 아래로 보내기                    
                pageViewModel.GoDown();

                // 차트 새로 반영 (타이틀이 업데이트 될 것임)
                pageViewModel.Reflect();

                // 새로운 page 만들기
                view.ExplorationView.AddNewPage();

                // Preview 풀기
                mainPageViewModel.TableViewModel.CancelIndexing();

                // 선택 표시 (컬럼 위 아래 헤더 업데이트)
                previewingColumnViewModel.IsSelected = true;

                mainPageViewModel.SheetViewModel.UpdateGroup(ViewStatus);
                mainPageViewModel.TableViewModel.UpdateRows();
                
                view.TableView.TopColumnHeader.Update();
                view.TableView.BottomColumnHeader.Update();
                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());
            }
            else if(pageView == TopPageView && ViewStatus.SelectedColumnViewModels.IndexOf(previewingColumnViewModel) >= 0) {
                // 이미 선택된 것 또 선택하는 경우
            }
            else // 선택해제하는 경우
            {
                selectedPageViews.Remove(pageView);

                // column header 업데이트
                foreach (ColumnViewModel cvm in mainPageViewModel.SheetViewModel.ColumnViewModels)
                {
                    cvm.UpdateHeaderName();
                }

                // Page View 위로 올리기
                pageViewModel.GoUp();

                // Preview 풀기 (만약 되어있다면)
                mainPageViewModel.TableViewModel.CancelIndexing();

                // 기존 탑 페이지 지우고 이걸 탑 페이지로 지정
                view.ExplorationView.RemoveTopPage(pageView);

                // 선택 해제 (컬럼 위 아래 헤더 업데이트)
                pageViewModel.ViewStatus.SelectedColumnViewModels.Last().IsSelected = false;

                // SheetViewModel 에서 ungrouping 하기, 이건 rowViewModels를 업데이트함
                mainPageViewModel.SheetViewModel.UpdateGroup(ViewStatus);

                // Table View 업데이트
                mainPageViewModel.TableViewModel.UpdateRows();

                view.TableView.TopColumnHeader.Update();
                view.TableView.BottomColumnHeader.Update();
                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());
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
