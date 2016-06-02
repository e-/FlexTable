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
using FlexTable.Util;

namespace FlexTable.ViewModel
{
    public class ExplorationViewModel : Util.Notifiable
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
        }

        public void Initialize()
        {
            view.ExplorationView.Initialize();
            TopPageView.ViewModel.ViewStatus = new ViewStatus();
            previewingColumnViewModel = null;
            selectedPageViews.Clear();
        }

        public void PreviewColumn(ColumnViewModel columnViewModel)
        {
            mainPageViewModel.TableViewModel.SkipAllAnimationToFill();

            previewingColumnViewModel = columnViewModel;

            if (ViewStatus.SelectedColumnViewModels.IndexOf(previewingColumnViewModel) >= 0)
                return;

            // 현재 선택된 view status를 가져옴
            ViewStatus selectedViewStatus = ViewStatus.Clone();
            
            // 새 컬럼 추가
            selectedViewStatus.SelectedColumnViewModels.Add(columnViewModel);
            selectedViewStatus.Refresh();

            // 현재 추가된 컬럼으로 groupedRows와 GroupedRowViewModels를 만듦. 
            selectedViewStatus.Generate(mainPageViewModel.SheetViewModel);

            // 탑 페이지 뷰 모델 가져와서
            PageViewModel pageViewModel = TopPageView.ViewModel;

            // 복제한 view status를 추가 한 다음 
            pageViewModel.ViewStatus = selectedViewStatus;
            pageViewModel.State = PageViewModel.PageViewState.Previewing;
            
            // 이걸로 pageView를 채움
            pageViewModel.Reflect(ReflectReason.Preview); // ReflectType2.TrackPreviousParagraph | ReflectType2.OnCreate, ReflectReason2.PreviewRequested);
            TopPageView.ReflectState();

            ExpandFilters();   
        }

        public void CancelPreviewColumn()
        {
            previewingColumnViewModel = null;
            if (TopPageView.ViewModel.State == PageViewModel.PageViewState.Previewing)
            {
                TopPageView.ViewModel.State = PageViewModel.PageViewState.Empty;
                TopPageView.ViewModel.Reflect(ReflectReason.CancelPreview); // ReflectType2.Default, ReflectReason2.PreviewRequested);
                TopPageView.ReflectState();

                ExpandFilters();
            }
        }

        void ExpandFilters()
        {
            PageViewModel pageViewModel = TopPageView.ViewModel;

            foreach (ColumnViewModel cvm in mainPageViewModel.FilterViewModel.FilterColumnViewModels)
            {
                cvm.IsExpanded = pageViewModel.ViewStatus.SelectedColumnViewModels.Contains(cvm);
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

                mainPageViewModel.ReflectAll(ReflectReason.Undo);

                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());

                // Preview 풀기
                mainPageViewModel.TableViewModel.CancelIndexing(false);

                // Page View 위로 보내기                    
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

                Logger.Instance.Log($"column selected,{String.Join(",", ViewStatus.SelectedColumnViewModels.Select(cvm => cvm.Column.Name))}");
                mainPageViewModel.ReflectAll(ReflectReason.ColumnSelected);
                
                //view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());
                //항상 움직이면 안됨. 애니메이션이 있으면 시작점으로 움직여야 하지만 애니메이션이 없는 경우 그대로 있어야함.

                // Preview 풀기
                mainPageViewModel.TableViewModel.CancelIndexing(false);

                pageView.ReflectState();
            }
            else if(state == PageViewModel.PageViewState.Undoing || state == PageViewModel.PageViewState.Empty) // 선택해제하는 경우 혹은 스트로크로 바로 컬럼 지우는 경우
            {
                mainPageViewModel.TableViewModel.StashViewStatus(ViewStatus, AnimationHint.AnimationType.DependOnViewStatus);

                selectedPageViews.Remove(pageView);

                // Preview 풀기 (만약 되어있다면)
                mainPageViewModel.TableViewModel.CancelIndexing(false);

                // 기존 탑 페이지 지우고 이걸 탑 페이지로 지정
                view.ExplorationView.RemoveTopPage(pageView);

                //mainPageViewModel.ReflectAll(true, ReflectReason.ColumnViewModelUnselected);
                mainPageViewModel.ReflectAll(ReflectReason.ColumnUnselected); // ReflectReason2.ColumnViewModelUnselected); // performance 이슈로 위에것만 업데이트 해봄
                Logger.Instance.Log($"column unselected,{String.Join(",", ViewStatus.SelectedColumnViewModels.Select(cvm => cvm.Column.Name))}");

                view.TableView.ScrollToColumnViewModel(mainPageViewModel.SheetViewModel.ColumnViewModels.OrderBy(c => c.Order).First());

                // Page View 위로 올리기
                pageView.ReflectState();
            }            
        }                
    }
}
