using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.View;
using Windows.UI.Xaml.Controls;
using FlexTable.Model;
using Windows.UI.Xaml;
using System.Diagnostics;
using Windows.UI.Xaml.Documents;
using FlexTable.Util;
using Windows.UI.Xaml.Media;

namespace FlexTable.ViewModel
{
    public class PivotTableViewModel
    {
        const Int32 MaximumCellNumber = 100;
        const Int32 MaximumRowNumber = 20;

        private MainPageViewModel mainPageViewModel;

        private PivotTableView pivotTableView;

        public PivotTableViewModel(MainPageViewModel mainPageViewModel, PivotTableView pivotTableView)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.pivotTableView = pivotTableView;
        }

        private Border GetNewHeaderBorder()
        {
            return new Border()
            {
                Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
            };
        }

        private TextBlock GetNewHeaderTextBlock(String text)
        {
            return new TextBlock()
            {
                Text = text,
                Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
            };
        }

        public void Preview(ViewStatus viewStatus)
        {
            UIElementCollection children = pivotTableView.PivotTable.Children;

            children.Clear();
            pivotTableView.PivotTable.ColumnDefinitions.Clear();
            pivotTableView.PivotTable.RowDefinitions.Clear();

            var groupedRows = viewStatus.GroupedRows;
            var groupedRowViewModels = viewStatus.GroupedRowViewModels;
            var selectedColumnViewModels = viewStatus.SelectedColumnViewModels;

            if (viewStatus.IsN)
            {
                Int32 rowN = groupedRows.Count + 1, // 테이블 전체 로우의 개수
                      columnN = 2;

                Int32 i, index, j;

                // 개수만큼 추가 컬럼 및 로우 정의 추가. 이중선 말고는 별 특별한 점 없음.
                for (i = 0; i < rowN; ++i)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Height = GridLength.Auto;
                    pivotTableView.PivotTable.RowDefinitions.Add(rowDefinition);
                }

                for (i = 0; i < columnN; ++i)
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    pivotTableView.PivotTable.ColumnDefinitions.Add(columnDefinition);
                }

                // 가로 이름 넣기 (인덱스 열 포함)

                index = 0;
                foreach (ColumnViewModel columnViewModel in selectedColumnViewModels)
                {
                    Border border = new Border()
                    {
                        Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = columnViewModel.FormatHeaderName(viewStatus),
                        Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
                    };
                    border.Child = textBlock;

                    children.Add(border);
                    Grid.SetRow(border, 0);
                    Grid.SetColumn(border, index++);
                }

                {
                    Border border = new Border()
                    {
                        Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = $"Count({selectedColumnViewModels.First().Name})",
                        Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
                    };
                    border.Child = textBlock;

                    children.Add(border);
                    Grid.SetRow(border, 0);
                    Grid.SetColumn(border, index++);
                }

                // 데이터 넣기
                i = 0;
                foreach (GroupedRows grs in groupedRows.Take(MaximumRowNumber))
                {
                    j = 0;
                    foreach (ColumnViewModel columnViewModel in selectedColumnViewModels)
                    {
                        Bin bin = grs.Keys[columnViewModel] as Bin;

                        String content = bin.ToString();
                        Border border = GetNewBorder(content);

                        (border.Child as TextBlock).Foreground = ViewStatus.Category10FirstSolidColorBrush;

                        children.Add(border);
                        Grid.SetRow(border, 1 + i);
                        Grid.SetColumn(border, j++);
                    }
                    {
                        Border border = GetNewBorder(grs.Rows.Count);

                        children.Add(border);
                        Grid.SetRow(border, 1 + i);
                        Grid.SetColumn(border, j++);
                    }
                    i++;
                }
            }
            /*else if (viewStatus.IsC)
            {
                Int32 rowN = groupedRows.Count + 1, // 테이블 전체 로우의 개수
                      columnN = 2;

                Int32 i, index, j;

                // 개수만큼 추가 컬럼 및 로우 정의 추가. 이중선 말고는 별 특별한 점 없음.
                for (i = 0; i < rowN; ++i)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Height = GridLength.Auto;
                    pivotTableView.PivotTable.RowDefinitions.Add(rowDefinition);
                }

                for (i = 0; i < columnN; ++i)
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    pivotTableView.PivotTable.ColumnDefinitions.Add(columnDefinition);
                }

                // 가로 이름 넣기 (인덱스 열 포함)

                index = 0;
                foreach (ColumnViewModel columnViewModel in selectedColumnViewModels)
                {
                    Border border = new Border()
                    {
                        Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = columnViewModel.FormatHeaderName(viewStatus),
                        Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
                    };
                    border.Child = textBlock;

                    children.Add(border);
                    Grid.SetRow(border, 0);
                    Grid.SetColumn(border, index++);
                }

                {
                    Border border = new Border()
                    {
                        Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = $"Count({selectedColumnViewModels.First().Name})",
                        Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
                    };
                    border.Child = textBlock;

                    children.Add(border);
                    Grid.SetRow(border, 0);
                    Grid.SetColumn(border, index++);
                }

                // 데이터 넣기
                i = 0;
                foreach (RowViewModel rowViewModel in groupedRowViewModels.Take(MaximumRowNumber))
                {
                    j = 0;
                    foreach (ColumnViewModel columnViewModel in selectedColumnViewModels)
                    {
                        Border border = GetNewBorder(rowViewModel.Cells[columnViewModel.Index].Content);

                        children.Add(border);
                        (border.Child as TextBlock).Foreground = new SolidColorBrush(rowViewModel.Color);
                        Grid.SetRow(border, 1 + i);
                        Grid.SetColumn(border, j++);
                    }
                    {
                        Border border = GetNewBorder(groupedRows[i].Rows.Count);

                        children.Add(border);
                        Grid.SetRow(border, 1 + i);
                        Grid.SetColumn(border, j++);
                    }
                    i++;
                }
            }*/
            else
            {
                // TODO Cx 의 경우 Count 컬럼 추가하고 값 넣어야함

                Int32 rowN = groupedRows.Count + 1, // 테이블 전체 로우의 개수
                      columnN = selectedColumnViewModels.Count/* + 1 + 1*/; // 테이블 전체 컬럼의 개수
                Boolean isOnlyCn = viewStatus.IsOnlyCn;

                Int32 i, index, j;
                if (isOnlyCn) columnN ++;

                // 개수만큼 추가 컬럼 및 로우 정의 추가. 이중선 말고는 별 특별한 점 없음.
                for (i = 0; i < rowN; ++i)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Height = GridLength.Auto;
                    pivotTableView.PivotTable.RowDefinitions.Add(rowDefinition);
                }

                for (i = 0; i < columnN; ++i)
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    pivotTableView.PivotTable.ColumnDefinitions.Add(columnDefinition);
                }

                // 가로 이름 넣기 (인덱스 열 포함)

                index = 0;
                foreach (ColumnViewModel columnViewModel in selectedColumnViewModels)
                {
                    Border border = new Border()
                    {
                        Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = columnViewModel.FormatHeaderName(viewStatus),
                        Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
                    };
                    border.Child = textBlock;

                    children.Add(border);
                    Grid.SetRow(border, 0);
                    Grid.SetColumn(border, index++);
                }

                if(isOnlyCn)
                {
                    Border border = new Border()
                    {
                        Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = "Count",
                        Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
                    };
                    border.Child = textBlock;

                    children.Add(border);
                    Grid.SetRow(border, 0);
                    Grid.SetColumn(border, columnN - 1);
                }

                // 데이터 넣기
                i = 0;
                ColumnViewModel lastCategorical = viewStatus.LastCategorical;

                foreach (RowViewModel rowViewModel in groupedRowViewModels.Take(MaximumRowNumber))
                {
                    j = 0;
                    foreach (ColumnViewModel columnViewModel in selectedColumnViewModels)
                    {
                        Border border = GetNewBorder(rowViewModel.Cells[columnViewModel.Index].Content);

                        children.Add(border);

                        if(columnViewModel == lastCategorical)
                            (border.Child as TextBlock).Foreground = new SolidColorBrush(rowViewModel.Color);

                        Grid.SetRow(border, 1 + i);
                        Grid.SetColumn(border, j++);
                    }

                    if(isOnlyCn)
                    {
                        Border border = GetNewBorder(rowViewModel.Rows.Count().ToString());

                        children.Add(border);

                        Grid.SetRow(border, 1 + i);
                        Grid.SetColumn(border, columnN - 1);
                    }
                    i++;
                }
            }
        }

        /*
        public void Preview(
            List<ColumnViewModel> verticals, // 세로축에 올것 모두 카테고리컬
            ColumnViewModel horizontal, // 가로축에 올것 카테고리컬
            List<ColumnViewModel> columns, // 가로축에 올 뉴메리컬들
            List<GroupedRows> groupedRows
            )
        {
            return;
            // 모두 지우기

            UIElementCollection children = pivotTableView.PivotTable.Children;
            children.Clear();
            pivotTableView.PivotTable.ColumnDefinitions.Clear();
            pivotTableView.PivotTable.RowDefinitions.Clear();

            /*
             * 세 가지 가능성이 있음
             * 1. horizontal != null 이고 columns.Count > 1 -> 이 경우 헤더 로우 세개를 써야함 (horizontal별 여러 컬럼의 통계값을 보여주면 됨)
             * 2. hozirontal != null 이고 columns.Count == 1 -> 이 경우 헤더 로우 두개를 써야함 (horizontal별 한 컬럼의 통계값을 보여주면 됨)
             * 3. hozirontal != null 이고 columns.Count == 0 -> 이 경우 헤더 로우 두개를 써야함 (horizontal의 카운트를 보여주면 됨)
             * 4. horizontal == null 이고 columns.Count >= 1 -> 이 경우 헤더 로우 하나를 써야함 (한 컬럼의 통계값을 보여주면 됨)
             * 5. horizontal == null 이고 columns.Count == 0 은 있을 수 없음
             *

            Int32 rowN, // 테이블 전체 로우의 개수
                  columnN; // 테이블 전체 컬럼의 개수

            Int32 i, index, j;
            Int32 headerRowN = 0, // 헤더 로우의 개수
                  headerColumnN = 0 // 헤더 컬럼의 개수
                  ;
                    
            
            if (horizontal != null)
            {
                if (columns.Count > 1)
                {
                    headerRowN = 3;
                    headerColumnN = horizontal.Categories.Count * columns.Count;
                }
                else
                {
                    headerRowN = 2;
                    headerColumnN = horizontal.Categories.Count;
                }
            }
            else
            {
                if (columns.Count >= 1)
                {
                    headerRowN = 1;
                    headerColumnN = columns.Count;
                }
                else {
                    throw new Exception("horizontal == null and columns.Count == 0");
                }
            }
            
            columnN = verticals.Count + // 앞쪽 세로축 개수
                      headerColumnN + // 헤더 컬럼 개수
                      1; // 이중선 하나

            Int32 permutate = 1;
            foreach (ColumnViewModel columnViewModel in verticals)
            {
                permutate *= columnViewModel.Categories.Count;
            }

            rowN = permutate + headerRowN;

            Int32 maximumRowNumber = MaximumCellNumber / columnN;

            // 개수만큼 추가 컬럼 및 로우 정의 추가. 이중선 말고는 별 특별한 점 없음.

            for (i = 0; i < rowN; ++i)
            {
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = GridLength.Auto;
                pivotTableView.PivotTable.RowDefinitions.Add(rowDefinition);
            }

            for (i = 0; i < columnN; ++i)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                if (i == verticals.Count) // 이중선
                { 
                    columnDefinition.Width = new GridLength(3);
                }
                pivotTableView.PivotTable.ColumnDefinitions.Add(columnDefinition);
            }

            // verticals(세로로 배열 될 컬럼들)의 이름을 넣어줌 특이한 점은 헤더가 2행이 될 수도 있으므로 병합해줘야 한다는 것.
            index = 0;
            foreach (ColumnViewModel columnViewModel in verticals)
            {
                Border border = new Border()
                {
                    Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
                };
                TextBlock textBlock = new TextBlock()
                {
                    Text = columnViewModel.Column.Name,
                    Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
                };
                border.Child = textBlock;

                children.Add(border);
                Grid.SetRow(border, 0);
                Grid.SetColumn(border, index++);
                Grid.SetRowSpan(border, headerRowN);
            }

            // 컬럼 헤더 이 경우에는 2개인지 하나인지에 따라 달라짐

            index = 0;

            if (horizontal != null)
            {
                if (columns.Count > 1)
                {
                    {
                        Border border = GetNewHeaderBorder();
                        TextBlock textBlock = GetNewHeaderTextBlock(String.Format("{0}", horizontal.Column.Name));
                        border.Child = textBlock;

                        children.Add(border);
                        Grid.SetRow(border, 0);
                        Grid.SetColumn(border, verticals.Count + 1);
                        Grid.SetColumnSpan(border, horizontal.Categories.Count * columns.Count);
                    }

                    // 여기는 컬럼헤더가 세개여야함
                    index = 0;
                    // 첫번째 카테고리컬
                    foreach (Category category in horizontal.Categories)
                    {
                        Border border = GetNewHeaderBorder();
                        TextBlock textBlock = GetNewHeaderTextBlock(category.ToString());
                        border.Child = textBlock;

                        children.Add(border);
                        Grid.SetRow(border, 1);
                        Grid.SetColumn(border, verticals.Count + 1 + index);
                        Grid.SetColumnSpan(border, columns.Count);
                        index += columns.Count;
                    }

                    // 두번째 뉴메리컬
                    index = 0;
                    foreach (Category category in horizontal.Categories)
                    {
                        foreach (ColumnViewModel column in columns)
                        {
                            Border border = GetNewHeaderBorder();
                            TextBlock textBlock = GetNewHeaderTextBlock(String.Format("AVG({0})", column.Column.Name));
                            border.Child = textBlock;

                            children.Add(border);
                            Grid.SetRow(border, 2); //2에 추가하는것에 주의
                            Grid.SetColumn(border, verticals.Count + 1 + index++);
                        }
                    }
                }
                else if (columns.Count == 1)
                {
                    {
                        Border border = GetNewHeaderBorder();
                        TextBlock textBlock = GetNewHeaderTextBlock(String.Format("AVG({0})", columns[0].Column.Name));
                        border.Child = textBlock;

                        children.Add(border);
                        Grid.SetRow(border, 0);
                        Grid.SetColumn(border, verticals.Count + 1);
                        Grid.SetColumnSpan(border, horizontal.Categories.Count);
                    }

                    // 두개
                    foreach (Category category in horizontal.Categories)
                    {
                        Border border = GetNewHeaderBorder();
                        TextBlock textBlock = GetNewHeaderTextBlock(category.ToString());
                        textBlock.Inlines.Clear();
                        textBlock.Inlines.Add(new Run(){Text = horizontal.Column.Name});
                        textBlock.Inlines.Add(new LineBreak());
                        textBlock.Inlines.Add(new Run(){Text = category.ToString()});
                        border.Child = textBlock;

                        children.Add(border);
                        Grid.SetRow(border, 1);
                        Grid.SetColumn(border, verticals.Count + 1 + index++);
                    }
                }
                else
                {
                    // 헤더
                    {
                        Border border = GetNewHeaderBorder();
                        TextBlock textBlock = GetNewHeaderTextBlock(String.Format("Frequency of {0}", horizontal.Column.Name));
                        border.Child = textBlock;

                        children.Add(border);
                        Grid.SetRow(border, 0);
                        Grid.SetColumn(border, verticals.Count + 1);
                        Grid.SetColumnSpan(border, horizontal.Categories.Count);
                    }

                    // 두개
                    foreach (Category category in horizontal.Categories)
                    {
                        Border border = GetNewHeaderBorder();
                        TextBlock textBlock = GetNewHeaderTextBlock(category.ToString());
                        border.Child = textBlock;

                        children.Add(border);
                        Grid.SetRow(border, 1);
                        Grid.SetColumn(border, verticals.Count + 1 + index++);
                    }
                }
            }
            else
            {
                if (columns.Count >= 1)
                {
                    foreach (ColumnViewModel column in columns)
                    {
                        Border border = GetNewHeaderBorder();
                        TextBlock textBlock = GetNewHeaderTextBlock(String.Format("AVG({0})", column.Column.Name));
                        border.Child = textBlock;

                        children.Add(border);
                        Grid.SetRow(border, 0);
                        Grid.SetColumn(border, verticals.Count + 1 + index++);
                    }
                }
                else
                {
                    throw new Exception("horizontal == null and columns.Count == 0");
                }
            }

           
            // 로우 헤더
            index = 0;
            Int32 repeat = 1, span;
            foreach (ColumnViewModel columnViewModel in verticals)
            {
                Int32 count = columnViewModel.Categories.Count;
                span = (rowN - headerRowN) / repeat / count;

                for (i = 0; i < repeat; ++i)
                {
                    j = 0;
                    foreach (Category category in columnViewModel.Categories)
                    {
                        if(headerRowN + span * (i * count + j) >= maximumRowNumber)
                        {
                            break;
                        }

                        Border border = new Border()
                        {
                            Style = pivotTableView.Resources["RowHeaderBorderStyle"] as Style,
                        };
                        TextBlock textBlock = new TextBlock()
                        {
                            Text = category.ToString(),
                            Style = pivotTableView.Resources["ValueTextStyle"] as Style
                        };
                        border.Child = textBlock;

                        children.Add(border);
                        Grid.SetRow(border, headerRowN + span * (i * count + j));
                        Grid.SetColumn(border, index);

                        Grid.SetRowSpan(border, headerRowN + span * (i * count + j) + span >= maximumRowNumber ? maximumRowNumber - headerRowN - span * (i * count + j) : span);

                        ++j;
                    }
                }
                repeat *= count;
                index++;
            }

            // 이중선
            for (i = 0; i < rowN; ++i)
            {
                if (i >= maximumRowNumber) break;
                Border border = new Border()
                {
                    Style = pivotTableView.Resources["SeparatingBorderStyle"] as Style
                };
                children.Add(border);
                Grid.SetRow(border, i);
                Grid.SetColumn(border, verticals.Count);
            }

            List<Border> zeroValueBorders = new List<Border>();
            Dictionary<String, Border> borders = new Dictionary<String, Border>();
            for (i = 0; i < rowN - headerRowN; ++i)
            {
                if (headerRowN + i >= maximumRowNumber) break;
                for (j = 0; j < headerColumnN; ++j)
                {
                    Border border = GetNewBorder(columns.Count > 0 ? "" : "0");
                    zeroValueBorders.Add(border);
                    children.Add(border);
                    Grid.SetColumn(border, verticals.Count + 1 + j);
                    Grid.SetRow(border, headerRowN + i);
                    borders[String.Format("{0},{1}", i, j)] = border;
                }
            }

            List<Border> maxValueBorders = new List<Border>();
            Double maxValue = Double.MinValue;

            List<Border> minValueBorders = new List<Border>();
            Double minValue = Double.MaxValue;

            foreach(GroupedRows groupedRow in groupedRows) {
                // 먼저 이 그룹에 대한 인덱스를 찾아야함
                // rowIndex는 컬럼헤더 두행 혹은 한 행을 제외하고 계산되는 것임
                Int32 rowIndex = 0;
                foreach(ColumnViewModel vertical in verticals)
                {
                    rowIndex *= vertical.Categories.Count;
                    rowIndex += vertical.Categories.IndexOf(groupedRow.Keys[vertical] as Category);
                }

                if (rowIndex + headerRowN >= maximumRowNumber)
                    continue;

                // 이제 행을 채운다.
                if (horizontal != null)
                {
                    if (columns.Count > 1)
                    {
                        index = 0;
                        Int32 columnIndex = horizontal.Categories.IndexOf(groupedRow.Keys[horizontal] as Category) * columns.Count;

                        foreach (ColumnViewModel column in columns)
                        {
                            Double value = column.AggregativeFunction.Aggregate(groupedRow.Rows.Select(r => (Double)r.Cells[column.Index].Content)); 
                            Border border = borders[String.Format("{0},{1}", rowIndex, columnIndex + index)];
                            (border.Child as TextBlock).Text = Util.Formatter.FormatAuto3(value);
                            zeroValueBorders.Remove(border);

                            if (value != 0)
                            {
                                if (value < minValue)
                                {
                                    minValue = value;
                                    minValueBorders.Clear();
                                }
                                if (value == minValue)
                                {
                                    minValueBorders.Add(border);
                                }

                                if (value > maxValue)
                                {
                                    maxValue = value;
                                    maxValueBorders.Clear();
                                }
                                if (value == maxValue)
                                {
                                    maxValueBorders.Add(border);
                                }
                            }
                            index++;
                        }
                    }
                    else if(columns.Count == 1){
                        Int32 columnIndex = horizontal.Categories.IndexOf(groupedRow.Keys[horizontal] as Category);

                        Double value = columns[0].AggregativeFunction.Aggregate(groupedRow.Rows.Select(r => (Double)r.Cells[columns[0].Index].Content));
                        Border border = borders[String.Format("{0},{1}", rowIndex, columnIndex)];
                        (border.Child as TextBlock).Text = Util.Formatter.FormatAuto3(value);
                        zeroValueBorders.Remove(border);

                        if (value != 0)
                        {
                            if (value < minValue)
                            {
                                minValue = value;
                                minValueBorders.Clear();
                            }
                            if (value == minValue)
                            {
                                minValueBorders.Add(border);
                            }

                            if (value > maxValue)
                            {
                                maxValue = value;
                                maxValueBorders.Clear();
                            }
                            if (value == maxValue)
                            {
                                maxValueBorders.Add(border);
                            }
                        }
                    }
                    else
                    {
                        // count로 채움
                        Int32 columnIndex = horizontal.Categories.IndexOf(groupedRow.Keys[horizontal] as Category);
                        
                        Int32 value = groupedRow.Rows.Count;
                        Border border = borders[String.Format("{0},{1}", rowIndex, columnIndex)];
                        (border.Child as TextBlock).Text = value.ToString();
                        zeroValueBorders.Remove(border);

                        if (value == 0)
                        {
                        }
                        else
                        {
                            if(value > maxValue){
                                maxValue = value;
                                maxValueBorders.Clear();
                            }
                            if (value == maxValue)
                            {
                                maxValueBorders.Add(border);
                            }
                        }
                    }
                }
                else
                {
                    if (columns.Count >= 1)
                    {
                        index = 0;
                        foreach (ColumnViewModel column in columns)
                        {
                            Double value = column.AggregativeFunction.Aggregate(groupedRow.Rows.Select(r => (Double)r.Cells[column.Index].Content));
                            Border border = borders[String.Format("{0},{1}", rowIndex, index)];
                            (border.Child as TextBlock).Text = Util.Formatter.FormatAuto3(value);
                            zeroValueBorders.Remove(border);

                            if (value != 0)
                            {
                                if (value < minValue)
                                {
                                    minValue = value;
                                    minValueBorders.Clear();
                                }
                                if (value == minValue)
                                {
                                    minValueBorders.Add(border);
                                }

                                if (value > maxValue)
                                {
                                    maxValue = value;
                                    maxValueBorders.Clear();
                                }
                                if (value == maxValue)
                                {
                                    maxValueBorders.Add(border);
                                }
                            }
                            index++;
                        }
                    }
                    else {
                        throw new Exception("horizontal == null and columns.Count == 0");
                    }
                }
            }

            foreach (Border border in minValueBorders)
            {
                border.Style = pivotTableView.Resources["MinBorderStyle"] as Style;
                (border.Child as TextBlock).Style = pivotTableView.Resources["MinValueTextStyle"] as Style;
            }

            foreach (Border border in zeroValueBorders)
            {
                (border.Child as TextBlock).Style = pivotTableView.Resources["ZeroValueTextStyle"] as Style;
            }

            foreach (Border border in maxValueBorders)
            {
                border.Style = pivotTableView.Resources["MaxBorderStyle"] as Style;
                (border.Child as TextBlock).Style = pivotTableView.Resources["MaxValueTextStyle"] as Style;
            }
        }*/

        private Border GetNewBorder(Object value)
        {
            Border border = new Border()
            {
                Style = pivotTableView.Resources["BorderStyle"] as Style
            };

            TextBlock textBlock = new TextBlock()
            {
                Text = value.ToString(),
                Style = pivotTableView.Resources["ValueTextStyle"] as Style
            };
            border.Child = textBlock;
            return border;
        }
    }
}
