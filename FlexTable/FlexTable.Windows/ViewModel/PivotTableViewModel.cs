using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.View;
using Windows.UI.Xaml.Controls;
using FlexTable.Model;
using Windows.UI.Xaml;

namespace FlexTable.ViewModel
{
    public class PivotTableViewModel
    {
        private MainPageViewModel mainPageViewModel;

        private PivotTableView pivotTableView;
        private Int32 maxValue = 0;

        public PivotTableViewModel(MainPageViewModel mainPageViewModel, PivotTableView pivotTableView)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.pivotTableView = pivotTableView;
        }

        public void Preview(List<ColumnViewModel> groupedColumnViewModels, ColumnViewModel previewingColumnViewModel)
        {
            UIElementCollection children = pivotTableView.PivotTable.Children;
            children.Clear();
            pivotTableView.PivotTable.ColumnDefinitions.Clear();
            pivotTableView.PivotTable.RowDefinitions.Clear();

            Int32 rowN = 1, columnN;
            Int32 i, index;

            columnN = groupedColumnViewModels.Count + previewingColumnViewModel.Categories.Count + 1;

            foreach (ColumnViewModel columnViewModel in groupedColumnViewModels)
            {
                rowN *= columnViewModel.Categories.Count;
            }

            rowN++; // title

            for (i = 0; i < rowN; ++i)
            {
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = GridLength.Auto;
                pivotTableView.PivotTable.RowDefinitions.Add(rowDefinition);
            }

            for (i = 0; i < columnN; ++i)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                if (i == groupedColumnViewModels.Count) // 이중선
                { 
                    columnDefinition.Width = new GridLength(3);
                }
                pivotTableView.PivotTable.ColumnDefinitions.Add(columnDefinition);
            }

            // 그룹 된 컬럼 헤더들
            index = 0;
            foreach (ColumnViewModel columnViewModel in groupedColumnViewModels)
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
            }

            // 컬럼 헤더
            index = 0;
            foreach (Category category in previewingColumnViewModel.Categories)
            {
                Border border = new Border()
                {
                    Style = pivotTableView.Resources["ColumnHeaderBorderStyle"] as Style
                };
                TextBlock textBlock = new TextBlock()
                {
                    Text = category.ToString(),
                    Style = pivotTableView.Resources["ColumnHeaderValueTextStyle"] as Style
                };
                border.Child = textBlock;

                children.Add(border);
                Grid.SetRow(border, 0);
                Grid.SetColumn(border, groupedColumnViewModels.Count + 1 + index++);
            }
           
            // 로우 헤더
            index = 0;
            Int32 repeat = 1, span;
            foreach (ColumnViewModel columnViewModel in groupedColumnViewModels)
            {
                Int32 count = columnViewModel.Categories.Count;
                span = (rowN - 1) / repeat / count;

                for (i = 0; i < repeat; ++i)
                {
                    Int32 j = 0;
                    foreach (Category category in columnViewModel.Categories)
                    {
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
                        Grid.SetRow(border, 1 + span * (i * count + j));
                        Grid.SetColumn(border, index);
                        Grid.SetRowSpan(border, span);

                        ++j;
                    }
                }
                repeat *= count;
                index++;
            }

            // 이중선
            for (i = 0; i < rowN; ++i)
            {
                Border border = new Border()
                {
                    Style = pivotTableView.Resources["SeparatingBorderStyle"] as Style
                };
                children.Add(border);
                Grid.SetRow(border, i);
                Grid.SetColumn(border, groupedColumnViewModels.Count);
            }

            // 값
            // 새로 분류해야함. 왜냐하면 기존에 로우가 하나도 없는 키들의 조합의 경우 아예 없기 때문에 매칭시 힘들 수 있음.

            Dictionary<Category, Object> classified = ConstructKeyDictionary(groupedColumnViewModels, 0);

            maxValue = 0;
            foreach(Row row in mainPageViewModel.Sheet.Rows.ToList())
            {
                Dictionary<Category, Object> dict = classified;

                //기존 그룹된 컬럼들
                foreach (ColumnViewModel grouped in groupedColumnViewModels)
                {
                    Category category = row.Cells[grouped.Index].Content as Category;
                    dict = dict[category] as Dictionary<Category, Object>;
                }

                //프리뷰 하는 컬럼
                {
                    Category category = row.Cells[previewingColumnViewModel.Index].Content as Category;
                    if (!dict.ContainsKey(category))
                    {
                        dict[category] = 0;
                    }
                    dict[category] = (Int32)dict[category] + 1;
                    if (maxValue < (Int32)dict[category]) maxValue = (Int32)dict[category];
                }
            }

            // 재귀적으로 채우기
            FillRecursive(groupedColumnViewModels, 0, previewingColumnViewModel, classified, children, 1, rowN - 1);
        }

        private Dictionary<Category, Object> ConstructKeyDictionary(List<ColumnViewModel> groupedColumnViewModels, Int32 index)
        {
            Dictionary<Category, Object> dict = new Dictionary<Category, object>();
            foreach (Category category in groupedColumnViewModels[index].Categories)
            {
                dict[category] = (index + 1) < groupedColumnViewModels.Count ? ConstructKeyDictionary(groupedColumnViewModels, index + 1) : new Dictionary<Category, Object>();
            }

            return dict;
        }

        private void FillRecursive(List<ColumnViewModel> groupedColumnViewModels, Int32 groupIndex, ColumnViewModel previewingColumnViewModel, 
            Dictionary<Category, Object> classified, UIElementCollection children, Int32 offset, Int32 cardinality)
        {
            if (groupedColumnViewModels.Count == groupIndex) // finished
            {
                Int32 index = 0;
                foreach (Category category in previewingColumnViewModel.Categories)
                {
                    Int32 value;
                    if (classified.ContainsKey(category)) value = (Int32)classified[category];
                    else value = 0;

                    Border border = new Border()
                    {
                        Style = pivotTableView.Resources[
                            value == 0 ? "BorderStyle" : (value == maxValue ? "MaxBorderStyle" : "BorderStyle")
                        ] as Style
                    };

                    TextBlock textBlock = new TextBlock()
                    {
                        Text = value.ToString(),
                        Style = pivotTableView.Resources[
                            value == 0 ? "ZeroValueTextStyle" : (value == maxValue ? "MaxValueTextStyle" : "ValueTextStyle")
                        ] as Style
                    };
                    border.Child = textBlock;

                    children.Add(border);
                    Grid.SetRow(border, offset);
                    Grid.SetColumn(border, index + groupedColumnViewModels.Count + 1);
                    index++;
                }
            }
            else
            {
                Int32 index = 0;
                Int32 count = groupedColumnViewModels[groupIndex].Categories.Count;
                foreach (Category category in groupedColumnViewModels[groupIndex].Categories)
                {
                    FillRecursive(groupedColumnViewModels, groupIndex + 1, previewingColumnViewModel, classified[category] as Dictionary<Category, Object>,
                        children, offset + cardinality / count * index, cardinality / count
                    );
                    index++;
                }
            }
        }
    }
}
