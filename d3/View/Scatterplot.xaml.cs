﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using d3.ViewModel;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace d3.View
{
    public sealed partial class Scatterplot : UserControl
    {
        ViewModel.ScatterplotViewModel viewModel = new ViewModel.ScatterplotViewModel();

        public IEnumerable<Tuple<Object, Double, Double>> Data
        {
            set
            {
                viewModel.Data = value;
            }
        }

        public static readonly DependencyProperty LegendVisibilityProperty =
            DependencyProperty.Register("LegendVisibility", typeof(Visibility), typeof(BarChart), new PropertyMetadata(Visibility.Visible));

        public Visibility LegendVisibility
        {
            get { return (Visibility)GetValue(LegendVisibilityProperty); }
            set { SetValue(LegendVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisVisibilityProperty =
            DependencyProperty.Register("HorizontalAxisVisibility", typeof(Visibility), typeof(BarChart), new PropertyMetadata(Visibility.Visible));

        public Visibility HorizontalAxisVisibility
        {
            get { return (Visibility)GetValue(HorizontalAxisVisibilityProperty); }
            set { SetValue(HorizontalAxisVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisLabelProperty =
            DependencyProperty.Register("HorizontalAxisLabel", typeof(String), typeof(BarChart), new PropertyMetadata(String.Empty));

        public String HorizontalAxisLabel
        {
            get { return (String)GetValue(HorizontalAxisLabelProperty); }
            set { SetValue(HorizontalAxisLabelProperty, value); }
        }

        public static readonly DependencyProperty VerticalAxisLabelProperty =
            DependencyProperty.Register("VeritcalAxisLabel", typeof(String), typeof(BarChart), new PropertyMetadata(String.Empty));

        public String VerticalAxisLabel
        {
            get { return (String)GetValue(VerticalAxisLabelProperty); }
            set { SetValue(VerticalAxisLabelProperty, value); }
        }

        public Scatterplot()
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        public void Update()
        {
            viewModel.HorizontalAxisVisibility = HorizontalAxisVisibility;
            viewModel.LegendVisibility = LegendVisibility;
            viewModel.HorizontalAxisLabel = HorizontalAxisLabel;
            viewModel.VerticalAxisLabel = VerticalAxisLabel;

            viewModel.Width = this.Width;
            viewModel.Height = this.Height;

            Double legendAreaWidth = 0;
            if (viewModel.IsLegendVisible)
            {
                viewModel.UpdateLegendData();
                LegendRectangleElement.Update();
                LegendTextElement.Update();

                legendAreaWidth = LegendTextElement.MaxActualWidth + ScatterplotViewModel.LegendPatchWidth +
                    ScatterplotViewModel.LegendPatchSpace + ScatterplotViewModel.PaddingRight;
            }

            Canvas.SetLeft(LegendPanel, this.Width - legendAreaWidth);

            viewModel.LegendAreaWidth = legendAreaWidth;

            viewModel.Update();

            CircleElement.Update();
            HorizontalAxis.Update();
            VerticalAxis.Update();
        }
    }
}
