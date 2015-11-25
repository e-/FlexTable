using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

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

        public MainPageViewModel(IMainPage view)
        {
            this.view = view;

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;

            var scaleFactor = 1.0; // DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            this.bounds = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            SheetViewModel = new SheetViewModel(this, view);
            TableViewModel = new TableViewModel(this, view);
            ExplorationViewModel = new ExplorationViewModel(this, view);
        }

        public async void Initialize()
        {
            Model.Sheet sheet = await Util.CsvLoader.Load("who.csv"); // "Population-filtered.csv");
            this.Sheet = sheet;

            PageHeight = Bounds.Height / 2 - 4;
            NegativePageHeight = -PageHeight;
            PageOffset = PageHeight + 8;
            PageWidth = Bounds.Width / 2;

            // rowViewModels 계산
            sheetViewModel.Initialize(sheet);

            // 가이드라인 및 헤더 컬럼 추가
            tableViewModel.Initialize();

            var watch = Stopwatch.StartNew();
            
            // rowViewModels 추가된 것 반영 여기가 시간 많이 듬
            // 전체 컬럼을 가지고 있는 것으로 이분화 하기로 했으므로 updateRows대신 CreateAllRows를 함
            

            // tableViewModel.UpdateRows();


            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.WriteLine("elapsed " + elapsedMs);

            PageViewModel pageViewModel = new PageViewModel(
                this,
                view.ExplorationView.TopPageView
                )
            {
                ViewStatus = new Model.ViewStatus() // 초기 비어있는 뷰 상태로 초기화
            };
            view.ExplorationView.TopPageView.DataContext = pageViewModel;

            // 메타데이터 초기화
            ExplorationViewModel.MetadataViewModel.Initialize();


            var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;


            ExplorationViewModel.PreviewColumn(SheetViewModel.ColumnViewModels[1]);

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (sender, e) =>
            {
                dispatcherTimer.Stop();
                ExplorationViewModel.TopPageView.PageViewModel.State = PageViewModel.PageViewState.Selected;
                ExplorationViewModel.PageViewStateChanged(ExplorationViewModel.TopPageView.PageViewModel, ExplorationViewModel.TopPageView);

                /*ExplorationViewModel.PreviewColumn(SheetViewModel.ColumnViewModels[3]);

                DispatcherTimer dispatcherTimer2 = new DispatcherTimer();
                dispatcherTimer2.Tick += (sender2, e2) =>
                {
                    dispatcherTimer2.Stop();
                    ExplorationViewModel.TopPageView.PageViewModel.State = PageViewModel.PageViewState.Selected;
                    ExplorationViewModel.PageViewStateChanged(ExplorationViewModel.TopPageView.PageViewModel, ExplorationViewModel.TopPageView);


                    ExplorationViewModel.PreviewColumn(SheetViewModel.ColumnViewModels[5]);

                    DispatcherTimer dispatcherTimer3 = new DispatcherTimer();
                    dispatcherTimer3.Tick += (sender3, e3) =>
                    {
                        dispatcherTimer3.Stop();
                        ExplorationViewModel.TopPageView.PageViewModel.State = PageViewModel.PageViewState.Selected;
                        ExplorationViewModel.PageViewStateChanged(ExplorationViewModel.TopPageView.PageViewModel, ExplorationViewModel.TopPageView);
                    };
                    dispatcherTimer3.Interval = TimeSpan.FromMilliseconds(500);
                    dispatcherTimer3.Start();

                };
                dispatcherTimer2.Interval = TimeSpan.FromMilliseconds(500);
                dispatcherTimer2.Start();*/
            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
            dispatcherTimer.Start();

            

            /*ExplorationViewModel.PreviewColumn(SheetViewModel.ColumnViewModels[4]);
            ExplorationViewModel.StatusChanged(ExplorationViewModel.TopPageView.PageViewModel, ExplorationViewModel.TopPageView, false);
            */
        }    
    }
}
