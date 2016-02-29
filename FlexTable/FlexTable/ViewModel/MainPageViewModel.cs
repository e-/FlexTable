using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlexTable.Model;
using FlexTable.View;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.Storage;

namespace FlexTable.ViewModel
{
    public class MainPageViewModel : NotifyViewModel
    {
        private IMainPage view;
        public IMainPage View { get { return view; } }

        private Size bounds;
        public Size Bounds { get { return bounds; } }

        private Model.Sheet sheet;
        public Model.Sheet Sheet {
            get { return sheet; }
            set { 
                sheet = value;
                OnPropertyChanged("Sheet"); 
            }
        }

        private SheetViewModel sheetViewModel;
        public SheetViewModel SheetViewModel { get { return sheetViewModel; } set { sheetViewModel = value; OnPropertyChanged("SheetViewModel"); } }

        private TableViewModel tableViewModel;
        public TableViewModel TableViewModel { get { return tableViewModel; } set { tableViewModel = value; OnPropertyChanged("TableViewModel"); } }

        private ExplorationViewModel explorationViewModel;
        public ExplorationViewModel ExplorationViewModel { get { return explorationViewModel; } set { explorationViewModel = value; OnPropertyChanged("ExplorationViewModel"); } }

        private Double pageHeight;
        public Double PageHeight { get { return pageHeight; } set { pageHeight = value; OnPropertyChanged("PageHeight"); } }

        private Double negativePageHeight;
        public Double NegativePageHeight { get { return negativePageHeight; } set { negativePageHeight = value; OnPropertyChanged("NegativePageHeight"); } }

        private Double pageWidth;
        public Double PageWidth { get { return pageWidth; } set { pageWidth = value; OnPropertyChanged("PageWidth"); } }

        private Double pageOffset;
        public Double PageOffset { get { return pageOffset; } set { pageOffset = value; OnPropertyChanged("PageOffset"); } }

        private Double paragraphWidth = 794;
        public Double ParagraphWidth
        {
            get { return paragraphWidth; }
            set { paragraphWidth = value; OnPropertyChanged(nameof(ParagraphWidth)); }
        }
        
        private Double paragraphHeight = 410;
        public Double ParagraphHeight { get { return paragraphHeight; } set { paragraphHeight = value; OnPropertyChanged(nameof(ParagraphHeight)); } }

        private Double paragraphTitleHeight = 90;
        public Double ParagraphTitleHeight { get { return paragraphTitleHeight; } set { paragraphTitleHeight = value; OnPropertyChanged(nameof(ParagraphTitleHeight)); } }

        private Double paragraphChartHeight = 410;
        public Double ParagraphChartHeight { get { return paragraphChartHeight; } set { paragraphChartHeight = value; OnPropertyChanged(nameof(ParagraphChartHeight)); } }
        
        private Double pageHeaderHeight = 50;
        public Double PageHeaderHeight { get { return pageHeaderHeight; } set { pageHeaderHeight = value; OnPropertyChanged(nameof(PageHeaderHeight)); } }

        private Double pageFooterHeight = 50;
        public Double PageFooterHeight { get { return pageFooterHeight; } set { pageFooterHeight = value; OnPropertyChanged(nameof(PageFooterHeight)); } }

        private Double pageLabelCarouselHeight = 50;
        public Double PageLabelCarouselHeight { get { return pageLabelCarouselHeight; } set { pageLabelCarouselHeight = value; OnPropertyChanged(nameof(PageLabelCarouselHeight)); } }

        /*
         <x:Double x:Key="ParagraphWidth">794</x:Double> <!-- 794인데 마지막 축 레이블 튀어나가서 줄임 -->
            <x:Double x:Key="ParagraphHeight">410</x:Double>
            <x:Double x:Key="ParagraphContainerHeight">500</x:Double> <!-- ParagraphHeight + 90 (titleheight) -->
            */

        public MainPageViewModel(IMainPage view)
        {
            this.view = view;

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;

            var scaleFactor = 1.0; // DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            this.bounds = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            SheetViewModel = new SheetViewModel(this, view);
            TableViewModel = new TableViewModel(this, view);
            ExplorationViewModel = new ExplorationViewModel(this, view);
            Const.Initialize();
        }

        /// <summary>
        /// 현재 ExplorationViewModel의 ViewStatus를 통해 SheetViewModel, TableViewModel, PageView들을 모두 업데이트 합니다
        /// </summary>
        public void ReflectAll(ReflectReason reason)
        {
            ReflectAll(ExplorationViewModel.ViewStatus, reason);
        }

        public void ReflectAll(ViewStatus viewStatus, ReflectReason reason)
        {
            //viewStatus.Generate(sheetViewModel);

            Boolean isN = viewStatus.IsN;
            foreach (ColumnViewModel cvm in SheetViewModel.ColumnViewModels)
            {
                cvm.IsSelected = viewStatus.SelectedColumnViewModels.IndexOf(cvm) >= 0;
                if(isN && cvm.IsSelected && cvm.SortOption == SortOption.None)
                {
                    SheetViewModel.Sort(cvm, SortOption.Ascending);
                }
            }

            viewStatus.Generate(SheetViewModel);

            // sheet 업데이트
            SheetViewModel.Reflect(viewStatus);

            // 테이블 업데이트
            TableViewModel.Reflect(viewStatus, reason);

            Boolean updateAllPageViews = reason.UpdateAllPageViews();
            Boolean updateLastPageView = reason.UpdateLastPageView();

            // 차트 업데이트
            if (updateAllPageViews)
            {
                foreach (PageView pageView in ExplorationViewModel.SelectedPageViews)
                {
                    if (pageView.ViewModel.ViewStatus != viewStatus)
                        pageView.ViewModel.ViewStatus.Generate(sheetViewModel);

                    pageView.ViewModel.Reflect(reason);

                    /*if (pageView.SelectedRows.Count() > 0)
                        pageView.ViewModel.Reflect(reason); // ReflectType2.TrackPreviousParagraph | ReflectType2.OnCreate | ReflectType2.OnSelectionChanged, reason);
                    else
                        pageView.ViewModel.Reflect(reason); // ReflectType2.TrackPreviousParagraph | ReflectType2.OnCreate, reason);*/
                }
            }
            else if (updateLastPageView)
            {
                if (ExplorationViewModel.SelectedPageViews.Count > 0)
                {
                    PageView pageView = ExplorationViewModel.SelectedPageViews.Last();
                    if (pageView.ViewModel.ViewStatus != viewStatus)
                        pageView.ViewModel.ViewStatus.Generate(sheetViewModel);

                    pageView.ViewModel.Reflect(reason);

                    /*
                    if (pageView.SelectedRows.Count() > 0)
                        pageView.ViewModel.Reflect(reason); // ReflectType2.TrackPreviousParagraph | ReflectType2.OnCreate | ReflectType2.OnSelectionChanged, reason);
                    else
                        pageView.ViewModel.Reflect(reason); // ReflectType2.TrackPreviousParagraph | ReflectType2.OnCreate, reason);*/
                }
            }
        }

        public async Task Initialize()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            Sheet sheet;

            if (localSettings.Values.ContainsKey("recent"))
            {
                String recent = (String)localSettings.Values["recent"];

                try
                {
                    sheet = await Util.CsvLoader.LoadLocal(recent);
                }
                catch
                {
                    sheet = await Util.CsvLoader.Load("student-mat.csv");
                }
            }
            else
            {
                sheet = await Util.CsvLoader.Load("student-mat.csv");
            }

            Initialize(sheet);
        }

        public void Initialize(Sheet sheet)
        {
            this.Sheet = sheet;

            PageHeight = Bounds.Height / 2 - 4;// Bounds.Height; 
            NegativePageHeight = -PageHeight;
            PageOffset = PageHeight + 8;
            PageWidth = Bounds.Width / 2;
            ParagraphWidth = PageWidth - 40;

            PageHeaderHeight = 0; // 110; 

            ParagraphTitleHeight = 90;
            PageLabelCarouselHeight = 50;

            PageFooterHeight = 0; // 110;

            ParagraphHeight = PageHeight - PageHeaderHeight - PageLabelCarouselHeight - PageFooterHeight - 8;
            ParagraphChartHeight = ParagraphHeight - ParagraphTitleHeight;

            // rowViewModels 계산
            sheetViewModel.Initialize(sheet);

            // 가이드라인 및 헤더 컬럼 추가
            tableViewModel.Initialize();

            explorationViewModel.Initialize();

            // 메타데이터 초기화
            ExplorationViewModel.MetadataViewModel.Initialize();

            var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            ExplorationViewModel.PreviewColumn(SheetViewModel.ColumnViewModels[3]);
            
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (sender, e) =>
            {
                dispatcherTimer.Stop();

                ExplorationViewModel.TopPageView.ViewModel.State = PageViewModel.PageViewState.Selected;
                ExplorationViewModel.PageViewStateChanged(ExplorationViewModel.TopPageView.ViewModel, ExplorationViewModel.TopPageView);
                
                //PageView topPageView = ExplorationViewModel.SelectedPageViews.Last();
                //topPageView.SelectionChanged(null, sheetViewModel.FilteredRows.Where((r, index) => index < 50).ToList(), SelectionChangedType.Add, ReflectReason.ChartSelection);

                ExplorationViewModel.PreviewColumn(SheetViewModel.ColumnViewModels[4]);

                DispatcherTimer dispatcherTimer2 = new DispatcherTimer();
                dispatcherTimer2.Tick += (sender2, e2) =>
                {
                    dispatcherTimer2.Stop();

                    ExplorationViewModel.TopPageView.ViewModel.State = PageViewModel.PageViewState.Selected;
                    ExplorationViewModel.PageViewStateChanged(ExplorationViewModel.TopPageView.ViewModel, ExplorationViewModel.TopPageView);

                    return;

                    ExplorationViewModel.PreviewColumn(SheetViewModel.ColumnViewModels[4]);

                    DispatcherTimer dispatcherTimer3 = new DispatcherTimer();
                    dispatcherTimer3.Tick += (sender3, e3) =>
                    {
                        dispatcherTimer3.Stop();
                        ExplorationViewModel.TopPageView.ViewModel.State = PageViewModel.PageViewState.Selected;
                        ExplorationViewModel.PageViewStateChanged(ExplorationViewModel.TopPageView.ViewModel, ExplorationViewModel.TopPageView);
                    };
                    dispatcherTimer3.Interval = TimeSpan.FromMilliseconds(500);
                    dispatcherTimer3.Start();
                    
                };
                dispatcherTimer2.Interval = TimeSpan.FromMilliseconds(300);
                dispatcherTimer2.Start();
            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
            dispatcherTimer.Start();
        }    
    }
}
