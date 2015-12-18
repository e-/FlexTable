using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlexTable.ViewModel
{
    public partial class PageViewModel : NotifyViewModel
    {
        void AddText(StackPanel stackPanel, String text)
        {
            TextBlock textBlock = new TextBlock()
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 30
            };
            Util.HtmlToTextBlockFormatter.Format(text, textBlock);

            stackPanel.Children.Add(textBlock);
        }


        void AddComboBox(StackPanel stackPanel, String selected, IEnumerable<String> candidates, SelectionChangedEventHandler selectionChanged)
        {
            AddComboBox(stackPanel, selected, candidates, selectionChanged, true);
        }

        void AddComboBox(StackPanel stackPanel, String selected, IEnumerable<String> candidates, SelectionChangedEventHandler selectionChanged, Boolean isColumnName)
        {
            if (!IsSelected)
            {
                AddText(stackPanel, $"<b>{selected}</b>");
            }
            else
            {
                ComboBox comboBox = new ComboBox()
                {
                    Style = App.Current.Resources[isColumnName ? "SeamlessComboBoxStyle" : "SeamlessComboBoxStyle2"] as Style,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 30
                };
                foreach (String candidate in candidates)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem()
                    {
                        Content = candidate,
                        IsSelected = candidate == selected,
                        Style = App.Current.Resources["SeamlessComboBoxItemStyle"] as Style,
                    };
                    comboBox.Items.Add(comboBoxItem);
                }
                stackPanel.Children.Add(comboBox);

                comboBox.DropDownOpened += (o, e) =>
                {
                    comboBox.Width = comboBox.ActualWidth;
                };

                comboBox.DropDownClosed += (o, e) =>
                {
                    comboBox.Width = Double.NaN;
                };

                comboBox.SelectionChanged += selectionChanged;
            }
        }

        SelectionChangedEventHandler CreateColumnChangedHandler(ColumnViewModel currentColumnViewModel)
        {
            if (currentColumnViewModel.Type == ColumnType.Numerical)
            {
                return CreateColumnChangedHandler(currentColumnViewModel, currentColumnViewModel.AggregativeFunction);
            }
            return CreateColumnChangedHandler(currentColumnViewModel, null);
        }

        SelectionChangedEventHandler CreateColumnChangedHandler(ColumnViewModel currentColumnViewModel, AggregativeFunction.BaseAggregation defaultAggregativeFunction)
        {
            return new SelectionChangedEventHandler((sender, args) =>
            {
                ComboBox comboBox = sender as ComboBox;
                String selectedName = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
                ColumnViewModel selectedColumnViewModel = mainPageViewModel.SheetViewModel.ColumnViewModels.First(cvm => cvm.Name == selectedName);

                // selectedColumnViewModel로 이제 바꾸면 됨. 이 과정에 대해서는 explorationViewModel의 PageViewTapped를 참고하면 좋다.

                // 1. 컬럼의 상태 변경 
                if (selectedColumnViewModel.IsSelected)
                {
                    Int32 index1 = ViewStatus.SelectedColumnViewModels.FindIndex(cvm => cvm == currentColumnViewModel);
                    Int32 index2 = ViewStatus.SelectedColumnViewModels.FindIndex(cvm => cvm == selectedColumnViewModel);
                    ViewStatus.SelectedColumnViewModels[index1] = selectedColumnViewModel;
                    ViewStatus.SelectedColumnViewModels[index2] = currentColumnViewModel;
                }
                else
                {
                    currentColumnViewModel.IsSelected = false;
                    selectedColumnViewModel.IsSelected = true;

                    Int32 index = ViewStatus.SelectedColumnViewModels.FindIndex(cvm => cvm == currentColumnViewModel);
                    ViewStatus.SelectedColumnViewModels[index] = selectedColumnViewModel;
                }

                if (selectedColumnViewModel.Type == ColumnType.Numerical && defaultAggregativeFunction != null)
                {
                    selectedColumnViewModel.AggregativeFunction = defaultAggregativeFunction;
                }

                mainPageViewModel.ReflectAll(false);
            });
        }

        SelectionChangedEventHandler CreateAggregationChangedHandler(ColumnViewModel columnViewModel)
        {
            return new SelectionChangedEventHandler((sender, args) =>
            {
                ComboBox comboBox = sender as ComboBox;
                String selectedName = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
                AggregativeFunction.BaseAggregation aggregativeFunction = AggregativeFunction.FromName(selectedName);

                columnViewModel.AggregativeFunction = aggregativeFunction;

                mainPageViewModel.ReflectAll(false);
            });
        }


        #region Editable Title Generator

        void DrawEditableTitleN(StackPanel title, ColumnViewModel numerical, String prefix)
        {
            AddText(title, prefix);
            AddComboBox(
                title,
                numerical.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical)
            );
        }

        void DrawEditableTitleCN(StackPanel title, ColumnViewModel categorical, ColumnViewModel numerical)
        {
            // TODO 선택 변경하면 에러남 이 부분 처리해야함
            // 현재 aggregation function의 상태 저장이 필요함 왜냐하면 numerical을 바꿨을때 aggrfunction은 유지해야 하기 때문
            AddComboBox(
                   title,
                   numerical.AggregativeFunction.Name,
                   AggregativeFunction.Names,
                   CreateAggregationChangedHandler(numerical),
                   false
               );
            AddText(title, "(");
            AddComboBox(
                   title,
                   numerical.Name,
                   mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                   CreateColumnChangedHandler(numerical)
               );
            AddText(title, ")\x00A0by\x00A0");
            AddComboBox(
                title,
                categorical.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(categorical)
            );
        }

        void DrawEditableTitleCCN(StackPanel title, ColumnViewModel categorical1, ColumnViewModel categorical2, ColumnViewModel numerical)
        {
            AddComboBox(
                   title,
                   numerical.AggregativeFunction.Name,
                   AggregativeFunction.Names,
                   CreateAggregationChangedHandler(numerical)
               );
            AddText(title, "(");
            AddComboBox(
                    title,
                    numerical.Name,
                    mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                    CreateColumnChangedHandler(numerical)
                );
            AddText(title, ")\x00A0by\x00A0");
            AddComboBox(
                title,
                categorical1.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(categorical1)
            );
            AddText(title, "\x00A0and\x00A0");
            AddComboBox(
               title,
               categorical2.Name,
               mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
               CreateColumnChangedHandler(categorical2)
           );
        }

        void DrawEditableTitleCNvsN(StackPanel title, ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            AddComboBox(
                title,
                numerical1.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical1)
            );

            AddText(title, "\x00A0vs.\x00A0");

            AddComboBox(
                title,
                numerical2.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical2)
            );

            AddText(title, "\x00A0colored by\x00A0");

            AddComboBox(
               title,
               categorical.Name,
               mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
               CreateColumnChangedHandler(categorical)
           );
        }

        void DrawEditableTitleNvsN(StackPanel title, ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            AddComboBox(
                title,
                numerical1.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical1)
            );

            AddText(title, "\x00A0vs.\x00A0");

            AddComboBox(
                title,
                numerical2.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical2)
            );
        }

        void DrawEditableTitleNN(StackPanel title, ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            AddComboBox(
                   title,
                   numerical1.AggregativeFunction.Name,
                   AggregativeFunction.Names,
                   CreateAggregationChangedHandler(numerical1)
               );
            AddText(title, "(");
            AddComboBox(
                title,
                numerical1.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical && cvm.Unit == numerical1.Unit).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical1)
            );

            AddText(title, ")\x00A0and\x00A0");

            AddComboBox(
                   title,
                   numerical2.AggregativeFunction.Name,
                   AggregativeFunction.Names,
                   CreateAggregationChangedHandler(numerical2)
               );
            AddText(title, "(");
            AddComboBox(
                title,
                numerical2.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical && cvm.Unit == numerical2.Unit).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical2)
            );
            AddText(title, ")");
        }
        void DrawEditableTitleCNN(StackPanel title, ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            AddComboBox(
                   title,
                   numerical1.AggregativeFunction.Name,
                   AggregativeFunction.Names,
                   CreateAggregationChangedHandler(numerical1)
               );
            AddText(title, "(");
            AddComboBox(
                title,
                numerical1.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical && cvm.Unit == numerical1.Unit).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical1)
            );

            AddText(title, ")\x00A0and\x00A0");

            AddComboBox(
                   title,
                   numerical2.AggregativeFunction.Name,
                   AggregativeFunction.Names,
                   CreateAggregationChangedHandler(numerical2)
               );
            AddText(title, "(");
            AddComboBox(
                title,
                numerical2.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical && cvm.Unit == numerical2.Unit).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical2)
            );

            AddText(title, ")\x00A0by\x00A0");

            AddComboBox(
               title,
               categorical.Name,
               mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
               CreateColumnChangedHandler(categorical)
           );
        }

        void DrawEditableTitleCxNx(StackPanel title, String prefix, List<ColumnViewModel> variables, String mid, List<ColumnViewModel> pivots)
        {
            if (prefix != null && prefix.Length > 0)
            {
                AddText(title, prefix);
            }

            Int32 index;
            index = 0;
            foreach (ColumnViewModel variable in variables)
            {
                if (variable.Type == ColumnType.Categorical)
                {
                    AddComboBox(title,
                            variable.Name,
                            mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
                            CreateColumnChangedHandler(variable)
                            );
                }
                else
                {
                    AddComboBox(
                        title,
                        variable.AggregativeFunction.Name,
                        AggregativeFunction.Names,
                        CreateAggregationChangedHandler(variable)
                    );
                    AddText(title, "(");
                    AddComboBox(title,
                        variable.Name,
                        mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                        CreateColumnChangedHandler(variable)
                        );
                    AddText(title, ")");
                }

                if (index < variables.Count - 1)
                {
                    if (variables.Count == 2)
                    {
                        AddText(title, "\x00A0and\x00A0");
                    }
                    else
                    {
                        AddText(title, ",\x00A0");
                        if (index == variables.Count - 2)
                        {
                            AddText(title, "and\x00A0");
                        }
                    }
                }
                index++;
            }

            if (mid != null && mid.Length > 0)
            {
                AddText(title, mid);
            }

            index = 0;
            foreach (ColumnViewModel variable in pivots)
            {
                if (variable.Type == ColumnType.Categorical)
                {
                    AddComboBox(title,
                        variable.Name,
                        mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
                        CreateColumnChangedHandler(variable)
                        );
                }
                else
                {
                    AddComboBox(
                        title,
                        variable.AggregativeFunction.Name,
                        AggregativeFunction.Names,
                        CreateAggregationChangedHandler(variable)
                    );
                    AddText(title, "(");
                    AddComboBox(title,
                        variable.Name,
                        mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                        CreateColumnChangedHandler(variable)
                        );
                    AddText(title, ")");
                }
                if (index < pivots.Count - 1)
                {
                    if (pivots.Count == 2)
                    {
                        AddText(title, "\x00A0and\x00A0");
                    }
                    else
                    {
                        AddText(title, ",\x00A0");
                        if (index == pivots.Count - 2)
                        {
                            AddText(title, "and\x00A0");
                        }
                    }
                }
                index++;
            }
        }
        #endregion

        private void DrawPivotTable()
        {
            pageView.PivotTableTitle.Children.Clear();
            if (ViewStatus.IsC)
            {
                DrawEditableTitleCxNx(pageView.PivotTableTitle,
                        "Frequency\x00A0of\x00A0",
                        new List<ColumnViewModel>() { ViewStatus.FirstCategorical },
                        "",
                        new List<ColumnViewModel>() { }
                        );
            }
            else if (ViewStatus.IsN)
            {
                DrawEditableTitleN(pageView.PivotTableTitle, ViewStatus.FirstNumerical, "Frequency\x00A0of\x00A0");
            }
            else if (ViewStatus.IsCC)
            {
                DrawEditableTitleCxNx(pageView.PivotTableTitle,
                        "Frequency\x00A0of\x00A0",
                        new List<ColumnViewModel>() { ViewStatus.SecondCategorical },
                        "\x00A0by\x00A0",
                        new List<ColumnViewModel>() { ViewStatus.FirstCategorical }
                        );
            }
            else if (ViewStatus.IsCN)
            {
                DrawEditableTitleCxNx(pageView.PivotTableTitle, "", ViewStatus.NumericalColumnViewModels.ToList(), "\x00A0by\x00A0", ViewStatus.CategoricalColumnViewModels.ToList());
            }
            else if (ViewStatus.IsNN)
            {
                ;
            }
            else if (ViewStatus.IsCCC)
            {
                DrawEditableTitleCxNx(pageView.PivotTableTitle,
                        "Frequency\x00A0of\x00A0",
                        new List<ColumnViewModel>() { ViewStatus.SelectedColumnViewModels.Last() },
                        "\x00A0by\x00A0",
                        ViewStatus.SelectedColumnViewModels.Where((r, i) => i < 2).ToList()
                        );
            }
            else if (ViewStatus.IsCCN)
            {
                DrawEditableTitleCxNx(pageView.PivotTableTitle,
                    "",
                    ViewStatus.NumericalColumnViewModels.ToList(),
                    "\x00A0by\x00A0",
                    ViewStatus.CategoricalColumnViewModels.ToList()
                    );
            }
            else if (ViewStatus.IsCNN)
            {
                DrawEditableTitleCNN(pageView.PivotTableTitle, ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, ViewStatus.SecondNumerical);
            }
            else if (ViewStatus.IsNNN)
            {
            }
            else if (ViewStatus.IsCnN0)
            {
                DrawEditableTitleCxNx(pageView.PivotTableTitle,
                    "Frequency of\x00A0",
                    new List<ColumnViewModel>() { ViewStatus.CategoricalColumnViewModels.Last() },
                    "\x00A0by\x00A0",
                    ViewStatus.CategoricalColumnViewModels.Where((cvm, i) => i != ViewStatus.CategoricalColumnViewModels.Count() - 1).ToList()
                    );
            }
            else if (ViewStatus.IsCnN1)
            {
                DrawEditableTitleCxNx(pageView.PivotTableTitle,
                    "",
                    ViewStatus.NumericalColumnViewModels.ToList(),
                    "\x00A0by\x00A0",
                    ViewStatus.CategoricalColumnViewModels.ToList()
                    );
            }
            else if (ViewStatus.IsCnNn)
            {
                DrawEditableTitleCxNx(pageView.PivotTableTitle,
                    "",
                    ViewStatus.NumericalColumnViewModels.ToList(),
                    "\x00A0by\x00A0",
                    ViewStatus.CategoricalColumnViewModels.ToList()
                    );
            }

            IsPivotTableVisible = true;
            pivotTableViewModel.Preview(ViewStatus);
        }

    }
}
