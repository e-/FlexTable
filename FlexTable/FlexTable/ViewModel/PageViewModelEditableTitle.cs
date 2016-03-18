using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexTable.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Text.RegularExpressions;
using FlexTable.View;
using Windows.Devices.Input;
using FlexTable.Util;

namespace FlexTable.ViewModel
{
    public partial class PageViewModel : NotifyViewModel
    {
        void AddText(EditableTitle stackPanel, String text)
        {
            TextBlock textBlock = new TextBlock()
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 30,
            };
            Util.HtmlToTextBlockFormatter.Format(text.Replace(' ', (char)160), textBlock);

            stackPanel.Add(textBlock);
        }


        void AddComboBox(EditableTitle stackPanel, String selected, IEnumerable<String> candidates, SelectionChangedEventHandler selectionChanged, String columnName)
        {
            if (!IsSelected)
            {
                AddText(stackPanel, $"<b>{selected}</b>");
            }
            else
            {
                ComboBox comboBox = new ComboBox()
                {
                    Style = App.Current.Resources[columnName.Length > 0 ? "SeamlessComboBoxStyle" : "SeamlessComboBoxStyle2"] as Style,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = columnName,
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
                stackPanel.Add(comboBox);

                /*comboBox.PointerEntered += (o, e) =>
                {
                    if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
                    {
                        e.Handled = true;
                        return;
                    }
                };
                */
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

                Logger.Log($"column changed,{currentColumnViewModel.Column.Name},{selectedColumnViewModel.Column.Name}");

                // 1. 컬럼의 상태 변경 
                if (selectedColumnViewModel.IsSelected)
                {
                    Int32 index1 = ViewStatus.SelectedColumnViewModels.FindIndex(cvm => cvm == currentColumnViewModel);
                    Int32 index2 = ViewStatus.SelectedColumnViewModels.FindIndex(cvm => cvm == selectedColumnViewModel);
                    ViewStatus.SelectedColumnViewModels[index1] = selectedColumnViewModel;
                    ViewStatus.SelectedColumnViewModels[index2] = currentColumnViewModel;
                    ViewStatus.Refresh();
                }
                else
                {
                    currentColumnViewModel.IsSelected = false;
                    selectedColumnViewModel.IsSelected = true;

                    Int32 index = ViewStatus.SelectedColumnViewModels.FindIndex(cvm => cvm == currentColumnViewModel);
                    ViewStatus.SelectedColumnViewModels[index] = selectedColumnViewModel;
                    ViewStatus.Refresh();
                }

                if (selectedColumnViewModel.Type == ColumnType.Numerical && defaultAggregativeFunction != null)
                {
                    selectedColumnViewModel.AggregativeFunction = defaultAggregativeFunction;
                }

            mainPageViewModel.ReflectAll(ReflectReason.ColumnChanged); // 2.ColumnViewModelChanged);
            });
        }

        SelectionChangedEventHandler CreateAggregationChangedHandler(ColumnViewModel columnViewModel)
        {
            return new SelectionChangedEventHandler((sender, args) =>
            {
                ComboBox comboBox = sender as ComboBox;
                String selectedName = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
                AggregativeFunction.BaseAggregation aggregativeFunction = AggregativeFunction.FromName(selectedName);

                Logger.Log($"aggregation changed,{columnViewModel.Column.Name},{columnViewModel.AggregativeFunction.Name},{aggregativeFunction.Name}");

                columnViewModel.AggregativeFunction = aggregativeFunction;

                mainPageViewModel.ReflectAll(ReflectReason.ColumnChanged); //.ColumnViewModelChanged);
            });
        }

        void AddAggregatedColumnComboBox(EditableTitle title, ColumnViewModel numerical, Boolean withSameUnit)
        {
            String format = Const.Loader.GetString("AggregatedColumnName");
            foreach(String f in format.Split('|'))
            {
                if(f == "{0}")
                {
                    if(IsSelected)
                    {
                        AddComboBox(
                           title,
                           numerical.AggregativeFunction.Name,
                           AggregativeFunction.Names,
                           CreateAggregationChangedHandler(numerical),
                           String.Empty
                       );
                    }
                    else
                    {
                        AddText(title, $"<b>{numerical.AggregativeFunction.Name}</b>");
                    }
                }
                else if(f == "{1}")
                {
                    if (IsSelected)
                    {
                        AddComboBox(
                           title,
                           numerical.Name,
                           mainPageViewModel.SheetViewModel.ColumnViewModels.Where(
                               cvm => cvm.Type == ColumnType.Numerical && (withSameUnit ? cvm.Unit == numerical.Unit : true)
                           ).Select(cvm => cvm.Name),
                           CreateColumnChangedHandler(numerical),
                           numerical.Column.Name
                       );
                    }
                    else
                    {
                        AddText(title, $"<b>{numerical.Name}</b>");
                    }
                }
                else
                {
                    AddText(title, f);
                }
            }
        }

        void AddColumnComboBox(EditableTitle title, ColumnViewModel columnViewModel)
        {
            if (IsSelected)
            {
                AddComboBox(
                    title,
                    columnViewModel.Name,
                    mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == columnViewModel.Type).Select(cvm => cvm.Name),
                    CreateColumnChangedHandler(columnViewModel),
                    columnViewModel.Column.Name
                );
            }
            else
            {
                AddText(title, $"<b>{columnViewModel.Name}</b>");
            }
        }

        void AddEditableTitle(EditableTitle title, String format, ColumnViewModel column0)
        {
            AddEditableTitle(title, format, new List<ColumnViewModel>() { column0 });
        }

        void AddEditableTitle(EditableTitle title, String format, ColumnViewModel column0, ColumnViewModel column1)
        {
            AddEditableTitle(title, format, new List<ColumnViewModel>() { column0, column1 });
        }

        void AddEditableTitle(EditableTitle title, String format, ColumnViewModel column0, ColumnViewModel column1, ColumnViewModel column2)
        {
            AddEditableTitle(title, format, new List<ColumnViewModel>() { column0, column1, column2 });
        }

        void AddEditableTitle(EditableTitle title, String format, List<ColumnViewModel> columnViewModels)
        {
            foreach(String f in format.Split('|'))
            {
                Match match = Regex.Match(f, @"{([^d]+):([c|a|u])}");

                if(match.Success)
                {
                    String num = match.Groups[1].Value;
                    String type = match.Groups[2].Value;
                    ColumnViewModel columnViewModel = columnViewModels[Int32.Parse(num)];

                    if(type == "a") // aggregated
                    {
                        AddAggregatedColumnComboBox(title, columnViewModel, false);
                    }
                    else if(type == "c")
                    {
                        AddColumnComboBox(title, columnViewModel);
                    }
                    else if(type == "u")
                    {
                        AddAggregatedColumnComboBox(title, columnViewModel, true);
                    }
                }
                else
                {
                    AddText(title, f);
                }
            }
        }

        #region Editable Title Generator

        /*
        void DrawEditableTitleN(StackPanel title, ColumnViewModel numerical, String prefix, String suffix)
        {
            if(prefix.Length > 0)
                AddText(title, prefix);
            AddComboBox(
                title,
                numerical.Name,
                mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                CreateColumnChangedHandler(numerical)
            );
            if (suffix.Length > 0)
                AddText(title, suffix);
        }
        void DrawEditableTitleCN(StackPanel title, ColumnViewModel categorical, ColumnViewModel numerical)
        {
            AddEditableTitle(title, Const.Loader.GetString("ChartTitleCN"), categorical, numerical);
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
        */

        void DrawEditableTitleCxNx(EditableTitle title, String prefix, List<ColumnViewModel> variables, String mid, List<ColumnViewModel> pivots)
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
                            CreateColumnChangedHandler(variable),
                            variable.Column.Name
                            );
                }
                else
                {
                    AddComboBox(
                        title,
                        variable.AggregativeFunction.Name,
                        AggregativeFunction.Names,
                        CreateAggregationChangedHandler(variable),
                        String.Empty
                    );
                    AddText(title, "(");
                    AddComboBox(title,
                        variable.Name,
                        mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                        CreateColumnChangedHandler(variable),
                        variable.Column.Name
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
                        CreateColumnChangedHandler(variable),
                        variable.Column.Name
                        );
                }
                else
                {
                    AddComboBox(
                        title,
                        variable.AggregativeFunction.Name,
                        AggregativeFunction.Names,
                        CreateAggregationChangedHandler(variable),
                        string.Empty
                    );
                    AddText(title, "(");
                    AddComboBox(title,
                        variable.Name,
                        mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Select(cvm => cvm.Name),
                        CreateColumnChangedHandler(variable),
                        variable.Column.Name
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
            pageView.PivotTableTitle.Clear();
            if (ViewStatus.IsC)
            {
                AddEditableTitle(pageView.PivotTableTitle, Const.Loader.GetString("FrequencyTitle"), ViewStatus.FirstCategorical);
            }
            else if (ViewStatus.IsN)
            {
                AddEditableTitle(pageView.PivotTableTitle, Const.Loader.GetString("DistributionTitle"), ViewStatus.FirstNumerical);
            }
            else if (ViewStatus.IsCC)
            {
                AddEditableTitle(pageView.PivotTableTitle, Const.Loader.GetString("ChartTitleCC"), ViewStatus.FirstCategorical, ViewStatus.SecondCategorical);
            }
            else if (ViewStatus.IsCN)
            {
                AddEditableTitle(pageView.PivotTableTitle, Const.Loader.GetString("ChartTitleCN"), ViewStatus.FirstCategorical, ViewStatus.FirstNumerical);
            }
            else if (ViewStatus.IsNN)
            {
                // no pivot table
            }
            else if (ViewStatus.IsCCC)
            {
                AddEditableTitle(pageView.PivotTableTitle, Const.Loader.GetString("ChartTitleCCC"), ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, ViewStatus.ThirdCategorical);
            }
            else if (ViewStatus.IsCCN)
            {
                AddEditableTitle(pageView.PivotTableTitle, Const.Loader.GetString("ChartTitleCCN"), ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, ViewStatus.FirstNumerical);
            }
            else if (ViewStatus.IsCNN)
            {
                AddEditableTitle(pageView.PivotTableTitle, Const.Loader.GetString("PivotTableTitleCNN"), ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, ViewStatus.SecondNumerical);
            }
            else if (ViewStatus.IsNNN)
            {
                // no pivot table
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
