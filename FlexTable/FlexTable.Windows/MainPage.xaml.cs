using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using CsvHelper;
using System.Text;
using System.Threading.Tasks;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace FlexTable
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ViewModel.MainPageViewModel mainPageViewModel = new ViewModel.MainPageViewModel();

        public MainPage()
        {
            this.DataContext = mainPageViewModel;
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*Random random = new Random();

            for (Int32 i = 0; i < 20; ++i)
            {
                Int32 q = random.Next(Data[0].Count);
                Int32 w = random.Next(Data[0].Count);
                Item temp;
                temp = Data[0][q];
                Data[0][q] = Data[0][w];
                Data[0][w] = temp;
            }
            */
            //Data[0].RemoveAt(0);
            //Data[0] = new ObservableCollection<Item>(Data[0].OrderBy(x => random.Next()));
            //Data[1] = new ObservableCollection<Item>(Data[1].OrderBy(x => random.Next()));
        }

        public async Task<Model.Sheet> Load()
        {
            String name = "Insurance.csv";
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Data");
            var file = await folder.GetFileAsync(name);
            var content = await Windows.Storage.FileIO.ReadTextAsync(file);
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            StreamReader sr = new StreamReader(ms);
            var parser = new CsvParser(sr);
            Model.Sheet sheet = new Model.Sheet()
            {
                Name = name
            };


            var header = parser.Read();
            if (header == null) // no header row
                return null;

            foreach (String columnName in header)
            {
                sheet.Columns.Add(new Model.Column()
                {
                    Name = columnName
                });
            }

            while (true) // body
            {
                var cellValues = parser.Read();

                if (cellValues == null)
                    break;

                Model.Row row = new Model.Row();
                sheet.Rows.Add(row);

                foreach (String cellValue in cellValues)
                {
                    row.Cells.Add(new Model.Cell()
                    {
                        Content = cellValue
                    });
                }
            }
            
            return sheet;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            mainPageViewModel.Sheet = await Load();
        }
    }
}
