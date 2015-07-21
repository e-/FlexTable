﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d3;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using FlexTable.Model;
using Windows.UI.Xaml.Media.Animation;

namespace FlexTable.ViewModel
{
    public class ExplorationViewModel : NotifyViewModel
    {
        private ColumnViewModel columnViewModel;
        public ColumnViewModel ColumnViewModel { get { return columnViewModel; } set { columnViewModel = value; OnPropertyChanged("ColumnViewModel"); } }

        private DescriptiveStatisticsResult boxPlotViewModel;
        public DescriptiveStatisticsResult BoxPlotViewModel { get { return boxPlotViewModel; } set { boxPlotViewModel = value; OnPropertyChanged("BoxPlotViewModel"); } }

        MainPageViewModel mainPageViewModel;
        public MainPageViewModel MainPageViewModel { get { return mainPageViewModel; } }

        private Boolean isSummaryVisible = false;
        public Boolean IsSummaryVisible { get { return isSummaryVisible; } set { isSummaryVisible = value; OnPropertyChanged("IsSummaryVisible"); } }

        private Boolean isCategoricalColumn;
        public Boolean IsCategoricalColumn { get { return isCategoricalColumn; } set { isCategoricalColumn = value; OnPropertyChanged("IsCategoricalColumn"); } }

        private Boolean isNumericalColumn;
        public Boolean IsNumericalColumn { get { return isNumericalColumn; } set { isNumericalColumn = value; OnPropertyChanged("IsNumericalColumn"); } }

        IMainPage view;

        public ExplorationViewModel(MainPageViewModel mainPageViewModel, IMainPage view)
        {
            this.mainPageViewModel = mainPageViewModel;
            this.view = view;
        }

        public void ShowSummary(ColumnViewModel columnViewModel)
        {
            PageViewModel pageViewModel = view.ExplorationView.TopPageView.PageViewModel;
            ColumnViewModel = columnViewModel;

            pageViewModel.ShowSummary(columnViewModel);
        }

        public void Hide()
        {
            IsSummaryVisible = false;
            ColumnViewModel = null;
        }

        public void DummyGroup()
        {
            if (columnViewModel != null && columnViewModel.Type == ColumnType.Categorical)
            {
                View.PageView page = view.ExplorationView.TopPageView;

                Storyboard sb = new Storyboard()
                {
                };

                DoubleAnimation ani = new DoubleAnimation()
                {
                    To = 530,
                    EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut },
                    Duration = TimeSpan.FromMilliseconds(500),
                };

                Storyboard.SetTarget(ani, page);
                Storyboard.SetTargetProperty(ani, "(Canvas.Top)");

                sb.Children.Add(ani);
                sb.Begin();

                view.ExplorationView.AddNewPage();
            }
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
    }
}
