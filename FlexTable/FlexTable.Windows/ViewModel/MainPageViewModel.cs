using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace FlexTable.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private Model.Sheet sheet;
        public Model.Sheet Sheet
        {
            get { return sheet; }
            set { sheet = value; OnPropertyChanged("Sheet"); }
        }

        private Rect bounds;
        public Double Width { get { return bounds.Width; } }
        public Double Height { get { return bounds.Height; } }

        public event PropertyChangedEventHandler PropertyChanged;
        private List<RowPresenter> rowPresenters = new List<RowPresenter>();
        public List<RowPresenter> RowPresenters { get { return rowPresenters; } }

        public MainPageViewModel()
        {
            bounds = Window.Current.Bounds;
            OnPropertyChanged("Width");
            OnPropertyChanged("Height");
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RowShuffle()
        {
            Random random = new Random();
            for (Int32 i = 0; i < 1000; ++i)
            {
                Int32 x = random.Next(sheet.RowCount), y = random.Next(sheet.RowCount);
                Int32 temp;
                temp = sheet.Rows[x].Index;
                sheet.Rows[x].Index = sheet.Rows[y].Index;
                sheet.Rows[y].Index = temp;
            }

            foreach (Model.Row row in sheet.Rows)
            {
                row.OnPropertyChanged("Y");
                row.OnPropertyChanged("Opacity");
            }

            foreach(RowPresenter rowPresenter in rowPresenters) {
                rowPresenter.Update();
            }
        }
    }
}
