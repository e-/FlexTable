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
using d3.ViewModel;
using DataPoint = System.Tuple<System.Object, System.Double, System.Double, System.Int32>;
using LegendDataPoint = System.Tuple<System.Object, System.Int32>;
using Windows.UI.Input.Inking;
using System.Diagnostics;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace d3.View
{
    public sealed partial class Scatterplot : UserControl
    {
        ViewModel.ScatterplotViewModel viewModel = new ViewModel.ScatterplotViewModel();

        public IEnumerable<DataPoint> Data
        {
            set
            {
                viewModel.Data = value;
            }
        }

        public static readonly DependencyProperty LegendVisibilityProperty =
            DependencyProperty.Register("LegendVisibility", typeof(Visibility), typeof(Scatterplot), new PropertyMetadata(Visibility.Visible));

        public Visibility LegendVisibility
        {
            get { return (Visibility)GetValue(LegendVisibilityProperty); }
            set { SetValue(LegendVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisVisibilityProperty =
            DependencyProperty.Register("HorizontalAxisVisibility", typeof(Visibility), typeof(Scatterplot), new PropertyMetadata(Visibility.Visible));

        public Visibility HorizontalAxisVisibility
        {
            get { return (Visibility)GetValue(HorizontalAxisVisibilityProperty); }
            set { SetValue(HorizontalAxisVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisLabelProperty =
            DependencyProperty.Register("HorizontalAxisLabel", typeof(String), typeof(Scatterplot), new PropertyMetadata(String.Empty));

        public String HorizontalAxisLabel
        {
            get { return (String)GetValue(HorizontalAxisLabelProperty); }
            set { SetValue(HorizontalAxisLabelProperty, value); }
        }

        public static readonly DependencyProperty VerticalAxisLabelProperty =
            DependencyProperty.Register("VeritcalAxisLabel", typeof(String), typeof(Scatterplot), new PropertyMetadata(String.Empty));

        public String VerticalAxisLabel
        {
            get { return (String)GetValue(VerticalAxisLabelProperty); }
            set { SetValue(VerticalAxisLabelProperty, value); }
        }

        public event Event.EventHandler CategoryPointerPressed;
        public event Event.EventHandler CategoryPointerReleased;
        public event Event.EventHandler LassoSelected;
        public event Event.EventHandler LassoUnselected;

        Drawable drawable = new Drawable()
        {
            IgnoreSmallStrokes = false
        };

        public Scatterplot()
        {
            this.InitializeComponent();
            this.DataContext = viewModel;

            drawable.Attach(RootCanvas, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += Drawable_StrokeAdded;
            LegendHandleRectangleElement.RectanglePointerPressed += LegendHandleRectangleElement_RectanglePointerPressed;
            LegendHandleRectangleElement.RectanglePointerReleased += LegendHandleRectangleElement_RectanglePointerReleased;
        }

        private void Drawable_StrokeAdded(InkManager inkManager)
        {
            List<Point> points = inkManager.GetStrokes()[0].GetInkPoints().Select(ip => ip.Position).ToList();
            Boolean isLassoSelecting = viewModel.IsLassoSelecting;

            viewModel.SelectLasso(points);

            if (viewModel.SelectedIndices.Count == 0)
            {
                viewModel.UnselectLasso();
                if (isLassoSelecting)
                {
                    if (LassoUnselected != null) LassoUnselected(null, null, null, 0);
                }
            }
            else
            {
                if (LassoSelected != null) LassoSelected(null, null, viewModel.SelectedIndices, 0);
            }
            
            CircleElement.Update(true, false);

            drawable.RemoveAllStrokes();
        }

        private void LegendHandleRectangleElement_RectanglePointerPressed(object sender, object e, object datum, int index)
        {

            if (viewModel.IsLassoSelecting)
            {
                viewModel.UnselectLasso();
                if (LassoUnselected != null) LassoUnselected(null, e, null, 0);
            }

            viewModel.SelectCategory((datum as LegendDataPoint).Item1);
            CircleElement.Update(true, false);
            if (viewModel.IsLegendVisible)
            {
                LegendRectangleElement.Update(true);
                LegendTextElement.Update(true);
            }

            if (CategoryPointerPressed != null)
                CategoryPointerPressed(sender, e, datum, index);
        }

        private void LegendHandleRectangleElement_RectanglePointerReleased(object sender, object e, object datum, int index)
        {
            viewModel.UnselectCategory((datum as LegendDataPoint).Item1);
            CircleElement.Update(true, false);
            if (viewModel.IsLegendVisible)
            {
                LegendRectangleElement.Update(true);
                LegendTextElement.Update(true);
            }

            if (CategoryPointerReleased != null)
                CategoryPointerReleased(sender, e, datum, index);

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

            LegendHandleRectangleElement.Update();
            CircleElement.Update(true, true);
            HorizontalAxis.Update();
            VerticalAxis.Update();
        }
    }
}
