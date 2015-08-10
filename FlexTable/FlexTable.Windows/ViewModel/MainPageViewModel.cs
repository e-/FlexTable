using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace FlexTable.ViewModel
{
    public class MainPageViewModel : NotifyViewModel
    {
        private IMainPage view;
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

        public MainPageViewModel(IMainPage view)
        {
            this.view = view;

            SheetViewModel = new SheetViewModel(this, view);
            TableViewModel = new TableViewModel(this, view);
            ExplorationViewModel = new ExplorationViewModel(this, view);
        }

        public void Initialize()
        {
            // rowViewModels 계산
            sheetViewModel.Initialize(sheet);

            // 가이드라인 및 헤더 컬럼 추가
            tableViewModel.Initialize();

            var watch = Stopwatch.StartNew();
            
            // rowViewModels 추가된 것 반영 여기가 시간 많이 듬
            // 전체 컬럼을 가지고 있는 것으로 이분화 하기로 했으므로 updateRows대신 CreateAllRows를 함

            tableViewModel.CreateAllRows();

            // tableViewModel.UpdateRows();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.WriteLine("elapsed " + elapsedMs);

            ViewModel.PageViewModel pageViewModel = new ViewModel.PageViewModel(
                this,
                view.ExplorationView.TopPageView
                );
            view.ExplorationView.TopPageView.DataContext = pageViewModel;
            
            // 메타데이터 초기화
            ExplorationViewModel.MetadataViewModel.Initialize();
        }  


     

        public void UpdateFiltering()
        {
            /*foreach (Model.Row row in sheet.Rows)
            {
                row.IsFilteredOut = false;
            }

            foreach (Model.Column column in Sheet.Columns)
            {
                foreach (Model.Bin bin in column.Bins.Where(b => b.IsFilteredOut))
                {
                    foreach (Model.Row row in bin.Rows)
                    {
                        row.IsFilteredOut = true;
                    }
                }
            }

            Int32 index = 0;
            foreach (Model.Row row in sheet.Rows)
            {
                if (!row.IsFilteredOut)
                {
                    row.Index = index++;
                }
            }

            foreach (Model.Row row in sheet.Rows)
            {
                row.OnPropertyChanged("Y");
                row.OnPropertyChanged("IsFilteredOut");
            }

            foreach (View.RowPresenter rowPresenter in rowPresenters)
            {
                rowPresenter.Update();
            }*/
        }
    }
}
