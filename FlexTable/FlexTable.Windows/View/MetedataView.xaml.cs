using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class MetedataView : UserControl
    {
        public MetedataView()
        {
            this.InitializeComponent();
        }

        private async void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = await Util.FileLoader.Instance.Open();
            if (file == null)
                return;

            var content = await Windows.Storage.FileIO.ReadTextAsync(file);
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            StreamReader sr = new StreamReader(ms);
            var parser = new CsvParser(sr);
            Model.Sheet sheet = new Model.Sheet()
            {
                Name = file.DisplayName
            };

            var header = parser.Read();
            if (header == null) // no header row
                return;

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
                        RawContent = cellValue
                    });
                }
            }

            ViewModel.MainPageViewModel mpvm = (this.DataContext as ViewModel.MetadataViewModel).MainPageViewModel;

            mpvm.View.TableView.TopColumnHeader.Reset();
            mpvm.View.TableView.BottomColumnHeader.Reset();
            mpvm.Sheet = sheet;
            mpvm.Initialize();
        }
    }
}
