using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Model
{
    public class MetadataStatisticsResult
    {
        public Int32 RowCount { get; set; }
        public Int32 ColumnCount { get; set; }
        public Int32 CategoricalCount { get; set; }
        public Int32 NumericalCount { get; set; }
    }

    public class MetadataStatistics
    {
        public static MetadataStatisticsResult Analyze(Sheet sheet, ViewModel.SheetViewModel viewModel)
        {
            return new MetadataStatisticsResult()
            {
                RowCount = sheet.Rows.Count,
                ColumnCount = sheet.Columns.Count,
                CategoricalCount = viewModel.ColumnViewModels.Count(c => c.Type == ColumnType.Categorical),
                NumericalCount = viewModel.ColumnViewModels.Count(c => c.Type == ColumnType.Numerical)
            };
        }
    }
}
