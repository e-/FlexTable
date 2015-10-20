using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using FlexTable.Model;
using Windows.UI.Xaml;
using FlexTable.View;
using Windows.UI.Xaml.Controls;
using Series = System.Tuple<System.String, System.Collections.Generic.List<System.Tuple<System.Object, System.Double>>>;
using DataPoint = System.Tuple<System.Object, System.Double>;
using Windows.UI.Xaml.Documents;
using Windows.UI.Text;
using System.Diagnostics;

namespace FlexTable.ViewModel
{
    public class PageViewModel : NotifyViewModel
    {
        const Int32 BarChartMaximumRecordNumber = 12;
        const Int32 GroupedBarChartMaximumRecordNumber = 48;
        const Int32 LineChartMaximumSeriesNumber = BarChartMaximumRecordNumber;
        const Int32 LineChartMaximumPointNumberInASeries = 14;

        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel => mainPageViewModel;

        PivotTableViewModel pivotTableViewModel;
        public PivotTableViewModel PivotTableViewModel => pivotTableViewModel; 

        private Boolean isSummaryVisible = false;
        public Boolean IsSummaryVisible { get { return isSummaryVisible; } set { isSummaryVisible = value; OnPropertyChanged("IsSummaryVisible"); } }

        private Boolean isBarChartVisible = false;
        public Boolean IsBarChartVisible { get { return isBarChartVisible; } set { isBarChartVisible = value; OnPropertyChanged("IsBarChartVisible"); } }

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
        public Boolean IsGroupedBarChartWarningVisible { get { return isGroupedBarChartWarningVisible; } set { isGroupedBarChartWarningVisible = value; OnPropertyChanged(nameof(isGroupedBarChartWarningVisible)); } }

        private Boolean isSelected = false;
        public Boolean IsSelected { get { return isSelected; } set { isSelected = value; OnPropertyChanged("IsSelected"); } }
        
        public Func<Category, Func<RowViewModel, Boolean>> BarChartRowSelecter { get; set; }
        public Func<Category, Category, Func<RowViewModel, Boolean>> GroupedBarChartRowSelecter { get; set; }
        public Func<Series, Func<RowViewModel, Boolean>> LineChartRowSelecter { get; set; }
        public Func<Category, Func<RowViewModel, Boolean>> ScatterplotRowSelecter { get; set; }

        PageView pageView;

        public ViewStatus ViewStatus { get; set; } // 현재 페이지 뷰의 viewStatus
        
        public PageViewModel(MainPageViewModel mainPageViewModel, PageView pageView)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.pageView = pageView;
            this.pivotTableViewModel = new PivotTableViewModel(mainPageViewModel, pageView.PivotTableView);
            //this.customHistogramViewModel = new CustomHistogramViewModel(mainPageViewModel, pageView.CustomHistogramView);
        }

        public void Hide()
        {
            IsSummaryVisible = false;
        }

        public void Tapped(PageView pageView)
        {
            mainPageViewModel.ExplorationViewModel.PageViewTapped(this, pageView);
        }

        public void Unselect()
        {
            pageView.Unselect();
            pageView.UpdateCarousel();
        }

        public void Select()
        {
            pageView.Select();
            pageView.UpdateCarousel();
        }

        public void StrokeAdded(InkStroke stroke)
        {
            /*
            Int32 index = 0;
            Rect rect = stroke.BoundingRect;
            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            foreach (Model.Bin bin in column.Bins.Select(b => b as Object).ToList())
            {
                Double x0 = LegendTextXGetter(bin, index),
                       y0 = LegendPatchYGetter(bin, index) + 10,
                       y1 = y0 + LegendPatchHeightGetter(bin, index) + 10;

                if (x0 <= center.X - mainPageViewModel.Width / 2 + ChartWidth && y0 <= center.Y && center.Y <= y1)
                {
                    bin.IsFilteredOut = !bin.IsFilteredOut;
                    break;
                }             
                index++;
            }

            d3.Scale.Ordinal xScale = new d3.Scale.Ordinal()
            {
                RangeStart = 70,
                RangeEnd = ChartWidth
            };
            foreach (Model.Bin bin in column.Bins.Where(b => !b.IsFilteredOut)) { xScale.Domain.Add(bin.Name); }
            XScale = xScale;

            Data = new d3.Selection.Data()
            {
                Real = column.Bins.Where(b => !b.IsFilteredOut).Select(b => b as Object).ToList()
            };

            LegendData = new d3.Selection.Data()
            {
                Real = column.Bins.Select(b => b as Object).ToList()
            };

            mainPageViewModel.UpdateFiltering();*/
        }

        public void Reflect()
        {
            IsSummaryVisible = true;
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

            List<ColumnViewModel> selectedColumnViewModels = ViewStatus.SelectedColumnViewModels;
            List<ColumnViewModel> numericalColumns = selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).ToList();
            List<ColumnViewModel> categoricalColumns = selectedColumnViewModels.Where(cvm => cvm.Type == ColumnType.Categorical).ToList();

            List<GroupedRows> groupedRows = null;
            Int32 numericalCount = numericalColumns.Count;
            Int32 categoricalCount = categoricalColumns.Count;

            if (categoricalColumns.Count > 0)
            {
                groupedRows = SheetViewModel.GroupRecursive(mainPageViewModel.SheetViewModel.Sheet.Rows.ToList(), categoricalColumns, 0);
            }

            if (categoricalCount == 1 && numericalCount == 0)
            {
                DrawFrequencyHistogram(selectedColumnViewModels.First(), groupedRows, IsSelected);          
            }
            else if (categoricalCount == 0 && numericalCount == 1)
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
            }
            else if (categoricalCount == 1 && numericalCount == 2)
            {
                // 스캐터플롯을 그린다.
                DrawScatterplot(categoricalColumns.First(), numericalColumns[0], numericalColumns[1], IsSelected);

                if(numericalColumns[0].Unit == numericalColumns[1].Unit) // 둘의 단위가 같으면 그룹 바 차트 가능
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
            }
            else
            {
                IsBarChartVisible = true;
            }
            
            pageView.UpdateCarousel();
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
            ComboBox comboBox = new ComboBox()
            {
                Style = App.Current.Resources["SeamlessComboBoxStyle"] as Style,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 30
            };

            foreach(String candidate in candidates)
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

                // 2. 테이블 변경
                mainPageViewModel.SheetViewModel.UpdateGroup(ViewStatus);
                mainPageViewModel.TableViewModel.Reflect(ViewStatus);

                // 3. 차트 변경
                Reflect();
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

                // 2. 테이블 변경
                mainPageViewModel.SheetViewModel.UpdateGroup(ViewStatus);
                mainPageViewModel.TableViewModel.Reflect(ViewStatus);

                // 3. 차트 변경
                Reflect();
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

        void DrawFrequencyHistogram(ColumnViewModel categorical, List<GroupedRows> groupedRows, Boolean isTitleEditable)
        {
            IsBarChartVisible = true;

            pageView.BarChartTitle.Children.Clear();
            if (isTitleEditable)
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
            BarChartRowSelecter = c => (r => r.Cells[index].Content == c);

            pageView.BarChart.YStartsWithZero = true;
            pageView.BarChart.HorizontalAxisLabel = categorical.Name;
            pageView.BarChart.VerticalAxisLabel = String.Format("Frequency");
            pageView.BarChart.Data = groupedRows
                .Select(grs => new Tuple<Object, Double>(grs.Keys[categorical], grs.Rows.Count))
                .OrderBy(t => (t.Item1 as Category).Order)
                .Take(BarChartMaximumRecordNumber);
            if (groupedRows.Count > BarChartMaximumRecordNumber) IsBarChartWarningVisible = true;
            pageView.BarChart.Update();
        }

        void DrawDescriptiveStatistics(ColumnViewModel numerical, Boolean isTitleEditable)
        {
            DescriptiveStatisticsResult result = DescriptiveStatistics.Analyze(
                    mainPageViewModel.SheetViewModel.AllRowViewModels.Select(r => (Double)r.Cells[numerical.Index].Content)
                    );

            pageView.DescriptiveStatisticsTitle.Children.Clear();
            if (isTitleEditable/* && mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Count() > 1*/) // 선택지가 하나밖에 없으면 셀렉트 박스 보여줄 필요가 없음.
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

        void DrawDistributionHistogram(ColumnViewModel numerical, Boolean isTitleEditable)
        {
            DescriptiveStatisticsResult result = DescriptiveStatistics.Analyze(
                       mainPageViewModel.SheetViewModel.AllRowViewModels.Select(r => (Double)r.Cells[numerical.Index].Content)
                       );

            pageView.DistributionViewTitle.Children.Clear();

            if (isTitleEditable/* && mainPageViewModel.SheetViewModel.ColumnViewModels.Where(cvm => cvm.Type == ColumnType.Numerical).Count() > 1*/) // 선택지가 하나밖에 없으면 selectbox 보여줄 필요 없음
            {
                DrawEditableTitleN(pageView.DistributionViewTitle, numerical, "Distribution of\x00A0");
            }
            else
            {
                AddText(pageView.DistributionViewTitle, $"Distribution of <b>{numerical.Name}</b>");
            }
            
            IsDistributionVisible = true;
            pageView.DistributionView.Update(
                result,
                mainPageViewModel.SheetViewModel.AllRowViewModels.Select(r => (Double)r.Cells[numerical.Index].Content)
                ); // 히스토그램 업데이트
        }

        void DrawGroupedBarChart(ColumnViewModel categorical1, ColumnViewModel categorical2, List<GroupedRows> groupedRows, Boolean isTitleEditable)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            if(isTitleEditable)
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

            GroupedBarChartRowSelecter = (c1, c2) => (r => r.Cells[categorical1.Index].Content == c1 && r.Cells[categorical2.Index].Content == c2);
            pageView.GroupedBarChart.YStartsWithZero = true;
            pageView.GroupedBarChart.HorizontalAxisLabel = categorical1.Name;
            pageView.GroupedBarChart.VerticalAxisLabel = $"Frequency of {categorical2.Name}";
            pageView.GroupedBarChart.Data = groupedRows
                        .OrderBy(g => (g.Keys[categorical1] as Category).Order * 10000 + (g.Keys[categorical2] as Category).Order)
                        .Select(g => new Tuple<Object, Object, Double>(
                            g.Keys[categorical1],
                            g.Keys[categorical2],
                            g.Rows.Count
                        ))
                        .Take(GroupedBarChartMaximumRecordNumber);
            if (groupedRows.Count > GroupedBarChartMaximumRecordNumber) IsGroupedBarChartWarningVisible = true;
            pageView.GroupedBarChart.Update();           
        }

        void DrawGroupedBarChart(ColumnViewModel categorical1, ColumnViewModel categorical2, ColumnViewModel numerical, List<GroupedRows> groupedRows, Boolean isTitleEditable)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            if (isTitleEditable)
            {
                DrawEditableTitleCCN(pageView.GroupedBarChartTitle, categorical1, categorical2, numerical);
            }
            else
            {
                AddText(pageView.GroupedBarChartTitle, $"<b>{numerical.HeaderName}</b> by <b>{categorical1.Name}</b> and <b>{categorical2.Name}</b>");
            }

            GroupedBarChartRowSelecter = (c1, c2) => (r => r.Cells[categorical1.Index].Content == c1 && r.Cells[categorical2.Index].Content == c2);

            pageView.GroupedBarChart.YStartsWithZero = false;
            pageView.GroupedBarChart.HorizontalAxisLabel = categorical1.Name;
            pageView.GroupedBarChart.VerticalAxisLabel = numerical.HeaderNameWithUnit;
            pageView.GroupedBarChart.Data = groupedRows
                        .OrderBy(g => (g.Keys[categorical1] as Category).Order * 10000 + (g.Keys[categorical2] as Category).Order)
                        .Select(g => new Tuple<Object, Object, Double>(
                            g.Keys[categorical1],
                            String.Format("{0} {1}", categorical2.Name, g.Keys[categorical2]),
                            numerical.AggregativeFunction.Aggregate(g.Rows.Select(row => (Double)row.Cells[numerical.Index].Content))
                        ))
                        .Take(GroupedBarChartMaximumRecordNumber);
            if (groupedRows.Count > GroupedBarChartMaximumRecordNumber) IsGroupedBarChartWarningVisible = true;
            pageView.GroupedBarChart.Update();
        }

        void DrawBarChart(ColumnViewModel categorical, ColumnViewModel numerical, List<GroupedRows> groupedRows, Boolean isTitleEditable)
        {
            IsBarChartVisible = true;

            pageView.BarChartTitle.Children.Clear();
            if(isTitleEditable)
            {
                DrawEditableTitleCN(pageView.BarChartTitle, categorical, numerical);
            }
            else
            {
                AddText(pageView.BarChartTitle, $"<b>{numerical.AggregatedName}</b> by <b>{categorical.Name}</b>");
            }

            BarChartRowSelecter = c => (r => r.Cells[categorical.Index].Content == c);

            pageView.BarChart.YStartsWithZero = false;
            pageView.BarChart.HorizontalAxisLabel = categorical.Name;
            pageView.BarChart.VerticalAxisLabel = numerical.HeaderNameWithUnit;
            pageView.BarChart.Data = groupedRows
                .OrderBy(g => (g.Keys[categorical] as Category).Order)
                .Select(g => new Tuple<Object, Double>(
                    g.Keys[categorical],
                    numerical.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical.Index].Content))
                    ))
                .Take(BarChartMaximumRecordNumber);
            if (groupedRows.Count > BarChartMaximumRecordNumber) IsBarChartWarningVisible = true;
            pageView.BarChart.Update();
        }

        void DrawLineChart(ColumnViewModel categorical, ColumnViewModel numerical, List<GroupedRows> groupedRows, Boolean isTitleEditable) // 라인 하나
        {
            //라인 차트로 보고 싶을 때
            IsLineChartVisible = true;

            pageView.LineChartTitle.Children.Clear();
            if (isTitleEditable)
            {
                DrawEditableTitleCN(pageView.LineChartTitle, categorical, numerical);
            }
            else
            {
                AddText(pageView.LineChartTitle, $"<b>{numerical.HeaderName}</b> by <b>{categorical.Name}</b>");
            }
            LineChartRowSelecter = series => (r => true); // r.Cells[categorical.Index].Content.ToString() == series.Item1);

            pageView.LineChart.YStartsWithZero = false;
            pageView.LineChart.HorizontalAxisLabel = categorical.Name;
            pageView.LineChart.VerticalAxisLabel = numerical.HeaderNameWithUnit;
            var rows = groupedRows
                .OrderBy(g => (g.Keys[categorical] as Category).Order)
                .Select(grs => new Tuple<Object, Double>(
                    grs.Keys[categorical],
                    numerical.AggregativeFunction.Aggregate(grs.Rows.Select(r => (Double)r.Cells[numerical.Index].Content))
                    ))
                .Take(LineChartMaximumPointNumberInASeries)
                .ToList();
                

            pageView.LineChart.Data = new List<Series>() { new Series(numerical.AggregatedName, rows) };
            if (rows.Count > LineChartMaximumPointNumberInASeries) IsLineChartWarningVisible = true;
            
            pageView.LineChart.Update();
        }

        void DrawLineChart(ColumnViewModel categorical1, ColumnViewModel categorical2, ColumnViewModel numerical, List<GroupedRows> groupedRows, Boolean isTitleEditable)
        {
            Debug.Assert(categorical1.Type == ColumnType.Categorical);
            Debug.Assert(categorical2.Type == ColumnType.Categorical);
            Debug.Assert(numerical.Type == ColumnType.Numerical);
            // 그룹 라인 차트를 그린다.
            IsLineChartVisible = true;

            pageView.LineChartTitle.Children.Clear();
            if (isTitleEditable)
            {
                DrawEditableTitleCCN(pageView.LineChartTitle, categorical1, categorical2, numerical);
            }
            else
            {
                AddText(pageView.LineChartTitle, $"<b>{numerical.HeaderName}</b> by <b>{categorical1.Name}</b> and <b>{categorical2.Name}</b>");
            }
            
            LineChartRowSelecter = series => (r => r.Cells[categorical2.Index].Content.ToString() == series.Item1);

            pageView.LineChart.YStartsWithZero = false;
            pageView.LineChart.HorizontalAxisLabel = categorical1.Name;
            pageView.LineChart.VerticalAxisLabel = numerical.HeaderNameWithUnit;
            var rows =
                groupedRows
                .GroupBy(grs => grs.Keys[categorical2]) // 먼저 묶고 
                .Select(group =>
                    new Series(
                        group.Key.ToString(),
                        group
                            .GroupBy(g => g.Keys[categorical1])
                            .Select(g =>
                                new DataPoint(
                                    g.Key,
                                    numerical.AggregativeFunction.Aggregate(
                                        g
                                        .SelectMany(grs => grs.Rows)
                                        .Select(r => (Double)r.Cells[numerical.Index].Content)
                                    )
                                )
                        )
                        .Take(LineChartMaximumPointNumberInASeries)
                        .ToList()
                    )
                )
                .Take(LineChartMaximumSeriesNumber);

            if (groupedRows.GroupBy(grs => grs.Keys[categorical2]).Count() > LineChartMaximumSeriesNumber) IsLineChartWarningVisible = true;
            if (rows.Max(row => row.Item2.Count) > LineChartMaximumPointNumberInASeries) IsLineChartWarningVisible = true;

            pageView.LineChart.Data = rows;
            pageView.LineChart.Update();
        }

        void DrawScatterplot(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2, Boolean isTitleEditable)
        {
            IsScatterplotVisible = true;

            pageView.ScatterplotTitle.Children.Clear();
            if(isTitleEditable)
            {
                DrawEditableTitleCNvsN(pageView.ScatterplotTitle, categorical, numerical1, numerical2);
            }
            else
            {
                AddText(pageView.ScatterplotTitle, $"<b>{numerical1.Name}</b> vs. <b>{numerical2.Name}</b> colored by <b>{categorical.Name}</b>");
            }
            
            ScatterplotRowSelecter = c => (r => r.Cells[categorical.Index].Content == c);

            pageView.Scatterplot.LegendVisibility = Visibility.Visible;
            pageView.Scatterplot.HorizontalAxisLabel = numerical1.Name + numerical1.UnitString;
            pageView.Scatterplot.VerticalAxisLabel = numerical2.Name + numerical2.UnitString;
            pageView.Scatterplot.Data = mainPageViewModel.SheetViewModel.Sheet.Rows
                .Select(r => new Tuple<Object, Double, Double, Int32>(
                    r.Cells[categorical.Index].Content,
                    (Double)r.Cells[numerical1.Index].Content,
                    (Double)r.Cells[numerical2.Index].Content,
                    r.Index
                    ));

            pageView.Scatterplot.Update();
        }

        void DrawScatterplot(ColumnViewModel numerical1, ColumnViewModel numerical2, Boolean isTitleEditable)
        {
            IsScatterplotVisible = true;

            pageView.ScatterplotTitle.Children.Clear();
            if (isTitleEditable)
            {
                DrawEditableTitleNvsN(pageView.ScatterplotTitle, numerical1, numerical2);
            }
            else
            {
                AddText(pageView.ScatterplotTitle, $"<b>{numerical1.Name}</b> vs. <b>{numerical2.Name}</b>");
            }

            ScatterplotRowSelecter = c => (r => true);

            pageView.Scatterplot.LegendVisibility = Visibility.Collapsed;
            pageView.Scatterplot.HorizontalAxisLabel = numerical1.Name + numerical1.UnitString;
            pageView.Scatterplot.VerticalAxisLabel = numerical2.Name + numerical2.UnitString;
            pageView.Scatterplot.Data = mainPageViewModel.SheetViewModel.Sheet.Rows
                .Select(r => new Tuple<Object, Double, Double, Int32>(0, (Double)r.Cells[numerical1.Index].Content, (Double)r.Cells[numerical2.Index].Content, r.Index));

            pageView.Scatterplot.Update();
        }       

        void DrawGroupedBarChartCNN(ColumnViewModel categorical, ColumnViewModel numerical1, ColumnViewModel numerical2, List<GroupedRows> groupedRows, Boolean isTitleEditable)
        {
            IsGroupedBarChartVisible = true;

            pageView.GroupedBarChartTitle.Children.Clear();
            if (isTitleEditable)
            {
                DrawEditableTitleCNN(pageView.GroupedBarChartTitle, categorical, numerical1, numerical2);
            }
            else
            {
                AddText(pageView.GroupedBarChartTitle, $"<b>{numerical1.HeaderName}</b> and <b>{numerical2.HeaderName}</b> by <b>{categorical.Name}</b>");
            }

            GroupedBarChartRowSelecter = (c1, c2) => (r => r.Cells[categorical.Index].Content == c1);

            pageView.GroupedBarChart.YStartsWithZero = true;
            pageView.GroupedBarChart.HorizontalAxisLabel = categorical.Name;
            pageView.GroupedBarChart.VerticalAxisLabel = $"{numerical1.Name} and {numerical2.Name} {numerical1.UnitString}";
            
            pageView.GroupedBarChart.Data = groupedRows
                        .OrderBy(g => (g.Keys[categorical] as Category).Order)
                        .SelectMany(g => new List<Tuple<Object, Object, Double>>() {
                            new Tuple<Object, Object, Double>(
                                g.Keys[categorical],
                                numerical1.Name,
                                numerical1.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical1.Index].Content))
                            ),
                            new Tuple<Object, Object, Double>(
                                g.Keys[categorical],
                                numerical2.Name,
                                numerical2.AggregativeFunction.Aggregate(g.Rows.Select(r => (Double)r.Cells[numerical2.Index].Content))
                            )
                        })
                        .Take(GroupedBarChartMaximumRecordNumber)
                        ;


            if (pageView.GroupedBarChart.Data.Count() > GroupedBarChartMaximumRecordNumber) IsGroupedBarChartWarningVisible = true;

            pageView.GroupedBarChart.Update();
        }

        void DrawCorrelatonStatistics(ColumnViewModel numerical1, ColumnViewModel numerical2, Boolean isTitleEditable)
        {
            CorrelationStatisticsResult result = CorrelationStatistics.Analyze(
                numerical1.Name,
                numerical2.Name,
                mainPageViewModel.SheetViewModel.Sheet.Rows.Select(r => (Double)r.Cells[numerical1.Index].Content),
                mainPageViewModel.SheetViewModel.Sheet.Rows.Select(r => (Double)r.Cells[numerical2.Index].Content)
                );

            pageView.CorrelationStatisticsTitle.Children.Clear();
            if (isTitleEditable)
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
