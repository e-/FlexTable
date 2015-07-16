using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace FlexTable.Model
{
    public class Sheet : INotifyPropertyChanged
    {
        private String name;
        public String Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }

        private ObservableCollection<Model.Column> columns = new ObservableCollection<Model.Column>();
        public ObservableCollection<Model.Column> Columns { get { return columns; } }

        private ObservableCollection<Model.Row> rows = new ObservableCollection<Model.Row>();
        public ObservableCollection<Model.Row> Rows { get { return rows; } private set { rows = value; OnPropertyChanged("Rows"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public Sheet()
        {
            
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void MeasureColumnWidth(TextBlock dummyCell)
        {
            foreach (Model.Column column in columns)
            {
                String maxValue = (from row in Rows
                                   orderby row.Cells[column.Index].RawContent.Count() descending
                                   select row.Cells[column.Index].RawContent).First();
                dummyCell.Text = maxValue;
                dummyCell.Measure(new Size(Double.MaxValue, Double.MaxValue));

                column.Width = dummyCell.ActualWidth;
            }
        }

        public void UpdateColumnX()
        {
            Double total = 0;
            foreach (Column column in columns.OrderBy(c => c.Index))
            {
                column.X = total;
                total += column.Width;
            }
        }

        public void GuessColumnType()
        {
            Int32 i;
            for (i = 0; i < Columns.Count; ++i)
            {
                columns[i].Type = Model.Column.GuessColumnType(rows.Select(r => r.Cells[i].RawContent));
                if(columns[i].Type == ColumnType.Categorical)
                {
                    foreach (Model.Row row in rows)
                    {
                        row.Cells[i].Content = row.Cells[i].RawContent;
                    }
                }
                else
                {
                    foreach (Model.Row row in rows)
                    {
                        row.Cells[i].Content = Double.Parse(row.Cells[i].RawContent);
                    }
                }
            }
        }

        public void CreateColumnSummary()
        {
            for (Int32 i = 0; i < Columns.Count; ++i)
            {
                Column column = columns[i];
                if (column.Type == ColumnType.Categorical) // bar chart
                {
                    column.Bins = Model.Column.GetFrequencyBins(rows, i);
                }
                else // histogram
                {
                    column.Bins = new List<Bin>();

                    IEnumerable<Double> cellValues = rows.Select(r => (Double)r.Cells[i].Content).OrderBy(v=>v);

                    column.MinValue = cellValues.Min();
                    column.MaxValue = cellValues.Max();
                    column.MeanValue = cellValues.Sum() / cellValues.Count();
                    column.MedianValue = cellValues.ElementAt(cellValues.Count() / 2);
                }
            }
        }
    }
}
