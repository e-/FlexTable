using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using d3.ViewModel;
using Series = System.Tuple<System.String, System.Collections.Generic.List<System.Tuple<System.Object, System.Double>>>;
using DataPoint = System.Tuple<System.Object, System.Double>;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace d3.View
{
    public sealed partial class LineChart2 : UserControl
    {
        ViewModel.LineChartViewModel viewModel = new ViewModel.LineChartViewModel();

        public IEnumerable<Series> Data
        {
            set
            {
                viewModel.Data = value;
            }
        }

        public static readonly DependencyProperty LegendVisibilityProperty =
            DependencyProperty.Register("LegendVisibility", typeof(Visibility), typeof(LineChart), new PropertyMetadata(Visibility.Visible));

        public Visibility LegendVisibility
        {
            get { return (Visibility)GetValue(LegendVisibilityProperty); }
            set { SetValue(LegendVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisVisibilityProperty =
            DependencyProperty.Register("HorizontalAxisVisibility", typeof(Visibility), typeof(LineChart), new PropertyMetadata(Visibility.Visible));

        public Visibility HorizontalAxisVisibility
        {
            get { return (Visibility)GetValue(HorizontalAxisVisibilityProperty); }
            set { SetValue(HorizontalAxisVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisLabelProperty =
            DependencyProperty.Register("HorizontalAxisLabel", typeof(String), typeof(LineChart), new PropertyMetadata(String.Empty));

        public String HorizontalAxisLabel
        {
            get { return (String)GetValue(HorizontalAxisLabelProperty); }
            set { SetValue(HorizontalAxisLabelProperty, value); }
        }

        public static readonly DependencyProperty VerticalAxisLabelProperty =
            DependencyProperty.Register("VeritcalAxisLabel", typeof(String), typeof(LineChart), new PropertyMetadata(String.Empty));

        public String VerticalAxisLabel
        {
            get { return (String)GetValue(VerticalAxisLabelProperty); }
            set { SetValue(VerticalAxisLabelProperty, value); }
        }

        public static readonly DependencyProperty AutoColorProperty =
            DependencyProperty.Register("AutoColor", typeof(Boolean), typeof(LineChart), new PropertyMetadata(true));

        public Boolean AutoColor
        {
            get { return (Boolean)GetValue(AutoColorProperty); }
            set { SetValue(AutoColorProperty, value); }
        }

        public static readonly DependencyProperty LinePointerPressedProperty = DependencyProperty.Register("LinePointerPressed", typeof(Event.EventHandler), typeof(LineChart), new PropertyMetadata(null));
        
        public event Event.EventHandler LinePointerPressed;
        public event Event.EventHandler LinePointerReleased;

        public static readonly DependencyProperty YStartsWithZeroProperty =
            DependencyProperty.Register("YStartsWithZero", typeof(Boolean), typeof(LineChart), new PropertyMetadata(false));

        public bool YStartsWithZero
        {
            get { return (Boolean)GetValue(YStartsWithZeroProperty); }
            set { SetValue(YStartsWithZeroProperty, value); }
        }

        public LineChart()
        {
            this.InitializeComponent();
            this.DataContext = viewModel;

            LineElement.LinePointerPressed += LineElement_LinePointerPressed;
            LineElement.LinePointerReleased += LineElement_LinePointerReleased;

            LegendHandleRectangleElement.RectanglePointerPressed += LegendHandleRectangleElement_RectanglePointerPressed;
            LegendHandleRectangleElement.RectanglePointerReleased += LegendHandleRectangleElement_RectanglePointerReleased;
        }

        private void LegendHandleRectangleElement_RectanglePointerPressed(object sender, object e, object datum, int index)
        {
            if (LinePointerPressed != null)
                LinePointerPressed(sender, e, datum, index);

            viewModel.SelectLine(index);
            LineElement.Update(true);
            CircleElement.Update(true, false);
            IndicatorTextElement.Update(true);
            if (viewModel.IsLegendVisible)
            {
                LegendRectangleElement.Update(true);
                LegendTextElement.Update(true);
            }
        }

        private void LegendHandleRectangleElement_RectanglePointerReleased(object sender, object e, object datum, int index)
        {
            if (LinePointerReleased != null)
                LinePointerReleased(sender, e, datum, index);

            viewModel.UnselectLine(index);
            LineElement.Update(true);
            CircleElement.Update(true, false);
            IndicatorTextElement.Update(true);
            if (viewModel.IsLegendVisible)
            {
                LegendRectangleElement.Update(true);
                LegendTextElement.Update(true);
            }
        }       

        void LineElement_LinePointerPressed(object sender, object e, object datum, Int32 index)
        {
            if (LinePointerPressed != null)
                LinePointerPressed(sender, e, datum, index);
        }

        void LineElement_LinePointerReleased(object sender, object e, object datum, Int32 index)
        {
            if (LinePointerReleased != null)
                LinePointerReleased(sender, e, datum, index);
        }

        public void Update()
        {
            viewModel.HorizontalAxisVisibility = HorizontalAxisVisibility;
            viewModel.LegendVisibility = LegendVisibility;
            viewModel.HorizontalAxisLabel = HorizontalAxisLabel;
            viewModel.VerticalAxisLabel = VerticalAxisLabel;
            viewModel.AutoColor = AutoColor;
            viewModel.Width = this.Width;
            viewModel.Height = this.Height;
            viewModel.YStartsWithZero = YStartsWithZero;

            //Double 
            Double legendAreaWidth = 0;
            if (viewModel.IsLegendVisible)
            {
                LegendRectangleElement.Data = new Data()
                {
                    List = viewModel.Data.Select(d => d as Object).ToList()
                };
                LegendRectangleElement.Update();

                LegendTextElement.Data = new Data()
                {
                    List = viewModel.Data.Select(d => d as Object).ToList()
                };
                LegendTextElement.Update();

                legendAreaWidth = LegendTextElement.MaxActualWidth + LineChartViewModel.LegendPatchWidth + LineChartViewModel.LegendPatchSpace + LineChartViewModel.PaddingRight;
            }

            Canvas.SetLeft(LegendPanel, this.Width - legendAreaWidth);

            viewModel.LegendAreaWidth = legendAreaWidth;
            viewModel.Update();

            LegendHandleRectangleElement.Update();
            LineElement.Update();
            CircleElement.Update(false, false);
            IndicatorTextElement.Update();
            HorizontalAxis.Update();
            VerticalAxis.Update();
        }
    }
}
