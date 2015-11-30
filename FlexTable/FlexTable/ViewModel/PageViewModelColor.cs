using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3.ColorScheme;
using FlexTable.Model;
using Windows.UI;

namespace FlexTable.ViewModel
{
    public partial class PageViewModel : NotifyViewModel
    {
        private static readonly Color DefaultRowHeaderColor = Colors.Black;
        private static readonly Color Category10FirstColor = Category10.Colors[0];

        /// <summary>
        /// This function colors rowViewModels according to the current viewStatus. Only called when a column is just seledted or unselected.
        /// This function is not called when a column is being previewed.
        /// </summary>
        /// <param name="viewStatus"></param>
        /// <param name="allRowViewModels"></param>
        /// <param name="groupedRowViewModels"></param>
        public static void ColorRowViewModels(
            ViewStatus viewStatus, 
            List<RowViewModel> allRowViewModels, 
            List<RowViewModel> groupedRowViewModels,
            List<GroupedRows> groupedRows
            )
        {
            Int32 index = 0;
            Dictionary<Row, Color> dictionary = new Dictionary<Row, Color>();

            if(viewStatus.IsEmpty)
            {
                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = DefaultRowHeaderColor;
            }
            else if (viewStatus.IsC)
            {
                index = 0;
                ColumnViewModel categorical = viewStatus.FirstCategorical;
                foreach(GroupedRows grs in groupedRows)
                {
                    foreach(Row row in grs.Rows)
                    {
                        dictionary[row] = (row.Cells[categorical.Index].Content as Category).Color;
                    }
                    groupedRowViewModels[index].Color = (grs.Keys[categorical] as Category).Color;
                    index++;
                }

                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = dictionary.ContainsKey(rowViewModel.Row) ? dictionary[rowViewModel.Row] : DefaultRowHeaderColor;
            }
            else if (viewStatus.IsN)
            {
                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = Category10FirstColor;
            }
            else if(viewStatus.IsCC)
            {
                index = 0;
                ColumnViewModel categorical = viewStatus.SecondCategorical;
                foreach (GroupedRows grs in groupedRows)
                {
                    foreach (Row row in grs.Rows)
                    {
                        dictionary[row] = (row.Cells[categorical.Index].Content as Category).Color;
                    }
                    groupedRowViewModels[index].Color = (grs.Keys[categorical] as Category).Color;
                    index++;
                }

                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = dictionary.ContainsKey(rowViewModel.Row) ? dictionary[rowViewModel.Row] : DefaultRowHeaderColor;
            }
            else if (viewStatus.IsCN)
            {
                if (viewStatus.IsLineChartVisible)
                {
                    foreach (RowViewModel rowViewModel in groupedRowViewModels) rowViewModel.Color = Category10FirstColor;
                    foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = Category10FirstColor;
                }
                else
                {
                    index = 0;
                    ColumnViewModel categorical = viewStatus.FirstCategorical;
                    foreach (GroupedRows grs in groupedRows)
                    {
                        foreach (Row row in grs.Rows)
                        {
                            dictionary[row] = (row.Cells[categorical.Index].Content as Category).Color;
                        }
                        groupedRowViewModels[index].Color = (grs.Keys[categorical] as Category).Color;
                        index++;
                    }

                    foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = dictionary.ContainsKey(rowViewModel.Row) ? dictionary[rowViewModel.Row] : DefaultRowHeaderColor;
                }
            }
            else if (viewStatus.IsNN)
            {
                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = Category10FirstColor;
                foreach (RowViewModel rowViewModel in groupedRowViewModels) rowViewModel.Color = DefaultRowHeaderColor;
            }
            else if (viewStatus.IsCCN)
            {
                index = 0;
                ColumnViewModel categorical = viewStatus.SecondCategorical;
                foreach (GroupedRows grs in groupedRows)
                {
                    foreach (Row row in grs.Rows)
                    {
                        dictionary[row] = (row.Cells[categorical.Index].Content as Category).Color;
                    }
                    groupedRowViewModels[index].Color = (grs.Keys[categorical] as Category).Color;
                    index++;
                }

                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = dictionary.ContainsKey(rowViewModel.Row) ? dictionary[rowViewModel.Row] : DefaultRowHeaderColor;
            }
            else if (viewStatus.IsCNN)
            {
                // grouped by 는 색깔 안넣는걸로 
                foreach (RowViewModel rowViewModel in groupedRowViewModels) rowViewModel.Color = DefaultRowHeaderColor;

                // all row는 categorical 별로 색깔
                ColumnViewModel categorical = viewStatus.FirstCategorical;
                foreach (RowViewModel rowViewModel in allRowViewModels) rowViewModel.Color = (rowViewModel.Row.Cells[categorical.Index].Content as Category).Color;
            }
            /*else if (categoricalCount == 0 && numericalCount == 1)
            {
                DrawDescriptiveStatistics(numericalColumns.First(), IsSelected);
                DrawDistributionHistogram(numericalColumns.First(), IsSelected);
            }
            else if (categoricalCount == 2 && numericalCount == 0)
            {
                DrawGroupedBarChart(categoricalColumns[0], categoricalColumns[1], groupedRows, IsSelected);
            }
            else if (categoricalCount == 1 && numericalCount == 1)
            {
                DrawBarChart(categoricalColumns.First(), numericalColumns.First(), groupedRows, IsSelected);
                DrawLineChart(categoricalColumns.First(), numericalColumns.First(), groupedRows, IsSelected);

                firstChartTag = categoricalColumns[0].CategoricalType == CategoricalType.Ordinal ? pageView.LineChart.Tag : pageView.BarChart.Tag;
            }
            else if (categoricalCount == 0 && numericalCount == 2)
            {
                DrawCorrelatonStatistics(numericalColumns[0], numericalColumns[1], IsSelected);
                DrawScatterplot(numericalColumns[0], numericalColumns[1], IsSelected);
            }
            else if (categoricalCount == 3 && numericalCount == 0)
            {
                IsPivotTableVisible = true;
                pageView.PivotTableTitle.Children.Clear();
                if (IsSelected)
                {
                    DrawEditableTitleCxNx(pageView.PivotTableTitle,
                        "Frequency of\x00A0",
                        new List<ColumnViewModel>() { categoricalColumns[2] },
                        "\x00A0by\x00A0",
                        categoricalColumns.Where((cvm, i) => i < 2).ToList()
                        );
                }
                else
                {
                    AddText(pageView.PivotTableTitle, $"Frequency of <b>{categoricalColumns[2].Name}</b> by <b>{categoricalColumns[0].Name}</b> and <b>{categoricalColumns[1].Name}</b>");
                }
                pivotTableViewModel.Preview(
                    selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ToList(),
                    categoricalColumns.Last(),
                    new List<ColumnViewModel>(),
                    groupedRows
                    );
            }
            else if (categoricalCount == 2 && numericalCount == 1)
            {
                // 테이블을 그린다
                IsPivotTableVisible = true;
                pageView.PivotTableTitle.Children.Clear();
                if (IsSelected)
                {
                    DrawEditableTitleCxNx(pageView.PivotTableTitle,
                        "",
                        numericalColumns,
                        "\x00A0by\x00A0",
                        categoricalColumns
                        );
                }
                else
                {
                    AddText(pageView.PivotTableTitle, $"<b>{numericalColumns[0].HeaderName}</b> by <b>{categoricalColumns[0].Name}</b> and <b>{categoricalColumns[1].Name}</b>");
                }

                pivotTableViewModel.Preview(
                    new List<ColumnViewModel>() { categoricalColumns[0] },
                    categoricalColumns[1],
                    numericalColumns,
                    groupedRows
                    );

                DrawGroupedBarChart(categoricalColumns[0], categoricalColumns[1], numericalColumns[0], groupedRows, IsSelected);
                DrawLineChart(categoricalColumns[0], categoricalColumns[1], numericalColumns[0], groupedRows, IsSelected);
                firstChartTag = categoricalColumns[0].CategoricalType == CategoricalType.Ordinal ? pageView.LineChart.Tag : pageView.BarChart.Tag;
            }
            else if (categoricalCount == 1 && numericalCount == 2)
            {
                // 스캐터플롯을 그린다.
                DrawScatterplot(categoricalColumns.First(), numericalColumns[0], numericalColumns[1], IsSelected);

                if (numericalColumns[0].Unit == numericalColumns[1].Unit) // 둘의 단위가 같으면 그룹 바 차트 가능
                {
                    DrawGroupedBarChartCNN(categoricalColumns[0], numericalColumns[0], numericalColumns[1], groupedRows, IsSelected);
                }
                // 테이블을 그린다

                IsPivotTableVisible = true;
                pageView.PivotTableTitle.Children.Clear();
                DrawEditableTitleCNN(pageView.PivotTableTitle, categoricalColumns[0], numericalColumns[0], numericalColumns[1]);

                pivotTableViewModel.Preview(
                    categoricalColumns,
                    null,
                    numericalColumns,
                    groupedRows
                    );
            }
            else if (categoricalCount == 0 && numericalCount == 3)
            {
                IsBarChartVisible = true;
                // 지금 필요없다.
            }
            else if (categoricalCount >= 1 && numericalCount == 0)
            {
                // 테이블을 그린다
                IsPivotTableVisible = true;
                pageView.PivotTableTitle.Children.Clear();
                if (IsSelected)
                {
                    DrawEditableTitleCxNx(pageView.PivotTableTitle,
                        "Frequency of\x00A0",
                        new List<ColumnViewModel>() { categoricalColumns.Last() },
                        "\x00A0by\x00A0",
                        categoricalColumns.Where((cvm, i) => i != categoricalColumns.Count - 1).ToList()
                        );
                }
                else
                {
                    AddText(pageView.PivotTableTitle, $"Frequency of <b>{categoricalColumns.Last().Name}</b> " +
                      $"by {Concatenate(categoricalColumns.Where((c, index) => index != categoricalColumns.Count - 1).Select(s => "<b>" + s.Name + "</b>"))}");
                }

                pivotTableViewModel.Preview(
                    categoricalColumns.Where((c, index) => index != categoricalColumns.Count - 1).ToList(),
                    categoricalColumns.Last(),
                    numericalColumns,
                    groupedRows
                    );
            }
            else if (categoricalCount >= 1 && numericalCount == 1)
            {
                // 테이블을 그린다
                IsPivotTableVisible = true;
                pageView.PivotTableTitle.Children.Clear();
                if (IsSelected)
                {
                    DrawEditableTitleCxNx(pageView.PivotTableTitle,
                        "",
                        numericalColumns,
                        "\x00A0by\x00A0",
                        categoricalColumns
                        );
                }
                else
                {
                    AddText(pageView.PivotTableTitle, $"<b>{numericalColumns[0].HeaderName}</b> by {Concatenate(categoricalColumns.Select(s => "<b>" + s.Name + "</b>"))}");
                }

                pivotTableViewModel.Preview(
                    categoricalColumns.Where((c, index) => index != categoricalColumns.Count - 1).ToList(),
                    categoricalColumns.Last(),
                    numericalColumns,
                    groupedRows
                    );
            }
            else if (categoricalCount >= 1 && numericalCount > 1)
            {
                // 테이블을 그린다
                IsPivotTableVisible = true;
                pageView.PivotTableTitle.Children.Clear();
                if (IsSelected)
                {
                    DrawEditableTitleCxNx(pageView.PivotTableTitle,
                        "",
                        numericalColumns,
                        "\x00A0by\x00A0",
                        categoricalColumns
                        );
                }
                else
                {
                    AddText(pageView.PivotTableTitle, $"{Concatenate(numericalColumns.Select(s => "<b>" + s.HeaderName + "</b>"))} by {Concatenate(categoricalColumns.Select(s => "<b>" + s.Name + "</b>"))}");
                }

                if (numericalCount * categoricalColumns.Last().Categories.Count <= 12)
                {
                    pivotTableViewModel.Preview(
                        categoricalColumns.Where((c, index) => index != categoricalColumns.Count - 1).ToList(),
                        categoricalColumns.Last(),
                        numericalColumns,
                        groupedRows
                        );
                }
                else
                {
                    pivotTableViewModel.Preview(
                        categoricalColumns,
                        null, // 여길 채워서 남은 카테고리컬 하나를 여기로 시각화 가능
                        numericalColumns,
                        groupedRows
                        );
                }
            }*/
        }
    }
}
