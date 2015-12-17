using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace d3.View
{
    public sealed partial class BoxPlot : UserControl
    {
        ViewModel.BoxPlotViewModel viewModel = new ViewModel.BoxPlotViewModel();

        public static readonly DependencyProperty MinProperty =
          DependencyProperty.Register("Min", typeof(Double), typeof(BoxPlot), new PropertyMetadata(null, new PropertyChangedCallback(PropertyChanged)));

        public Double Min
        {
            get { return (Double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public static readonly DependencyProperty MaxProperty =
          DependencyProperty.Register("Max", typeof(Double), typeof(BoxPlot), new PropertyMetadata(null, new PropertyChangedCallback(PropertyChanged)));

        public Double Max
        {
            get { return (Double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public static readonly DependencyProperty FisrtQuartileProperty =
          DependencyProperty.Register("FirstQuartile", typeof(Double), typeof(BoxPlot), new PropertyMetadata(null, new PropertyChangedCallback(PropertyChanged)));

        public Double FirstQuartile
        {
            get { return (Double)GetValue(FisrtQuartileProperty); }
            set { SetValue(FisrtQuartileProperty, value); }
        }

        public static readonly DependencyProperty ThirdQuartileProperty =
          DependencyProperty.Register("ThirdQuartile", typeof(Double), typeof(BoxPlot), new PropertyMetadata(null, new PropertyChangedCallback(PropertyChanged)));

        public Double ThirdQuartile
        {
            get { return (Double)GetValue(ThirdQuartileProperty); }
            set { SetValue(ThirdQuartileProperty, value); }
        }

        public static readonly DependencyProperty MedianProperty =
          DependencyProperty.Register("Median", typeof(Double), typeof(BoxPlot), new PropertyMetadata(null, new PropertyChangedCallback(PropertyChanged)));

        public Double Median
        {
            get { return (Double)GetValue(MedianProperty); }
            set { SetValue(MedianProperty, value); }
        }

        public static readonly DependencyProperty MeanProperty =
          DependencyProperty.Register("Mean", typeof(Double), typeof(BoxPlot), new PropertyMetadata(null, new PropertyChangedCallback(PropertyChanged)));

        public Double Mean
        {
            get { return (Double)GetValue(MeanProperty); }
            set { SetValue(MeanProperty, value); }
        }      

        private static void PropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            BoxPlot boxPlot = source as BoxPlot;
            //boxPlot.Update();
        }

        public ViewModel.BoxPlotViewModel BoxPlotViewModel { get { return viewModel; } }

        public BoxPlot()
        {
            this.DataContext = viewModel;
            this.InitializeComponent();
        }

        public void Update()
        {
            viewModel.Min = Min;
            viewModel.Max = Max;
            viewModel.Mean = Mean;
            viewModel.Median = Median;
            viewModel.FirstQuartile = FirstQuartile;
            viewModel.ThirdQuartile = ThirdQuartile;
            viewModel.Width = this.Width;
            viewModel.Update();

            BoxPlotAxis.Scale = viewModel.Scale;

            BoxPlotAxis.Update(true);
            UpdateStoryboard.Pause();
            UpdateStoryboard.Begin();
        }
    }
}
