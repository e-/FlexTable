using System;
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
        
        //public static readonly DependencyProperty BarPointerPressedProperty = DependencyProperty.Register("BarPointerPressed", typeof(d3.Event.EventHandler), typeof(BarChart), new PropertyMetadata(null));

        /*public d3.Event.EventHandler BarPointerPressed
        {
            get { return (d3.Event.EventHandler)GetValue(BarPointerPressedProperty); }
            set { SetValue(BarPointerPressedProperty, value); }
        }

        public static readonly DependencyProperty BarPointerReleasedProperty = DependencyProperty.Register("BarPointerReleased", typeof(d3.Event.EventHandler), typeof(BarChart), new PropertyMetadata(null));

        public d3.Event.EventHandler BarPointerReleased
        {
            get { return (d3.Event.EventHandler)GetValue(BarPointerReleasedProperty); }
            set { SetValue(BarPointerReleasedProperty, value); }
        }*/

        public event d3.Event.EventHandler BarPointerPressed;
        public event d3.Event.EventHandler BarPointerReleased;

        public Scatterplot()
        {
            this.InitializeComponent();
            this.DataContext = viewModel;

            /*RectangleElement.RectanglePointerPressed += RectangleElement_RectanglePointerPressed;
            RectangleElement.RectanglePointerReleased += RectangleElement_RectanglePointerReleased;*/
        }

        void RectangleElement_RectanglePointerPressed(object sender, object datum)
        {
            if (BarPointerPressed != null)
                BarPointerPressed(sender, datum);
        }

        void RectangleElement_RectanglePointerReleased(object sender, object datum)
        {
            if (BarPointerReleased != null)
                BarPointerReleased(sender, datum);
        }

        public void Update()
        {
            viewModel.LegendVisibility = LegendVisibility;
            viewModel.Width = this.Width;
            viewModel.Height = this.Height;
            viewModel.Update();

            CircleElement.Update();
            HorizontalAxis.Update();
            VerticalAxis.Update();

            LegendRectangleElement.Update();
            LegendTextElement.Update();

//            Canvas.SetLeft(LegendPanel, 50 - LegendTextElement.MaxActualWidth / 2);
        }
    }
}
