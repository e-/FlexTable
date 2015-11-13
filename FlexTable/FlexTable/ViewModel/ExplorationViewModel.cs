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
        public MainPageViewModel MainPageViewModel
        {
            get { return mainPageViewModel; }
            set { mainPageViewModel = value; OnPropertyChanged(nameof(MainPageViewModel)); }
        }

        MetadataViewModel metadataViewModel;
        public MetadataViewModel MetadataViewModel { get { return metadataViewModel; } set { metadataViewModel = value; OnPropertyChanged("MetadataViewModel"); } }

        private ViewStatus initialViewStatus = new ViewStatus();
        /// <summary>
        /// 이 view status는 이미 선택된 컬럼들을 가지는 view status 임
        /// </summary>
        public ViewStatus ViewStatus => selectedPageViews.Count == 0 ? initialViewStatus : selectedPageViews.Last().PageViewModel.ViewStatus; 

        public PageView TopPageView { get { return view.ExplorationView.TopPageView; } }

        public PageViewModel DummyPageViewModel { get; set; } // for surpressing initial pageview binding warnings
        
        List<PageView> selectedPageViews = new List<PageView>();
        public List<PageView> SelectedPageViews => selectedPageViews;
        ColumnViewModel previewingColumnViewModel = null;
        IMainPage view;

        public ExplorationViewModel(MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.MainPageViewModel = mainPageViewModel;
            MetadataViewModel = new MetadataViewModel(mainPageViewModel);
            this.view = view;
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
            pageViewModel.Reflect(false);
        }

        public void CancelPreviewColumn()
        {
            previewingColumnViewModel = null;
            TopPageView.PageViewModel.HideSummary();
        }

        public void StatusChanged(PageViewModel pageViewModel, PageView pageView, Boolean isUndo)
        {
            if(isUndo && selectedPageViews.IndexOf(pageView) < 0)
            {
                selectedPageViews.Add(pageView);
                pageViewModel.IsSelected = true;

                // 차트 새로 반영할 필요는 없다 이미 있으므로
                // pageViewModel.Reflect(true);

                // 새로운 page 만들기
                view.ExplorationView.AddNewPage();

                mainPageViewModel.SheetViewModel.UpdateGroup(ViewStatus);
                mainPageViewModel.TableViewModel.Reflect(ViewStatus);

                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());

                // Preview 풀기
                mainPageViewModel.TableViewModel.CancelIndexing();

                // Page View 아래로 보내기                    
                pageView.MoveToSelectedPosition(isUndo);
                pageView.EnterSelectedMode();
            }
            else if (pageView == TopPageView && previewingColumnViewModel != null && ViewStatus.SelectedColumnViewModels.IndexOf(previewingColumnViewModel) < 0)
            {
                // 현재 탭된 컬럼의 viewStatus에는 previewingColumn이 추가되어 있는 상태임.
                selectedPageViews.Add(pageView);

                pageViewModel.IsSelected = true;
                                
                // 차트 새로 반영 (타이틀이 업데이트 될 것임)
                pageViewModel.Reflect(true);

                // 새로운 page 만들기
                view.ExplorationView.AddNewPage();

                mainPageViewModel.SheetViewModel.UpdateGroup(ViewStatus);
                mainPageViewModel.TableViewModel.Reflect(ViewStatus);               
                
                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());

                // Preview 풀기
                mainPageViewModel.TableViewModel.CancelIndexing();

                // Page View 아래로 보내기                    
                pageView.MoveToSelectedPosition(isUndo);
                pageView.EnterSelectedMode();
            }
            else if(pageView == TopPageView && ViewStatus.SelectedColumnViewModels.IndexOf(previewingColumnViewModel) >= 0) {
                // 이미 선택된 것 또 선택하는 경우
            }
            else if(pageView.PageViewModel.IsSelected) // 선택해제하는 경우
            {
                selectedPageViews.Remove(pageView);

                pageViewModel.IsSelected = false;

                // Preview 풀기 (만약 되어있다면)
                mainPageViewModel.TableViewModel.CancelIndexing();

                // 기존 탑 페이지 지우고 이걸 탑 페이지로 지정
                view.ExplorationView.RemoveTopPage(pageView);

                // SheetViewModel 에서 ungrouping 하기, 이건 rowViewModels를 업데이트함
                mainPageViewModel.SheetViewModel.UpdateGroup(ViewStatus);

                // Table View 업데이트
                mainPageViewModel.TableViewModel.Reflect(ViewStatus);

                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());

                // Page View 위로 올리기
                pageView.MoveToUnselectedPosition();
                pageView.EnterUndoMode();
            }            
        }        
    }
}
