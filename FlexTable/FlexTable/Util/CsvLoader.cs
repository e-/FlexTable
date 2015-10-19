using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using FlexTable.Model;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FlexTable.Util
{
    public class CsvLoader
    {
        public static async Task<Sheet> Load(String name)
        {
            //String name = "Population-filtered.csv"; // "economic-condition2.csv";// "Insurance.csv"; // "Population-small.csv";
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Data");
            var file = await folder.GetFileAsync(name);
            return await Load(file);
        }

        public static async Task<Sheet> Load(StorageFile file)
        {
            IBuffer buffer = await FileIO.ReadBufferAsync(file);
            DataReader reader = DataReader.FromBuffer(buffer);
            byte[] fileContent = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(fileContent);
            string content = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            StreamReader sr = new StreamReader(ms);
            var parser = new CsvParser(sr);
            Sheet sheet = new Sheet()
            {
                Name = file.Name
            };

            var header = parser.Read();
            if (header == null) // no header row
                return null;

            foreach (String columnName in header)
            {
                sheet.Columns.Add(new Column()
                {
                    Name = columnName
                });
            }

            Boolean firstRow = true;

            while (true) // body
            {
                var cellValues = parser.Read();

                if (cellValues == null)
                    break;

                if(firstRow && CheckColumnInfo(cellValues, sheet.Columns))
                {
                    firstRow = false;
                    Int32 index = 0;
                    foreach(String cellValue in cellValues)
                    {
                        if (cellValue.ToLower() == "nominal")
                        {
                            sheet.Columns[index].Type = ColumnType.Categorical;
                            sheet.Columns[index].CategoricalType = CategoricalType.Nominal;
                        }
                        else if (cellValue.ToLower() == "ordinal")
                        {
                            sheet.Columns[index].Type = ColumnType.Categorical;
                            sheet.Columns[index].CategoricalType = CategoricalType.Ordinal;
                        }
                        else
                        {
                            sheet.Columns[index].Type = ColumnType.Numerical;
                            sheet.Columns[index].Unit = cellValue;
                        }
                        index++;
                    }
                    continue;
                }

                Row row = new Row();

                sheet.Rows.Add(row);

                foreach (String cellValue in cellValues)
                {
                    row.Cells.Add(new Cell()
                    {
                        RawContent = cellValue.Trim()
                    });
                }
            }

            return sheet;
        }

        private static Boolean CheckColumnInfo(String[] values, List<Column> columns)
        {
            return true;
            /*if (values.Length != columns.Count) return false;

            foreach(String value in )*/
        }
    }
}
