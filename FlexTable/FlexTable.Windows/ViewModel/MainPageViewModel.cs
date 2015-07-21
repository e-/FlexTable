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

        private SummaryViewModel summaryViewModel;
        public SummaryViewModel SummaryViewModel { get { return summaryViewModel; } set { summaryViewModel = value; OnPropertyChanged("SummaryViewModel"); } }

        private ChartViewModel chartViewModel;
        public ChartViewModel ChartViewModel { get { return chartViewModel; } set { chartViewModel = value; OnPropertyChanged("ChartViewModel"); } }

        private TableViewModel tableViewModel;
        public TableViewModel TableViewModel { get { return tableViewModel; } set { tableViewModel = value; OnPropertyChanged("TableViewModel"); } }

        private ExplorationViewModel explorationViewModel;
        public ExplorationViewModel ExplorationViewModel { get { return explorationViewModel; } set { explorationViewModel = value; OnPropertyChanged("ExplorationViewModel"); } }

        public MainPageViewModel(IMainPage view)
        {
            this.view = view;

            SummaryViewModel = new SummaryViewModel(this, view);            
            ChartViewModel = new ChartViewModel(this);
            SheetViewModel = new SheetViewModel(this, view);
            TableViewModel = new TableViewModel(this, view);
            ExplorationViewModel = new ExplorationViewModel(this, view);
        }

        public void Initialize()
        {
            sheetViewModel.Initialize(sheet);
            tableViewModel.UpdateRows();
            view.TableView.AddGuidelines(sheet.Rows.Count);


            ViewModel.PageViewModel pageViewModel = new ViewModel.PageViewModel(
                this,
                view.ExplorationView.TopPageView
                );
            view.ExplorationView.TopPageView.DataContext = pageViewModel;

            //SummaryViewModel.ShowSummary(sheetViewModel.ColumnViewModels[0]);
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
