﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using d3;
using d3.ColorScheme;
using d3.Scale;
using FlexTable.Model;
using FlexTable.Util;
using FlexTable.ViewModel;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.Crayon.Chart
{
    public sealed partial class Scatterplot : UserControl
    {
        public IList<ScatterplotDatum> Data { get; set; }
        public Data D3Data { get; set; }

        // 색깔 및 레전드용 데이터 Key 들의 집함
        public IList<Object> LegendData { get; set; }
        public Data D3LegendData { get; set; }

        public static readonly DependencyProperty LegendVisibilityProperty =
            DependencyProperty.Register(nameof(LegendVisibility), typeof(Visibility), typeof(Scatterplot), new PropertyMetadata(Visibility.Visible));

        public Visibility LegendVisibility
        {
            get { return (Visibility)GetValue(LegendVisibilityProperty); }
            set { SetValue(LegendVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisVisibilityProperty =
            DependencyProperty.Register(nameof(HorizontalAxisVisibility), typeof(Visibility), typeof(Scatterplot), new PropertyMetadata(Visibility.Visible));

        public Visibility HorizontalAxisVisibility
        {
            get { return (Visibility)GetValue(HorizontalAxisVisibilityProperty); }
            set { SetValue(HorizontalAxisVisibilityProperty, value); }
        }


        public static readonly DependencyProperty HorizontalAxisTitleProperty = DependencyProperty.Register(nameof(HorizontalAxisTitle), typeof(String), typeof(Scatterplot), new PropertyMetadata(String.Empty));

        public String HorizontalAxisTitle
        {
            get { return (String)GetValue(HorizontalAxisTitleProperty); }
            set { SetValue(HorizontalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty VerticalAxisTitleProperty =
            DependencyProperty.Register(nameof(VerticalAxisTitle), typeof(String), typeof(Scatterplot), new PropertyMetadata(String.Empty));

        public String VerticalAxisTitle
        {
            get { return (String)GetValue(VerticalAxisTitleProperty); }
            set { SetValue(VerticalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty AutoColorProperty =
            DependencyProperty.Register(nameof(AutoColor), typeof(Boolean), typeof(BarChart), new PropertyMetadata(true));

        public Boolean AutoColor
        {
            get { return (Boolean)GetValue(AutoColorProperty); }
            set { SetValue(AutoColorProperty, value); }
        }

        public static readonly DependencyProperty YStartsFromZeroProperty =
            DependencyProperty.Register(nameof(YStartsFromZero), typeof(Boolean), typeof(BarChart), new PropertyMetadata(false));

        public bool YStartsFromZero
        {
            get { return (Boolean)GetValue(YStartsFromZeroProperty); }
            set { SetValue(YStartsFromZeroProperty, value); }
        }

        public Linear XScale { get; set; } = new Linear();
        public Linear YScale { get; set; } = new Linear();

        public Double ChartAreaEndX { get; set; }
        public Double ChartAreaEndY { get; set; } = 300;
        
        public Dictionary<Object, Int32> KeyDictionary { get; set; } = new Dictionary<Object, Int32>();

        public Func<Object, Int32, Double> XGetter { get { return (d, index) => XScale.Map((d as ScatterplotDatum).Value1); } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => YScale.Map((d as ScatterplotDatum).Value2); } }
        
        public Func<Object, Int32, Double> OpacityGetter
        {
            get
            {
                return (d, index) => (selectedRows.Count == 0) ? 0.8 : (selectedRows.Contains((d as ScatterplotDatum).Row) ? 0.8 : 0.1);
            }
        }

        public Func<Object, Int32, Double> LegendHandleWidthGetter { get { return (d, index) => LegendAreaWidth; } }
        public Func<Object, Int32, Double> LegendHandleYGetter
        {
            get
            {
                return
                    (d, index) => (Height - LegendData.Count * Const.LegendPatchHeight - (LegendData.Count - 1) * Const.LegendPatchSpace) / 2 + index * (Const.LegendPatchHeight + Const.LegendPatchSpace) - Const.LegendPatchSpace / 2;
            }
        }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - LegendData.Count * Const.LegendPatchHeight - (LegendData.Count - 1) * Const.LegendPatchSpace) / 2 + index * (Const.LegendPatchHeight + Const.LegendPatchSpace);
            }
        }
        public Func<Object, Int32, Double> LegendPatchOpacityGetter { get {
                return (d, index) => (selectedRows.Count == 0 || categoricalColumnViewModel == null) ? 1 : (selectedRows.Any(dd => dd.Cells[categoricalColumnViewModel.Index].Content == d) ? 1 : 0.2);
            } }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => Const.LegendPatchWidth + Const.LegendPatchSpace; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => d.ToString(); } }
        public Func<TextBlock, Object, Int32, Double> LegendTextOpacityGetter { get {
                return (textBlock, d, index) => (selectedRows.Count == 0 || categoricalColumnViewModel == null) ? 1 : (selectedRows.Any(dd => dd.Cells[categoricalColumnViewModel.Index].Content == d) ? 1 : 0.2); } }

        public Func<Object, Int32, Color> ColorGetter
        {
            get
            {
                return (d, index) => AutoColor ? ((d as ScatterplotDatum).Key as Category).Color : Category10.Colors[0];
            }
        }

        public Func<Object, Int32, Color> LegendColorGetter
        {
            get
            {
                return (d, index) => AutoColor ? (d as Category).Color : Category10.Colors[0];
            }
        }

        public Double HorizontalAxisLabelCanvasTop { get; set; }
        public Double HorizontalAxisLabelCanvasLeft { get; set; }
        public Double HorizontalAxisLabelWidth { get; set; }
        public Double VerticalAxisLabelCanvasTop { get; set; }
        public Double VerticalAxisLabelCanvasLeft { get; set; }
        public Double VerticalAxisLabelHeight { get; set; }
        public Double VerticalAxisCanvasLeft { get; set; }

        public Double LegendAreaWidth { get; set; } = 140;

        public event d3.Event.EventHandler SelectionChanged;
        private List<Row> selectedRows = new List<Row>();
        public List<Row> SelectedRows => selectedRows;
        private ColumnViewModel categoricalColumnViewModel;

        public event d3.Event.EventHandler FilterIn;
        public event d3.Event.EventHandler FilterOut;
        
        Drawable drawable = new Drawable()
        {
            IgnoreSmallStrokes = false
        };

        public Scatterplot()
        {
            this.InitializeComponent();

            CircleElement.Data = D3Data;
            CircleElement.RadiusGetter = d3.Util.CreateConstantGetter<Double>(5);
            CircleElement.XGetter = XGetter;
            CircleElement.YGetter = YGetter;
            CircleElement.ColorGetter = ColorGetter;
            CircleElement.OpacityGetter = OpacityGetter;

            HorizontalAxis.Scale = XScale;
            Canvas.SetTop(HorizontalAxis, ChartAreaEndY);
            HorizontalAxis.Visibility = HorizontalAxisVisibility;
            //HorizontalAxis.LabelFontSizeGetter = LabelFontSizeGetter;
            //HorizontalAxis.LabelOpacityGetter = HorizontalAxisLabelOpacityGetter;
            //HorizontalAxis.LabelYGetter = HorizontalAxisLabelYGetter;

            Canvas.SetTop(HorizontalAxisTitleElement, HorizontalAxisLabelCanvasTop);
            Canvas.SetLeft(HorizontalAxisTitleElement, HorizontalAxisLabelCanvasLeft);
            HorizontalAxisTitleElement.Width = HorizontalAxisLabelWidth;
            HorizontalAxisTitleElement.Visibility = HorizontalAxisVisibility;
            HorizontalAxisTitleElement.Text = HorizontalAxisTitle;

            VerticalAxis.Scale = YScale;
            Canvas.SetLeft(VerticalAxis, VerticalAxisCanvasLeft);
            Canvas.SetTop(VerticalAxisTitleElement, VerticalAxisLabelCanvasTop);
            Canvas.SetLeft(VerticalAxisTitleElement, VerticalAxisLabelCanvasLeft);
            VerticalAxisTitleElement.Width = VerticalAxisLabelHeight;
            VerticalAxisTitleElement.Text = VerticalAxisTitle;

            LegendHandleRectangleElement.Data = D3Data;
            LegendHandleRectangleElement.WidthGetter = LegendHandleWidthGetter;
            LegendHandleRectangleElement.HeightGetter = d3.Util.CreateConstantGetter<Double>(Const.LegendPatchHeight + Const.LegendPatchSpace);
            LegendHandleRectangleElement.XGetter = d3.Util.CreateConstantGetter<Double>(0);
            LegendHandleRectangleElement.YGetter = LegendHandleYGetter;
            LegendHandleRectangleElement.ColorGetter = d3.Util.CreateConstantGetter<Color>(Colors.Transparent);
            LegendHandleRectangleElement.Visibility = LegendVisibility;

            LegendRectangleElement.Data = D3Data;
            LegendRectangleElement.WidthGetter = d3.Util.CreateConstantGetter<Double>(Const.LegendPatchWidth);
            LegendRectangleElement.HeightGetter = d3.Util.CreateConstantGetter<Double>(Const.LegendPatchHeight);
            LegendRectangleElement.XGetter = d3.Util.CreateConstantGetter<Double>(0);
            LegendRectangleElement.YGetter = LegendPatchYGetter;
            LegendRectangleElement.ColorGetter = LegendColorGetter;
            LegendRectangleElement.OpacityGetter = LegendPatchOpacityGetter;
            LegendRectangleElement.Visibility = LegendVisibility;

            LegendTextElement.Data = D3Data;
            LegendTextElement.TextGetter = LegendTextGetter;
            LegendTextElement.HeightGetter = d3.Util.CreateConstantGetter<Double>(Const.LegendPatchHeight);
            LegendTextElement.XGetter = LegendTextXGetter;
            LegendTextElement.YGetter = LegendPatchYGetter;
            LegendTextElement.ColorGetter = d3.Util.CreateConstantGetter<Color>(Colors.Black);
            LegendTextElement.OpacityGetter = LegendTextOpacityGetter;
            LegendTextElement.Visibility = LegendVisibility;
                        
            drawable.Attach(RootCanvas, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += Drawable_StrokeAdded;

            LegendHandleRectangleElement.RectangleTapped += LegendHandleRectangleElement_RectangleTapped;
        }

        private void LegendHandleRectangleElement_RectangleTapped(object sender, object e, object datum, int index)
        {
            TappedRoutedEventArgs args = e as TappedRoutedEventArgs;
            if (args.PointerDeviceType == PointerDeviceType.Touch)
            {
                IEnumerable<Row> selected = Data.Where(d => d.Key == datum).Select(d => d.Row);

                if(selected.Except(selectedRows).Count() == 0) // 모두 다이미있으면 지워야함
                {
                    selectedRows = selectedRows.Except(selected).Distinct().ToList();
                }
                else
                {
                    selectedRows = selectedRows.Concat(selected).Distinct().ToList();
                }

                if (selectedRows.Count == Data.Count)
                {
                    selectedRows.Clear();
                }

                if (SelectionChanged != null)
                    SelectionChanged(this, e, selectedRows, index);

                CircleElement.Update(true, false);
                if (LegendVisibility == Visibility.Visible)
                {
                    LegendRectangleElement.Update(TransitionType.All);
                    LegendTextElement.Update(TransitionType.Opacity);
                }
                args.Handled = true;
            }
        }
        
        private void Drawable_StrokeAdded(InkManager inkManager)
        {
            if (inkManager.GetStrokes().Count > 0)
            {
                List<Point> points = inkManager.GetStrokes()[0].GetInkPoints().Select(ip => ip.Position).ToList();
                
                Rect boundingRect = inkManager.GetStrokes()[0].BoundingRect;
                
                Int32 index = 0;

                IEnumerable<Row> intersectedRows = Data
                    .Where(d => d3.Util.TestPointInPolygon(new Point(XGetter(d, 0), YGetter(d, 0)), points))
                    .Select(d => d.Row);

                index = 0;
                foreach (Rectangle rect in LegendHandleRectangleElement.Children)
                {
                    Rect r = new Rect(Canvas.GetLeft(rect) + ChartAreaEndX, Canvas.GetTop(rect), rect.Width, rect.Height);

                    if (Const.IsIntersected(r, boundingRect))
                    {
                        Object datum = LegendData[index];

                        intersectedRows = intersectedRows.Concat(Data.Where(d => d.Key == datum).Select(d => d.Row));
                    }
                    index++;
                }

                if (Const.IsStrikeThrough(boundingRect))
                {
                    if (FilterOut != null)
                    {
                        FilterOut(this, $"Filtered by {Data[0].ColumnViewModel.Name}", intersectedRows.ToList(), index);
                    }
                } 
                else
                {
                    selectedRows = selectedRows.Concat(intersectedRows).Distinct().ToList();
                    if (selectedRows.Count == Data.Count())
                        selectedRows.Clear();

                    if (SelectionChanged != null)
                        SelectionChanged(this, null, selectedRows, index);
                }
                
                CircleElement.Update(true, false);
                if (LegendVisibility == Visibility.Visible)
                {
                    LegendRectangleElement.Update(TransitionType.All);
                    LegendTextElement.Update(TransitionType.Opacity);
                }
            }

            drawable.RemoveAllStrokes();
        }
        
        public void Update()
        {
            categoricalColumnViewModel = Data.First().ColumnViewModel;

            LegendAreaWidth = 0;
            D3Data = new Data()
            {
                List = Data.Select(d => d as Object).ToList()
            };

            LegendData = Data.Select(d => d.Key).Distinct().ToList();
            D3LegendData = new Data() { List = LegendData.Select(d => d as Object).ToList() };

            if (LegendVisibility == Visibility.Visible)
            {
                LegendRectangleElement.Data = D3LegendData;
                LegendRectangleElement.Update(TransitionType.None);

                LegendTextElement.Data = D3LegendData;
                LegendTextElement.Update(TransitionType.None);

                LegendAreaWidth = LegendTextElement.MaxActualWidth + Const.LegendPatchWidth + Const.LegendPatchSpace + Const.PaddingRight;
            }

            Canvas.SetLeft(LegendPanel, this.Width - LegendAreaWidth);

            if (HorizontalAxisVisibility == Visibility.Visible)
            {
                ChartAreaEndY = this.Height - Const.PaddingBottom - Const.HorizontalAxisHeight - Const.HorizontalAxisLabelHeight;
            }
            else
            {
                ChartAreaEndY = this.Height - Const.PaddingBottom;
            }

            if (LegendVisibility == Visibility.Visible)
            {
                ChartAreaEndX = this.Width - Const.PaddingRight - LegendAreaWidth;
            }
            else
            {
                ChartAreaEndX = this.Width - Const.PaddingRight - 20;
            }

            HorizontalAxisLabelCanvasLeft = Const.PaddingLeft + Const.VerticalAxisWidth + Const.VerticalAxisLabelWidth;
            HorizontalAxisLabelCanvasTop = ChartAreaEndY + Const.HorizontalAxisHeight;
            HorizontalAxisLabelWidth = ChartAreaEndX - Const.PaddingLeft - Const.VerticalAxisWidth - Const.VerticalAxisLabelWidth;

            VerticalAxisCanvasLeft = Const.PaddingLeft + Const.VerticalAxisLabelWidth + Const.VerticalAxisWidth;
            VerticalAxisLabelCanvasLeft = Const.PaddingLeft + Const.VerticalAxisLabelWidth / 2 - (ChartAreaEndY - Const.PaddingTop) / 2;
            VerticalAxisLabelCanvasTop = Const.PaddingTop + (ChartAreaEndY - Const.PaddingTop) / 2;
            VerticalAxisLabelHeight = ChartAreaEndY - Const.PaddingTop;            

            XScale = new Linear()
            {
                DomainStart = Data.Select(d => d.Value1).Min() * 0.9,
                DomainEnd = Data.Select(d => d.Value1).Max() * 1.1,
                RangeStart = VerticalAxisCanvasLeft,
                RangeEnd = ChartAreaEndX + Const.PaddingLeft
            };
            XScale.Nice();

            YScale = new Linear()
            {
                DomainStart = Data.Select(d => d.Value2).Min() * 0.9,
                DomainEnd = Data.Select(d => d.Value2).Max() * 1.1,
                RangeStart = ChartAreaEndY,
                RangeEnd = Const.PaddingTop
            };
            YScale.Nice();

            Boolean useCircleTransition = true;
            if (CircleElement.Data == null || CircleElement.Data.List.Count != D3Data.List.Count) useCircleTransition = false;
            CircleElement.Data = D3Data;

            HorizontalAxis.Scale = XScale;
            Canvas.SetTop(HorizontalAxis, ChartAreaEndY);
            HorizontalAxis.Visibility = HorizontalAxisVisibility;

            Canvas.SetTop(HorizontalAxisTitleElement, HorizontalAxisLabelCanvasTop);
            Canvas.SetLeft(HorizontalAxisTitleElement, HorizontalAxisLabelCanvasLeft);
            HorizontalAxisTitleElement.Width = HorizontalAxisLabelWidth;
            HorizontalAxisTitleElement.Visibility = HorizontalAxisVisibility;
            HorizontalAxisTitleElement.Text = HorizontalAxisTitle;

            VerticalAxis.Scale = YScale;
            Canvas.SetLeft(VerticalAxis, VerticalAxisCanvasLeft);

            Canvas.SetTop(VerticalAxisTitleElement, VerticalAxisLabelCanvasTop);
            Canvas.SetLeft(VerticalAxisTitleElement, VerticalAxisLabelCanvasLeft);
            VerticalAxisTitleElement.Width = VerticalAxisLabelHeight;
            VerticalAxisTitleElement.Text = VerticalAxisTitle;

            LegendHandleRectangleElement.Data = D3LegendData;
            LegendHandleRectangleElement.Visibility = LegendVisibility;

            LegendRectangleElement.Data = D3LegendData;
            LegendRectangleElement.Visibility = LegendVisibility;

            LegendTextElement.Data = D3LegendData;
            LegendTextElement.Visibility = LegendVisibility;

            LegendHandleRectangleElement.Update(TransitionType.None);
            CircleElement.Update(useCircleTransition, true);
            HorizontalAxis.Update();
            VerticalAxis.Update();
        }

        public void ClearSelection(Boolean withHandler)
        {
            selectedRows.Clear();

            if (withHandler && SelectionChanged != null)
                SelectionChanged(this, null, new List<Row>(), 0);

            CircleElement.Update(true, false);
            if (LegendVisibility == Visibility.Visible)
            {
                LegendRectangleElement.Update(TransitionType.All);
                LegendTextElement.Update(TransitionType.Opacity);
            }
        }

        public void ImportSelection(IEnumerable<Row> importedRows)
        {
            selectedRows = importedRows.ToList();

            CircleElement.Update(false, false);
            if (LegendVisibility == Visibility.Visible)
            {
                LegendRectangleElement.Update(TransitionType.None);
                LegendTextElement.Update(TransitionType.None);
            }
        }
    }
}
