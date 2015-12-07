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
using System.Collections.Specialized;

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
        public ViewStatus ViewStatus => selectedPageViews.Count == 0 ? initialViewStatus : selectedPageViews.Last().ViewModel.ViewStatus; 

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
            mainPageViewModel.SheetViewModel.FilterViewModels.CollectionChanged += FilterViewModels_CollectionChanged;
        }

        public void Initialize()
        {
            view.ExplorationView.Initialize();
            TopPageView.ViewModel.ViewStatus = new ViewStatus();
            previewingColumnViewModel = null;
            selectedPageViews.Clear();
        }

        private void FilterViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            view.ExplorationView.SetFilterEnabled(mainPageViewModel.SheetViewModel.FilterViewModels.Count > 0);
        }

        public void PreviewColumn(ColumnViewModel columnViewModel)
        {
            previewingColumnViewModel = columnViewModel;

            if (ViewStatus.SelectedColumnViewModels.IndexOf(previewingColumnViewModel) >= 0)
                return;

            // 현재 선택된 view status를 가져옴
            ViewStatus selectedViewStatus = ViewStatus.Clone();
            
            // 새 컬럼 추가
            selectedViewStatus.SelectedColumnViewModels.Add(columnViewModel);

            // 현재 추가된 컬럼으로 groupedRows와 GroupedRowViewModels를 만듦. 
            selectedViewStatus.Generate(mainPageViewModel.SheetViewModel);

            // 탑 페이지 뷰 모델 가져와서
            PageViewModel pageViewModel = TopPageView.ViewModel;

            // 복제한 view status를 추가 한 다음 
            pageViewModel.ViewStatus = selectedViewStatus;

            pageViewModel.State = PageViewModel.PageViewState.Previewing;

            // 이걸로 pageView를 채움
            
            pageViewModel.Reflect(true);
            TopPageView.ReflectState();
        }

        public void CancelPreviewColumn()
        {
            previewingColumnViewModel = null;
            if (TopPageView.ViewModel.State == PageViewModel.PageViewState.Previewing)
            {
                TopPageView.ViewModel.State = PageViewModel.PageViewState.Empty;
                TopPageView.ViewModel.Reflect(false);
                TopPageView.ReflectState();
            }
        }

        /// <summary>
        /// 바뀐 state는 이미 pageViewModel.State에 할당된 상태임. 이전 상태는 OldState에서 확인할 수 있음
        /// </summary>
        /// <param name="pageViewModel"></param>
        /// <param name="pageView"></param>
        /// <param name="isUndo"></param>
        public void PageViewStateChanged(PageViewModel pageViewModel, PageView pageView/*, Boolean isUndo*/)
        {
            PageViewModel.PageViewState state = pageViewModel.State,
                oldState = pageViewModel.OldState;
            
            if(oldState == PageViewModel.PageViewState.Undoing && state == PageViewModel.PageViewState.Selected) // undo 하는 경우
            {
                Debug.Assert(selectedPageViews.IndexOf(pageView) < 0);

                mainPageViewModel.TableViewModel.StashViewStatus(ViewStatus, AnimationHint.AnimationType.DependOnViewStatus);

                selectedPageViews.Add(pageView);

                // 새로운 page 만들기
                view.ExplorationView.AddNewPage();

                mainPageViewModel.ReflectAll(false);

                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());

                // Preview 풀기
                mainPageViewModel.TableViewModel.CancelIndexing(false);

                // Page View 아래로 보내기                    
                pageView.ReflectState();
            }
            else if (state == PageViewModel.PageViewState.Selected) // 현재 선택하는 경우
            {
                Debug.Assert(pageView == TopPageView);
                Debug.Assert(previewingColumnViewModel != null);
                Debug.Assert(ViewStatus.SelectedColumnViewModels.IndexOf(previewingColumnViewModel) < 0); // 이미 선택한 걸 또 선택하는 경우는 없도록 이 함수 호출전에 미리 체크해야함

                mainPageViewModel.TableViewModel.StashViewStatus(ViewStatus, AnimationHint.AnimationType.DependOnViewStatus);

                // 현재 탭된 컬럼의 viewStatus에는 previewingColumn이 추가되어 있는 상태임.
                selectedPageViews.Add(pageView);

                // 차트 새로 반영 (타이틀이 업데이트 될 것임)
                //pageViewModel.Reflect(true);

                // 새로운 page 만들기
                view.ExplorationView.AddNewPage();

                mainPageViewModel.ReflectAll(false);
                
                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());

                // Preview 풀기
                mainPageViewModel.TableViewModel.CancelIndexing(false);

                pageView.ReflectState();
            }
            else if(state == PageViewModel.PageViewState.Undoing) // 선택해제하는 경우
            {
                selectedPageViews.Remove(pageView);

                // Preview 풀기 (만약 되어있다면)
                mainPageViewModel.TableViewModel.CancelIndexing(false);

                // 기존 탑 페이지 지우고 이걸 탑 페이지로 지정
                view.ExplorationView.RemoveTopPage(pageView);

                mainPageViewModel.ReflectAll(true);

                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());

                // Page View 위로 올리기
                pageView.ReflectState();
            }            
        }        

        public Boolean FilterOut(FilterViewModel filterViewModel)
        {
            if (filterViewModel.Filter(mainPageViewModel.SheetViewModel.FilteredRows).Count() > 0)
            {
                mainPageViewModel.SheetViewModel.FilterViewModels.Insert(0, filterViewModel);

                mainPageViewModel.ReflectAll(ViewStatus, false);
                return true;
            }
            return false;
            /*
            // sheet 업데이트
            mainPageViewModel.SheetViewModel.Reflect(ViewStatus);

            // 테이블 업데이트
            mainPageViewModel.TableViewModel.Reflect(ViewStatus, null);

            // 차트 업데이트
            foreach(PageView pageView in selectedPageViews)
            {
                pageView.ViewModel.Reflect(true);
            }
            */
            // 뒤에 숨겨진 차트들은 어떻게하나? 좀 더 빨리 안될까
        }

        public void CancelFilter(FilterViewModel filterViewModel)
        {
            mainPageViewModel.SheetViewModel.FilterViewModels.Remove(filterViewModel);

            mainPageViewModel.ReflectAll(ViewStatus, false);

            /*
            // sheet 업데이트
            mainPageViewModel.SheetViewModel.Reflect(ViewStatus);

            // 테이블 업데이트
            mainPageViewModel.TableViewModel.Reflect(ViewStatus, null);

            // 차트 업데이트
            foreach (PageView pageView in selectedPageViews)
            {
                pageView.ViewModel.Reflect(true);
            }

            // 뒤에 숨겨진 차트들은 어떻게하나? 좀 더 빨리 안될까
            */
        }
    }
}
