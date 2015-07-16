using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace FlexTable.Util
{
    public class CsvLoader
    {
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
                        RawContent = cellValue
                    });
                }
            }


            return sheet;
        }
    }
}
