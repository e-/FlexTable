using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using FlexTable.Model;
using Windows.UI.Xaml;
using FlexTable.View;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Text;
using System.Diagnostics;
using FlexTable.Crayon.Chart;
using d3.ColorScheme;

namespace FlexTable.ViewModel
{
    public partial class PageViewModel : NotifyViewModel
    {
        const Int32 BarChartMaximumRecordNumber = 12;
        const Int32 GroupedBarChartMaximumRecordNumber = 48;
        const Int32 LineChartMaximumSeriesNumber = BarChartMaximumRecordNumber;
        const Int32 LineChartMaximumPointNumberInASeries = 14;
        const Int32 ScatterplotMaximumCategoryNumber = BarChartMaximumRecordNumber;

        public enum PageViewState { Empty, Previewing, Selected, Undoing }

        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

        PivotTableViewModel pivotTableViewModel;
        public PivotTableViewModel PivotTableViewModel => pivotTableViewModel;

        public PageViewState OldState { get; set; } = PageViewState.Empty;

        private PageViewState state = PageViewState.Empty;
        public PageViewState State
        {
            get { return state; }
            set
            {
                OldState = state;
                state = value;
                OnPropertyChanged(nameof(IsPreviewing));
                OnPropertyChanged(nameof(IsUndoing));
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public Boolean IsPreviewing { get { return state == PageViewState.Previewing; } }
        public Boolean IsUndoing { get { return state == PageViewState.Undoing; } }
        public Boolean IsEmpty { get { return state == PageViewState.Empty; } }
        public Boolean IsSelected { get { return state == PageViewState.Selected; } }

        private Boolean isBarChartVisible = false;
        public Boolean IsBarChartVisible { get { return isBarChartVisible; } set { isBarChartVisible = value; OnPropertyChanged(nameof(IsBarChartVisible)); } }

        private Boolean isLineChartVisible = false;
        public Boolean IsLineChartVisible { get { return isLineChartVisible; } set { isLineChartVisible = value; OnPropertyChanged(nameof(IsLineChartVisible)); } }

        private Boolean isDescriptiveStatisticsVisible = false;
        public Boolean IsDescriptiveStatisticsVisible { get { return isDescriptiveStatisticsVisible; } set { isDescriptiveStatisticsVisible = value; OnPropertyChanged("IsDescriptiveStatisticsVisible"); } }
        
        private Boolean isDistributionVisible = false;
        public Boolean IsDistributionVisible { get { return isDistributionVisible; } set { isDistributionVisible = value; OnPropertyChanged("IsDistributionVisible"); } }

        private Boolean isGroupedBarChartVisible = false;
        public Boolean IsGroupedBarChartVisible { get { return isGroupedBarChartVisible; } set { isGroupedBarChartVisible = value; OnPropertyChanged("IsGroupedBarChartVisible"); } }

        private Boolean isScatterplotVisible = false;
        public Boolean IsScatterplotVisible { get { return isScatterplotVisible; } set { isScatterplotVisible = value; OnPropertyChanged("IsScatterplotVisible"); } }

        private Boolean isPivotTableVisible = false;
        public Boolean IsPivotTableVisible { get { return isPivotTableVisible; } set { isPivotTableVisible = value; OnPropertyChanged("IsPivotTableVisible"); } }

        private Boolean isCorrelationStatisticsVisible = false;
        public Boolean IsCorrelationStatisticsVisible { get { return isCorrelationStatisticsVisible; } set { isCorrelationStatisticsVisible = value; OnPropertyChanged("IsCorrelationStatisticsVisible"); } }

        private Boolean isBarChartWarningVisible = false;
        public Boolean IsBarChartWarningVisible { get { return isBarChartWarningVisible; } set { isBarChartWarningVisible = value; OnPropertyChanged(nameof(IsBarChartWarningVisible)); } }

        private Boolean isLineChartWarningVisible = false;
        public Boolean IsLineChartWarningVisible { get { return isLineChartWarningVisible; } set { isLineChartWarningVisible = value; OnPropertyChanged(nameof(IsLineChartWarningVisible)); } }

        private Boolean isGroupedBarChartWarningVisible = false;
        public Boolean IsGroupedBarChartWarningVisible { get { return isGroupedBarChartWarningVisible; } set { isGroupedBarChartWarningVisible = value; OnPropertyChanged(nameof(IsGroupedBarChartWarningVisible)); } }

        private Boolean isScatterplotWarningVisible = false;
        public Boolean IsScatterplotWarningVisible { get { return isScatterplotWarningVisible; } set { isScatterplotWarningVisible = value; OnPropertyChanged(nameof(IsScatterplotWarningVisible)); } }

        private Boolean isPrimaryUndoMessageVisible = true;
        public Boolean IsPrimaryUndoMessageVisible { get { return isPrimaryUndoMessageVisible; } set { isPrimaryUndoMessageVisible = value; OnPropertyChanged(nameof(IsPrimaryUndoMessageVisible)); } }

        PageView pageView;

        public ViewStatus ViewStatus { get; set; } // 현재 페이지 뷰의 viewStatus
        
        public PageViewModel(MainPageViewModel mainPageViewModel, PageView pageView)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.pageView = pageView;
            this.pivotTableViewModel = new PivotTableViewModel(mainPageViewModel, pageView.PivotTableView);
        }        

        public void FilterOut(FilterViewModel filterViewModel)
        {
            mainPageViewModel.ExplorationViewModel.FilterOut(filterViewModel);
        }

        /// <summary>
        /// 상태가 바뀌었으면 일단 exploration에 보고를 한 후 거기서 reflect 메소드를 호출해서 이 페이지 뷰와 뷰모델에 반영한다.
        /// </summary>
        /// <param name="pageView"></param>
        /// <param name="isUndo"></param>
        public void StateChanged(PageView pageView)
        {
            mainPageViewModel.ExplorationViewModel.PageViewStateChanged(this, pageView);
        }
        
        /// <summary>
        /// 페이지뷰의 상태가 바꾸면 우선 exploration view에 보고를 한 후 거기서 reflect를 호출함. exploration view에 호출하지 않아도 될 때만 직접 reflect를 해야함
        /// </summary>
        /// <param name="trackPreviousParagraph"></param>
        public void Reflect(Boolean trackPreviousParagraph)
        {
            IsBarChartVisible = false;
            IsLineChartVisible = false;
            IsDescriptiveStatisticsVisible = false;
            IsDistributionVisible = false;
            IsGroupedBarChartVisible = false;
            IsScatterplotVisible = false;
            IsPivotTableVisible = false;
            IsCorrelationStatisticsVisible = false;

            IsBarChartWarningVisible = false;
            IsLineChartWarningVisible = false;
            IsGroupedBarChartWarningVisible = false;
            IsScatterplotWarningVisible = false;

            List<ColumnViewModel> selectedColumnViewModels = ViewStatus.SelectedColumnViewModels;
            List<ColumnViewModel> numericalColumns = selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).ToList();
            List<ColumnViewModel> categoricalColumns = selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ToList();

            List<GroupedRows> groupedRows = ViewStatus.GroupedRows;
            Object firstChartTag = "dummy tag wer";

            if (ViewStatus.IsC)
            {
                DrawFrequencyHistogram(ViewStatus.FirstCategorical, groupedRows);
                DrawPivotTable();
            }
            else if (ViewStatus.IsN)
            {
                DrawDescriptiveStatistics(ViewStatus.FirstNumerical);
                DrawDistributionHistogram(ViewStatus.FirstNumerical);
                DrawPivotTable();
            }
            else if (ViewStatus.IsCC)
            {
                DrawGroupedBarChart(ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, groupedRows);
                DrawPivotTable();
            }
            else if (ViewStatus.IsCN)
            {
                DrawBarChart(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, groupedRows);
                DrawLineChart(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, groupedRows);
                DrawPivotTable();

                firstChartTag = ViewStatus.FirstCategorical.CategoricalType == CategoricalType.Ordinal ? pageView.LineChart.Tag : pageView.BarChart.Tag;
            }
            else if (ViewStatus.IsNN)
            {
                DrawCorrelatonStatistics(ViewStatus.FirstNumerical, ViewStatus.SecondNumerical);
                DrawScatterplot(ViewStatus.FirstNumerical, ViewStatus.SecondNumerical);
                if (ViewStatus.FirstNumerical.Unit == ViewStatus.SecondNumerical.Unit) // 둘의 단위가 같으면 그룹 바 차트 가능
                {
                    DrawGroupedBarChartNN(ViewStatus.FirstNumerical, ViewStatus.SecondNumerical, groupedRows);
                }
            }
            else if (ViewStatus.IsCCC)
            {
                DrawPivotTable();
            }
            else if (ViewStatus.IsCCN)
            {
                DrawGroupedBarChart(ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, ViewStatus.FirstNumerical, groupedRows);
                DrawLineChart(ViewStatus.FirstCategorical, ViewStatus.SecondCategorical, ViewStatus.FirstNumerical, groupedRows);
                DrawPivotTable();
            }
            else if (ViewStatus.IsCNN)
            {
                // 스캐터플롯을 그린다.
                DrawScatterplot(categoricalColumns.First(), numericalColumns[0], numericalColumns[1]);

                if(ViewStatus.FirstNumerical.Unit == ViewStatus.SecondNumerical.Unit) // 둘의 단위가 같으면 그룹 바 차트 가능
                {
                    DrawGroupedBarChartCNN(ViewStatus.FirstCategorical, ViewStatus.FirstNumerical, ViewStatus.SecondNumerical, groupedRows);
                }

                DrawPivotTable();
            }
            else if (ViewStatus.IsNNN)
            {
                // ???
                IsBarChartVisible = true;
                // 지금 필요없다.
            }
            else if (ViewStatus.IsCnN0)
            {
                DrawPivotTable();
            }
            else if (ViewStatus.IsCnN1)
            {
                DrawPivotTable();
            }
            else if (ViewStatus.IsCnNn)
            {
                DrawPivotTable();
            }
            else
            {
                IsBarChartVisible = true;
            }

            pageView.UpdateCarousel(trackPreviousParagraph, firstChartTag?.ToString());
        }

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

        public String Concatenate(IEnumerable<String> words)
        {
            Int32 count = words.Count();
            if (count == 1)
            {
                return words.First();
            }
            else if(count == 2)
            {
                return String.Join(" and ", words);
            }

            try
            {
                return String.Join(", ", words.Where((s, i) => i < count - 1)) + ", and " + words.Last();
            }
            catch
            {
                return "Wrong Title";
            }
        }

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
            if (!IsSelected)
            {
                AddText(stackPanel, selected);
            }
            else
            {
                ComboBox comboBox = new ComboBox()
                {
                    Style = App.Current.Resources["SeamlessComboBoxStyle"] as Style,
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
            if(currentColumnViewModel.Type == ColumnType.Numerical)
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

                mainPageViewModel.ReflectAll();
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

                mainPageViewModel.ReflectAll();
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
            if(prefix != null && prefix.Length > 0)
            {
                AddText(title, prefix);
            }

            Int32 index;
            index = 0;
            foreach(ColumnViewModel variable in variables)
            {
                if(variable.Type == ColumnType.Categorical)
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

                if (index < variables.Count - 1) {
                    if (variables.Count == 2)
                    {
                        AddText(title, "\x00A0and\x00A0");
                    }
                    else
                    {
                        AddText(title, ",\x00A0");
                        if(index == variables.Count - 2)
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

        #region Visualizaiton Generator

        void DrawFrequencyHistogram(ColumnViewModel categorical, List<GroupedRows> groupedRows)
        {
            IsBarChartVisible = true;

            pageView.BarChartTitle.Children.Clear();
            if (IsSelected)
            {
                AddText(pageView.BarChartTitle, "Frequency of\x00A0");
                AddComboBox(
                    pageView.BarChartTitle,
                    categorical.Name,
                    mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
                    CreateColumnChangedHandler(categorical)
                );
            }
            else
            {
                AddText(pageView.BarChartTitle, $"Frequency of <b>{categorical.Name}</b>");
            }

            Int32 index = categorical.Index;

            pageView.BarChart.YStartsFromZero = true;
            pageView.BarChart.HorizontalAxisTitle = categorical.Name;
            pageView.BarChart.VerticalAxisTitle = String.Format("Frequency");
            pageView.BarChart.Data = groupedRows
                .Select(grs => new BarChartDatum() { Key = grs.Keys[categorical], Value = grs.Rows.Count, Rows = grs.Rows, ColumnViewModel = categorical })
                .Take(BarChartMaximumRecordNumber).ToList();
            if (groupedRows.Count > BarChartMaximumRecordNumber) IsBarChartWarningVisible = true;
            pageView.BarChart.Update();
        }

        void DrawDescriptiveStatistics(ColumnViewModel numerical)
        {
            DescriptiveStatisticsResult result = DescriptiveStatistics.Analyze(
                    mainPageViewModel.SheetViewModel.AllRowViewModels.Select(r => (Double)r.Cells[numerical.Index].Content)
                    );

            pageView.DescriptiveStatisticsTitle.Children.Clear();
            if (IsSelected/* && mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Count() > 1*/) // 선택지가 하나밖에 없으면 셀렉트 박스 보여줄 필요가 없음.
            {
                DrawEditableTitleN(pageView.DescriptiveStatisticsTitle, numerical, "Descriptive Statistics of\x00A0");
            }
            else
            {
                AddText(pageView.DescriptiveStatisticsTitle, $"Descriptive Statistics of <b>{numerical.Name}</b>");
            }

            IsDescriptiveStatisticsVisible = true;
            pageView.DescriptiveStatisticsView.DataContext = result;            
        }

        void DrawDistributionHistogram(ColumnViewModel numerical)
        {
            pageView.DistributionViewTitle.Children.Clear();

            if (IsSelected/* && mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Count() > 1*/) // 선택지가 하나밖에 없으면 selectbox 보여줄 필요 없음
            {
                DrawEditableTitleN(pageView.DistributionViewTitle, numerical, "Distribution of\x00A0");
            }
            else
            {
                AddText(pageView.DistributionViewTitle, $"Distribution of <b>{numerical.Name}</b>");
            }
            
            IsDistributionVisible = true;
            
            pageView.DistributionView.Update(
                mainPageViewModel.SheetViewModel.FilteredRows, 
                numerical
                ); // 히스토그램 업데이트
        }

        void DrawGroupedBarChart(ColumnViewModel categorical1, ColumnViewModel categorical2, List<GroupedRows> groupedRows)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            if(IsSelected)
            {
                AddText(pageView.GroupedBarChartTitle, "Frequency of\x00A0");
                AddComboBox(
                    pageView.GroupedBarChartTitle,
                    categorical2.Name,
                    mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
                    CreateColumnChangedHandler(categorical2)
                );
                AddText(pageView.GroupedBarChartTitle, "\x00A0by\x00A0");
                AddComboBox(
                    pageView.GroupedBarChartTitle,
                    categorical1.Name,
                    mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).Select(cvm => cvm.Name),
                    CreateColumnChangedHandler(categorical1)
                );
            }
            else
            {
                AddText(pageView.GroupedBarChartTitle, $"Frequency of <b>{categorical2.Name}</b> by <b>{categorical1.Name}</b>");
            }

            pageView.GroupedBarChart.YStartsFromZero = true;
            pageView.GroupedBarChart.HorizontalAxisTitle = categorical1.Name;
            pageView.GroupedBarChart.VerticalAxisTitle = $"Frequency of {categorical2.Name}";

            if (groupedRows.Count > GroupedBarChartMaximumRecordNumber)
            {
                groupedRows = groupedRows.Take(GroupedBarChartMaximumRecordNumber).ToList();
                IsGroupedBarChartWarningVisible = true;
            }

            var validLegends = groupedRows.GroupBy(g => g.Keys[categorical2]).Select(g => g.Key);

            if (validLegends.Count() > BarChartMaximumRecordNumber)
            {
                // number of categories in legend
                validLegends = validLegends.Take(BarChartMaximumRecordNumber);
                IsGroupedBarChartWarningVisible = true;
            }

            pageView.GroupedBarChart.Data = groupedRows
                        .GroupBy(g => g.Keys[categorical1])
                        .Select(gs =>
                        {
                            GroupedBarChartDatum datum = new GroupedBarChartDatum()
                            {
                                ColumnViewModel = categorical1,
                                Key = gs.Key
                            };

                            datum.Children = gs.Where(g => validLegends.Contains(g.Keys[categorical2]))
                            .Select(g => new BarChartDatum()
                            {
                                Key = g.Keys[categorical2],
                                Value = g.Rows.Count,
                                ColumnViewModel = categorical2,
                                Parent = datum,
                                Rows = g.Rows
                            }).ToList();
                            return datum;
                        })
                        .Where(datum => datum.Children != null && datum.Children.Count > 0)
                        .ToList();            

            pageView.GroupedBarChart.Update();           
        }

        void DrawGroupedBarChart(ColumnViewModel categorical1, ColumnViewModel categorical2, ColumnViewModel numerical, List<GroupedRows> groupedRows)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            if (IsSelected)
            {
                DrawEditableTitleCCN(pageView.GroupedBarChartTitle, categorical1, categorical2, numerical);
            }
            else
            {
                AddText(pageView.GroupedBarChartTitle, $"<b>{numerical.HeaderName}</b> by <b>{categorical1.Name}</b> and <b>{categorical2.Name}</b>");
            }

            pageView.GroupedBarChart.YStartsFromZero = false;
            pageView.GroupedBarChart.HorizontalAxisTitle = categorical1.Name;
            pageView.GroupedBarChart.VerticalAxisTitle = numerical.HeaderNameWithUnit;

            if (groupedRows.Count > GroupedBarChartMaximumRecordNumber)
            {
                groupedRows = groupedRows.Take(GroupedBarChartMaximumRecordNumber).ToList();
                IsGroupedBarChartWarningVisible = true;
            }

            var validLegends = groupedRows.GroupBy(g => g.Keys[categorical2]).Select(g => g.Key);

            if (validLegends.Count() > BarChartMaximumRecordNumber)
            {
                // number of categories in legend
                validLegends = validLegends.Take(BarChartMaximumRecordNumber);
                IsGroupedBarChartWarningVisible = true;
            }

            pageView.GroupedBarChart.Data = groupedRows
                        .GroupBy(g => g.Keys[categorical1])
                        .Select(gs =>
                        {
                            GroupedBarChartDatum datum = new GroupedBarChartDatum()
                            {
                                ColumnViewModel = categorical1,
                                Key = gs.Key
                            };

                            datum.Children = gs.Where(g => validLegends.Contains(g.Keys[categorical2]))
                            .Select(g => new BarChartDatum()
                            {
                                Key = g.Keys[categorical2],
                                Value = numerical.AggregativeFunction.Aggregate(g.Rows.Select(row => (Double)row.Cells[numerical.Index].Content)),
                                ColumnViewModel = categorical2,
                                Parent = datum,
                                Rows = g.Rows
                            }).ToList();
                            return datum;
                        })
                        .Where(datum => datum.Children != null && datum.Children.Count > 0)
                        .ToList();

            pageView.GroupedBarChart.Update();
        }

        void DrawBarChart(ColumnViewModel categorical, ColumnViewModel numerical, List<GroupedRows> groupedRows)
        {
            IsBarChartVisible = true;

            pageView.BarChartTitle.Children.Clear();
            if(IsSelected)
            {
                DrawEditableTitleCN(pageView.BarChartTitle, categorical, numerical);
            }
            else
            {
                AddText(pageView.BarChartTitle, $"<b>{numerical.AggregatedName}</b> by <b>{categorical.Name}</b>");
            }

            pageView.BarChart.YStartsFromZero = false;
            pageView.BarChart.HorizontalAxisTitle = categorical.Name;
            pageView.BarChart.VerticalAxisTitle = numerical.HeaderNameWithUnit;
            pageView.BarChart.Data = groupedRows
                .Select(g => new BarChartDatum()
                {
                    Key = g.Keys[categorical],
                    Value = numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                    Rows = g.Rows,
                    ColumnViewModel = categorical
                })
                .Take(BarChartMaximumRecordNumber).ToList();

            if (groupedRows.Count > BarChartMaximumRecordNumber) IsBarChartWarningVisible = true;
            pageView.BarChart.Update();
        }

        void DrawLineChart(ColumnViewModel categorical, ColumnViewModel numerical, List<GroupedRows> groupedRows) // 라인 하나
        {
            //라인 차트로 보고 싶을 때
            IsLineChartVisible = true;

            pageView.LineChartTitle.Children.Clear();
            if (IsSelected)
            {
                DrawEditableTitleCN(pageView.LineChartTitle, categorical, numerical);
            }
            else
            {
                AddText(pageView.LineChartTitle, $"<b>{numerical.HeaderName}</b> by <b>{categorical.Name}</b>");
            }

            pageView.LineChart.YStartsFromZero = false;
            pageView.LineChart.HorizontalAxisTitle = categorical.Name;
            pageView.LineChart.VerticalAxisTitle = numerical.HeaderNameWithUnit;
            pageView.LineChart.AutoColor = false;

            LineChartDatum datum = new LineChartDatum()
            {
                ColumnViewModel = numerical,
                Key = numerical.AggregatedName,
                Rows = mainPageViewModel.SheetViewModel.FilteredRows
            };

            datum.DataPoints = groupedRows
                .Select(g => new DataPoint(
                    g.Keys[categorical],
                    numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)),
                    datum
                ))
                .Take(LineChartMaximumPointNumberInASeries)
                .ToList();

            pageView.LineChart.Data = new List<LineChartDatum>() { datum };
            if (groupedRows.Count > LineChartMaximumPointNumberInASeries) IsLineChartWarningVisible = true;
            
            pageView.LineChart.Update();
        }

        void DrawLineChart(ColumnViewModel categorical1, ColumnViewModel categorical2, ColumnViewModel numerical, List<GroupedRows> groupedRows)
        {
            Debug.Assert(categorical1.Type == ColumnType.Categorical);
            Debug.Assert(categorical2.Type == ColumnType.Categorical);
            Debug.Assert(numerical.Type == ColumnType.Numerical);
            // 그룹 라인 차트를 그린다.
            IsLineChartVisible = true;

            pageView.LineChartTitle.Children.Clear();
            if (IsSelected)
            {
                DrawEditableTitleCCN(pageView.LineChartTitle, categorical1, categorical2, numerical);
            }
            else
            {
                AddText(pageView.LineChartTitle, $"<b>{numerical.HeaderName}</b> by <b>{categorical1.Name}</b> and <b>{categorical2.Name}</b>");
            }
            
            pageView.LineChart.YStartsFromZero = false;
            pageView.LineChart.HorizontalAxisTitle = categorical1.Name;
            pageView.LineChart.VerticalAxisTitle = numerical.HeaderNameWithUnit;
            pageView.LineChart.AutoColor = true;

            var rows =
                groupedRows
                .GroupBy(grs => grs.Keys[categorical2]) // 먼저 묶고 
                .Select(group => {
                    LineChartDatum datum = new LineChartDatum()
                    {
                        ColumnViewModel = categorical2,
                        Key = group.Key,
                        Rows = group.SelectMany(grs => grs.Rows)
                    };

                    datum.DataPoints = group.Select(g => new DataPoint(
                        g.Keys[categorical1],
                        numerical.AggregativeFunction.Aggregate(
                                g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content)
                            ),
                        datum
                    )).ToList();

                    return datum;
                })
                .Take(LineChartMaximumSeriesNumber);

            if (rows.Count() > LineChartMaximumSeriesNumber) IsLineChartWarningVisible = true;
            if (rows.Max(row => row.DataPoints.Count) > LineChartMaximumPointNumberInASeries) IsLineChartWarningVisible = true;

            pageView.LineChart.Data = rows.ToList();
            pageView.LineChart.Update();
        }

        void DrawScatterplot(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            IsScatterplotVisible = true;

            pageView.ScatterplotTitle.Children.Clear();
            if(IsSelected)
            {
                DrawEditableTitleCNvsN(pageView.ScatterplotTitle, categorical, numerical1, numerical2);
            }
            else
            {
                AddText(pageView.ScatterplotTitle, $"<b>{numerical1.Name}</b> vs. <b>{numerical2.Name}</b> colored by <b>{categorical.Name}</b>");
            }
            
            pageView.Scatterplot.LegendVisibility = Visibility.Visible;
            pageView.Scatterplot.HorizontalAxisTitle = numerical1.Name + numerical1.UnitString;
            pageView.Scatterplot.VerticalAxisTitle = numerical2.Name + numerical2.UnitString;
            pageView.Scatterplot.AutoColor = true;

            var data = mainPageViewModel.SheetViewModel.FilteredRows
                .Select(r => new ScatterplotDatum(
                    r.Cells[categorical.Index].Content,
                    (Double)r.Cells[numerical1.Index].Content,
                    (Double)r.Cells[numerical2.Index].Content,
                    r,
                    categorical
                    ));

            if (data.Select(d => d.Key).Distinct().Count() > ScatterplotMaximumCategoryNumber) {
                IsScatterplotWarningVisible = true;
                var categories = data.Select(d => d.Key).Distinct().Take(ScatterplotMaximumCategoryNumber).ToList();
                data = data.Where(d => categories.IndexOf(d.Key) >= 0);
            }

            pageView.Scatterplot.Data = data.ToList();

            pageView.Scatterplot.Update();
        }

        void DrawScatterplot(ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            IsScatterplotVisible = true;

            pageView.ScatterplotTitle.Children.Clear();
            if (IsSelected)
            {
                DrawEditableTitleNvsN(pageView.ScatterplotTitle, numerical1, numerical2);
            }
            else
            {
                AddText(pageView.ScatterplotTitle, $"<b>{numerical1.Name}</b> vs. <b>{numerical2.Name}</b>");
            }

            pageView.Scatterplot.LegendVisibility = Visibility.Collapsed;
            pageView.Scatterplot.HorizontalAxisTitle = numerical1.Name + numerical1.UnitString;
            pageView.Scatterplot.VerticalAxisTitle = numerical2.Name + numerical2.UnitString;
            pageView.Scatterplot.AutoColor = false;
            pageView.Scatterplot.Data = mainPageViewModel.SheetViewModel.FilteredRows
                .Select(r => new ScatterplotDatum(
                    0, 
                    (Double)r.Cells[numerical1.Index].Content, 
                    (Double)r.Cells[numerical2.Index].Content, 
                    r,
                    null))
                .ToList();

            pageView.Scatterplot.Update();
        }

        void DrawGroupedBarChartNN(ColumnViewModel numerical1, ColumnViewModel numerical2, List<GroupedRows> groupedRows)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            if (IsSelected)
            {
                DrawEditableTitleNN(pageView.GroupedBarChartTitle, numerical1, numerical2);
            }
            else
            {
                AddText(pageView.GroupedBarChartTitle, $"<b>{numerical1.AggregatedName}</b> and <b>{numerical2.AggregatedName}</b>");
            }

            pageView.GroupedBarChart.YStartsFromZero = true;
            pageView.GroupedBarChart.HorizontalAxisTitle = "";
            pageView.GroupedBarChart.VerticalAxisTitle = $"{numerical1.UnitString}";

            Category category1 = new Category() { Value = numerical1.Name, Color = Category10.Colors[0], IsVirtual = true },
                     category2 = new Category() { Value = numerical2.Name, Color = Category10.Colors[1], IsVirtual = true }, 
                     dummyKey = new Category() { Value = $"{numerical1.Name} and {numerical2.Name} ", IsVirtual = true };

            GroupedBarChartDatum datum = new GroupedBarChartDatum()
            {
                ColumnViewModel = null,
                Key = dummyKey
            };

            datum.Children = new List<BarChartDatum>()
                            {
                                new BarChartDatum()
                                {
                                    ColumnViewModel = null,
                                    Key = category1,
                                    Parent = datum,
                                    Value = numerical1.AggregativeFunction.Aggregate(mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical1.Index].Content)),
                                    Rows = mainPageViewModel.SheetViewModel.FilteredRows
                                },
                                new BarChartDatum()
                                {
                                    ColumnViewModel = null,
                                    Key = category2,
                                    Parent = datum,
                                    Value = numerical1.AggregativeFunction.Aggregate(mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical2.Index].Content)),
                                    Rows = mainPageViewModel.SheetViewModel.FilteredRows
                                }
                            };

            pageView.GroupedBarChart.Data = new List<GroupedBarChartDatum>() { datum };

            pageView.GroupedBarChart.Update();
        }

        void DrawGroupedBarChartCNN(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2, List<GroupedRows> groupedRows)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            if (IsSelected)
            {
                DrawEditableTitleCNN(pageView.GroupedBarChartTitle, categorical, numerical1, numerical2);
            }
            else
            {
                AddText(pageView.GroupedBarChartTitle, $"<b>{numerical1.AggregatedName}</b> and <b>{numerical2.AggregatedName}</b> by <b>{categorical.Name}</b>");
            }

            pageView.GroupedBarChart.YStartsFromZero = true;
            pageView.GroupedBarChart.HorizontalAxisTitle = categorical.Name;
            pageView.GroupedBarChart.VerticalAxisTitle = $"{numerical1.Name} and {numerical2.Name} {numerical1.UnitString}";

            Category category1 = new Category() { Value = numerical1.Name, Color = Category10.Colors[0], IsVirtual = true },
                     category2 = new Category() { Value = numerical2.Name, Color = Category10.Colors[1], IsVirtual = true };

            var data = groupedRows
                        .Select(g =>
                        {
                            GroupedBarChartDatum datum = new GroupedBarChartDatum()
                            {
                                ColumnViewModel = categorical,
                                Key = g.Keys[categorical]
                            };

                            datum.Children = new List<BarChartDatum>()
                            {
                                new BarChartDatum()
                                {
                                    ColumnViewModel = null,
                                    Key = category1,
                                    Parent = datum,
                                    Value = numerical1.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical1.Index].Content)),
                                    Rows = g.Rows
                                },
                                new BarChartDatum()
                                {
                                    ColumnViewModel = null,
                                    Key = category2,
                                    Parent = datum,
                                    Value = numerical2.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical2.Index].Content)),
                                    Rows = g.Rows
                                }
                            };
                            return datum;
                        });

            if (data.Count() > GroupedBarChartMaximumRecordNumber) IsGroupedBarChartWarningVisible = true;
            pageView.GroupedBarChart.Data = data.Take(GroupedBarChartMaximumRecordNumber).ToList();

            pageView.GroupedBarChart.Update();
        }

        void DrawCorrelatonStatistics(ColumnViewModel numerical1, ColumnViewModel numerical2)
        {
            CorrelationStatisticsResult result = CorrelationStatistics.Analyze(
                numerical1.Name,
                numerical2.Name,
                mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical1.Index].Content),
                mainPageViewModel.SheetViewModel.FilteredRows.Select(r => (Double)r.Cells[numerical2.Index].Content)
                );

            pageView.CorrelationStatisticsTitle.Children.Clear();
            if (IsSelected)
            {
                DrawEditableTitleNvsN(pageView.CorrelationStatisticsTitle, numerical1, numerical2);
            }
            else
            {
                AddText(pageView.CorrelationStatisticsTitle, $"<b>{numerical1.Name}</b> vs. <b>{numerical2.Name}</b>");
            }

            IsCorrelationStatisticsVisible = true;
            pageView.CorrelationStatisticsView.DataContext = result;
        }
        #endregion
    }
}
