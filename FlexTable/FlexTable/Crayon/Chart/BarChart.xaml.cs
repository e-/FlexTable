using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using d3;
using d3.Scale;
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
using d3.ColorScheme;
using FlexTable.Util;
using Windows.UI.Input.Inking;
using Windows.Devices.Input;
using Windows.UI.Xaml.Shapes;
using FlexTable.Model;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.Crayon.Chart
{
    public sealed partial class BarChart : UserControl
    {       
        public IList<BarChartDatum> Data { get; set; }
        public Data D3Data { get; set; }                   

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

        public static readonly DependencyProperty HorizontalAxisTitleProperty =
            DependencyProperty.Register(nameof(HorizontalAxisTitle), typeof(String), typeof(BarChart), new PropertyMetadata(String.Empty));

        public String HorizontalAxisTitle
        {
            get { return (String)GetValue(HorizontalAxisTitleProperty); }
            set { SetValue(HorizontalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty VerticalAxisTitleProperty =
            DependencyProperty.Register(nameof(VerticalAxisTitle), typeof(String), typeof(BarChart), new PropertyMetadata(String.Empty));

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

        public Ordinal XScale { get; set; } = new Ordinal();
        public Linear YScale { get; set; } = new Linear();

        public Double ChartAreaEndX { get; set; }
        public Double ChartAreaEndY { get; set; } = 300;

        #region Attribute Getter
        private Double BarWidth { get { return Math.Min(60, XScale.RangeBand / 2); } }
        public Func<Object, Int32, Double> WidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => ChartAreaEndY - YScale.Map((d as BarChartDatum).Value); } }
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => XScale.Map(d) - BarWidth / 2; } }
        public Func<Object, Int32, Double> YGetter
        {
            get
            {
                return (d, index) =>
                {
                    BarChartDatum datum = d as BarChartDatum;
                    return YScale.Map(datum.Value) +
                        (selectedRows.Count > 0 ?
                         (selectedRows.Any(sr => datum.Rows.Contains(sr)) ? DragToFilterYDelta : 0) :
                         (d == DragToFilterFocusedBar ? DragToFilterYDelta : 0));
                };
            }
        }
        public Func<Object, Int32, Color> ColorGetter { get { return (d, index) => (AutoColor ? ((d as BarChartDatum).Key as Category).Color: Category10.Colors.First()); } }
        public Func<Object, Int32, Double> OpacityGetter { get {
                return (d, index) =>
                {
                    BarChartDatum datum = d as BarChartDatum;
                    return (selectedRows.Count > 0 ?
                        (selectedRows.Any(sr => datum.Rows.Contains(sr)) ? DragToFilterOpacity : 0.2) :
                        (d == DragToFilterFocusedBar ? DragToFilterOpacity : 1));
                };
            } }

        /*public Func<Object, Int32, Double> ForegroundHeightGetter { get { return (d, index) => ChartAreaEndY - YScale.Map((d as BarChartDatum).Value); } }
        public Func<Object, Int32, Double> ForegroundYGetter
        {
            get
            {
                return (d, index) => YScale.Map((d as BarChartDatum).Value) +
                (selectedRows.Count > 0 ?
                    (selectedRows.IndexOf((d as BarChartDatum).Key) >= 0 ? DragToFilterYDelta : 0) :
                    (d == DragToFilterFocusedBar ? DragToFilterYDelta : 0));
            }
        }
        public Func<Object, Int32, Double> ForegroundOpacityGetter
        {
            get
            {
                return (d, index) => (selectedRows.Count == 0) ? (d == DragToFilterFocusedBar ? DragToFilterOpacity : 1) :
                (selectedRows.IndexOf((d as BarChartDatum).Key) < 0 ? 0.2 : DragToFilterOpacity);
            }
        }
        */
        public Func<Object, Int32, TextBlock, Double> HorizontalAxisLabelOpacityGetter
        {
            get
            {
                return (d, index, textBlock) => {
                    BarChartDatum datum = d as BarChartDatum;
                    return (selectedRows.Count == 0) ?
                        (d == DragToFilterFocusedBar ? DragToFilterOpacity : 1) :
                        (selectedRows.Any(sr => datum.Rows.Contains(sr)) ? DragToFilterOpacity : 1);
                };
            }
        }

        public Func<Object, Int32, TextBlock, Double> HorizontalAxisLabelYGetter { get {
                return (d, index, textBlock) =>
                {
                    BarChartDatum datum = d as BarChartDatum;
                    return (selectedRows.Count == 0) ?
                        (d == DragToFilterFocusedBar ? DragToFilterYDelta : 0) :
                        (selectedRows.Any(sr => datum.Rows.Contains(sr)) ? DragToFilterYDelta : 0);
                };
            } }
        

        public Func<Object, Int32, Double> HandleWidthGetter { get { return (d, index) => XScale.RangeBand; } }
        public Func<Object, Int32, Double> HandleHeightGetter { get { return (d, index) => ChartAreaEndY - Const.PaddingTop; } }
        public Func<Object, Int32, Double> HandleXGetter { get { return (d, index) => XScale.Map(d) - XScale.RangeBand / 2; } }        

        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - Data.Count() * Const.LegendPatchHeight - (Data.Count() - 1) * Const.LegendPatchSpace) / 2 + index * (Const.LegendPatchHeight + Const.LegendPatchSpace);
            }
        }

        public Func<Object, Int32, Double> LegendHandleWidthGetter { get { return (d, index) => LegendAreaWidth; } }
        public Func<Object, Int32, Double> LegendHandleYGetter
        {
            get
            {
                return (d, index) => (Height - Data.Count() * Const.LegendPatchHeight - (Data.Count() - 1) * Const.LegendPatchSpace) / 2 + index * (Const.LegendPatchHeight + Const.LegendPatchSpace) - Const.LegendPatchSpace / 2;
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => Const.LegendPatchWidth + Const.LegendPatchSpace; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as BarChartDatum).Key.ToString(); } }
        public Func<TextBlock, Object, Int32, Double> LegendTextOpacityGetter { get {
                return (textBlock, d, index) =>
                {
                    BarChartDatum datum = d as BarChartDatum;
                    return (selectedRows.Count == 0) ? 1 : (selectedRows.Any(sr => datum.Rows.Contains(sr)) ? 1 : 0.2);
                };
            } }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => XScale.RangeBand; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => d3.Format.IntegerBalanced.Format((d as BarChartDatum).Value); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) => XScale.Map(d) - XScale.RangeBand / 2; } }
        public Func<Object, Int32, Double> IndicatorYGetter { get {
                return (d, index) =>
                {
                    BarChartDatum datum = d as BarChartDatum;
                    return YScale.Map(datum.Value) - 18 + 
                        ((selectedRows.Count == 0) ? 
                            (d == DragToFilterFocusedBar ? DragToFilterYDelta : 0) :
                            (selectedRows.Any(sr => datum.Rows.Contains(sr)) ? DragToFilterYDelta : 0));
                };
            } }
        public Func<TextBlock, Object, Int32, Double> IndicatorTextOpacityGetter { get {
                return (textBlock, d, index) =>
                {
                    BarChartDatum datum = d as BarChartDatum;
                    return ((selectedRows.Count == 0) ? (d == DragToFilterFocusedBar ? DragToFilterOpacity : 0) :
                    (selectedRows.Any(sr => datum.Rows.Contains(sr)) ? DragToFilterOpacity : 0));
                };
                } }
        
        public Double HorizontalAxisLabelCanvasTop { get; set; }
        public Double HorizontalAxisLabelCanvasLeft { get; set; }
        public Double HorizontalAxisLabelWidth { get; set; }
        public Double VerticalAxisLabelCanvasTop { get; set; }
        public Double VerticalAxisLabelCanvasLeft { get; set; }
        public Double VerticalAxisLabelHeight { get; set; }
        public Double VerticalAxisCanvasLeft { get; set; }

        public Double LegendAreaWidth { get; set; } = 140;

        private Double DragToFilterYDelta = 0;
        private Double DragToFilterOpacity = 1;
        private BarChartDatum DragToFilterFocusedBar = null;
        
        public event Event.EventHandler SelectionChanged;
        private List<Row> selectedRows = new List<Row>();

        public event Event.EventHandler FilterIn;
        public event Event.EventHandler FilterOut;

        #endregion

        Drawable drawable = new Drawable()
        {
            //IgnoreSmallStrokes = true
        };

        public BarChart()
        {
            this.InitializeComponent();

            // getter가 아닌 경우 재대입을 해야함 예를 들어 visibliity나 Scale등

            HandleRectangleElement.Data = D3Data;
            HandleRectangleElement.WidthGetter = HandleWidthGetter;
            HandleRectangleElement.HeightGetter = HandleHeightGetter;
            HandleRectangleElement.XGetter = HandleXGetter;
            HandleRectangleElement.YGetter = d3.Util.CreateConstantGetter<Double>(Const.PaddingTop);
            HandleRectangleElement.ColorGetter = d3.Util.CreateConstantGetter<Color>(Colors.Transparent);

            RectangleElement.Data = D3Data;
            RectangleElement.WidthGetter = WidthGetter;
            RectangleElement.HeightGetter = HeightGetter;
            RectangleElement.XGetter = XGetter;
            RectangleElement.YGetter = YGetter;
            RectangleElement.ColorGetter = ColorGetter;
            RectangleElement.OpacityGetter = OpacityGetter;

            /*ForegroundRectangleElement.Data = D3Data;
            ForegroundRectangleElement.WidthGetter = WidthGetter;
            ForegroundRectangleElement.HeightGetter = ForegroundHeightGetter;
            ForegroundRectangleElement.XGetter = XGetter;
            ForegroundRectangleElement.YGetter = ForegroundYGetter;
            ForegroundRectangleElement.ColorGetter = ColorGetter;
            ForegroundRectangleElement.OpacityGetter = ForegroundOpacityGetter;*/

            HorizontalAxis.Scale = XScale;
            Canvas.SetTop(HorizontalAxis, ChartAreaEndY);
            HorizontalAxis.Visibility = HorizontalAxisVisibility;
            //HorizontalAxis.LabelFontSizeGetter = LabelFontSizeGetter;
            HorizontalAxis.LabelOpacityGetter = HorizontalAxisLabelOpacityGetter;
            HorizontalAxis.LabelYGetter = HorizontalAxisLabelYGetter;

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
            LegendRectangleElement.ColorGetter = ColorGetter;
            LegendRectangleElement.OpacityGetter = OpacityGetter;
            LegendRectangleElement.Visibility = LegendVisibility;

            LegendTextElement.Data = D3Data;
            LegendTextElement.TextGetter = LegendTextGetter;
            LegendTextElement.HeightGetter = d3.Util.CreateConstantGetter<Double>(Const.LegendPatchHeight);
            LegendTextElement.XGetter = LegendTextXGetter;
            LegendTextElement.YGetter = LegendPatchYGetter;
            LegendTextElement.ColorGetter = d3.Util.CreateConstantGetter<Color>(Colors.Black);
            LegendTextElement.OpacityGetter = LegendTextOpacityGetter;
            LegendTextElement.Visibility = LegendVisibility;

            IndicatorTextElement.Data = D3Data;
            IndicatorTextElement.TextGetter = IndicatorTextGetter;
            IndicatorTextElement.WidthGetter = IndicatorWidthGetter;
            IndicatorTextElement.XGetter = IndicatorXGetter;
            IndicatorTextElement.YGetter = IndicatorYGetter;
            IndicatorTextElement.OpacityGetter = IndicatorTextOpacityGetter;

            HandleRectangleElement.RectangleTapped += RectangleElement_RectangleTapped;
            LegendHandleRectangleElement.RectangleTapped += RectangleElement_RectangleTapped;
            HandleRectangleElement.RectangleManipulationDelta += HandleRectangleElement_RectangleManipulationDelta;
            HandleRectangleElement.RectangleManipulationCompleted += HandleRectangleElement_RectangleManipulationCompleted;
            drawable.Attach(RootCanvas, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += Drawable_StrokeAdded;
        }

        private void HandleRectangleElement_RectangleManipulationCompleted(object sender, object eo, object datumo, int index)
        {
            ManipulationCompletedRoutedEventArgs e = eo as ManipulationCompletedRoutedEventArgs;
            if (e.PointerDeviceType != PointerDeviceType.Touch) return;
            Double delta = e.Cumulative.Translation.Y;
            BarChartDatum datum = datumo as BarChartDatum;

            if (delta > Const.DragToFilterThreshold)
            {
                // filter out

                if (FilterOut != null)
                {
                    if (selectedRows.Count > 0)
                    {
                        FilterOut(sender, eo, selectedRows, index);
                    }
                    else
                    {
                        FilterOut(sender, eo, datum.Rows, index);
                    }
                }

                ClearSelection(true);
            }

            DragToFilterYDelta = 0;
            DragToFilterFocusedBar = null;
            DragToFilterOpacity = 1;

            RectangleElement.Update();
            //ForegroundRectangleElement.Update();
            HorizontalAxis.Update();
            IndicatorTextElement.Update();
        }
        
        private void HandleRectangleElement_RectangleManipulationDelta(object sender, object eo, object datumo, int index)
        {
            ManipulationDeltaRoutedEventArgs e = eo as ManipulationDeltaRoutedEventArgs;
            if (e.PointerDeviceType != PointerDeviceType.Touch) return;
            Double delta = e.Cumulative.Translation.Y;
            BarChartDatum datum = datumo as BarChartDatum;

            if (delta < 0) delta = 0;
            if (delta > Const.DragToFilterThreshold) delta = Const.DragToFilterThreshold;

            DragToFilterYDelta = delta;
            DragToFilterFocusedBar = datum;
            DragToFilterOpacity = 1 - delta / Const.DragToFilterThreshold;

            RectangleElement.Update();
            //ForegroundRectangleElement.Update();
            HorizontalAxis.Update();
            IndicatorTextElement.Update();
        }
       
        private void Drawable_StrokeAdded(InkManager inkManager)
        {
            if (inkManager.GetStrokes().Count > 0)
            {
                List<Point> points = inkManager.GetStrokes()[0].GetInkPoints().Select(ip => ip.Position).ToList();
                Rect boundingRect = inkManager.GetStrokes()[0].BoundingRect;

                // 먼저 boundingRect와 겹친 row들을 모두 찾는다.
                List<Row> intersectedRows = new List<Row>();
                Int32 index = 0;

                index = 0;
                foreach (Rectangle rect in RectangleElement.Children)
                {
                    Rect r = new Rect(Canvas.GetLeft(rect), Canvas.GetTop(rect), rect.Width, rect.Height);

                    if (Const.IsIntersected(r, boundingRect))
                    {
                        BarChartDatum datum = Data[index];
                        intersectedRows = intersectedRows.Concat(datum.Rows).ToList();
                    }
                    index++;
                }


                index = 0;
                foreach (Rectangle rect in LegendHandleRectangleElement.Children)
                {
                    Rect r = new Rect(Canvas.GetLeft(rect) + ChartAreaEndX, Canvas.GetTop(rect), rect.Width, rect.Height);

                    if (Const.IsIntersected(r, boundingRect))
                    {
                        BarChartDatum datum = Data[index];
                        intersectedRows = intersectedRows.Concat(datum.Rows).ToList();
                    }
                    index++;
                }

                index = 0;
                foreach (TextBlock label in HorizontalAxis.TickLabels)
                {
                    Rect r = new Rect(Canvas.GetLeft(label), Canvas.GetTop(label), label.Width, label.Height);

                    if (Const.IsIntersected(r, boundingRect))
                    {
                        BarChartDatum datum = Data[index];
                        intersectedRows = intersectedRows.Concat(datum.Rows).ToList();
                    }
                    index++;
                }

                intersectedRows = intersectedRows.Distinct().ToList();

                if(Const.IsStrikeThrough(boundingRect)) // strikethrough 및 무조건 필터아웃 
                {
                    if(FilterOut != null)
                    {
                        FilterOut(this, null, intersectedRows, index);
                    }
                }
                else // 아니면 무조건 셀렉션 
                {
                    selectedRows = selectedRows.Concat(intersectedRows).Distinct().ToList();
                    if (selectedRows.Count == Data.SelectMany(d => d.Rows).Count())
                        selectedRows.Clear();

                    if (SelectionChanged != null)
                        SelectionChanged(this, null, selectedRows, index);
                }
                
                RectangleElement.Update(true);
                //ForegroundRectangleElement.Update();
                IndicatorTextElement.Update(true);
                if (LegendVisibility == Visibility.Visible)
                {
                    LegendRectangleElement.Update(true);
                    LegendTextElement.Update(true);
                }
            }

            drawable.RemoveAllStrokes();
        }

        void RectangleElement_RectangleTapped(object sender, object e, object datum, Int32 index)
        {
            TappedRoutedEventArgs args = e as TappedRoutedEventArgs;
            if (args.PointerDeviceType == PointerDeviceType.Touch)
            {
                BarChartDatum barChartDatum = datum as BarChartDatum;
                if (selectedRows.Intersect(barChartDatum.Rows).Count() == barChartDatum.Rows.Count()) // contains all
                {
                    selectedRows = selectedRows.Except(barChartDatum.Rows).Distinct().ToList();
                }
                else
                {
                    selectedRows = selectedRows.Concat(barChartDatum.Rows).Distinct().ToList();
                }
                
                if(selectedRows.Count == Data.SelectMany(d => d.Rows).Count())
                {
                    selectedRows.Clear();
                }

                if (SelectionChanged != null)
                    SelectionChanged(sender, e, selectedRows, index);

                RectangleElement.Update(true);
                //ForegroundRectangleElement.Update();
                IndicatorTextElement.Update(true);
                if(LegendVisibility == Visibility.Visible)
                {
                    LegendRectangleElement.Update(true);
                    LegendTextElement.Update(true);
                }
                args.Handled = true;
            }
        }

        public void Update()
        {
            LegendAreaWidth = 0;
            D3Data = new Data()
            {
                List = Data.Select(d => d as Object).ToList()
            }; 

            if (LegendVisibility == Visibility.Visible)
            {
                LegendRectangleElement.Data = D3Data;
                LegendRectangleElement.Update();

                LegendTextElement.Data = D3Data;
                LegendTextElement.Update();

                LegendAreaWidth = Math.Max(LegendTextElement.MaxActualWidth + Const.LegendPatchWidth + Const.LegendPatchSpace + Const.PaddingRight, Const.MinimumLegendWidth);
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
                ChartAreaEndX = this.Width - Const.PaddingRight;
            }

            HorizontalAxisLabelCanvasLeft = Const.PaddingLeft + Const.VerticalAxisWidth + Const.VerticalAxisLabelWidth;
            HorizontalAxisLabelCanvasTop = ChartAreaEndY + Const.HorizontalAxisHeight;
            HorizontalAxisLabelWidth = ChartAreaEndX - Const.PaddingLeft - Const.VerticalAxisWidth - Const.VerticalAxisLabelWidth;

            VerticalAxisCanvasLeft = Const.PaddingLeft + Const.VerticalAxisLabelWidth + Const.VerticalAxisWidth;
            VerticalAxisLabelCanvasLeft = Const.PaddingLeft + Const.VerticalAxisLabelWidth / 2 - (ChartAreaEndY - Const.PaddingTop) / 2;
            VerticalAxisLabelCanvasTop = Const.PaddingTop + (ChartAreaEndY - Const.PaddingTop) / 2;
            VerticalAxisLabelHeight = ChartAreaEndY - Const.PaddingTop;
            
            IEnumerable<Double> values = Data.Select(d => d.Value);
            Double yMin = values.Min(), yMax = values.Max();
            if (YStartsFromZero) yMin = 0;
            else if (yMin == yMax)
            {
                if (yMin == 0.0)
                {
                    yMin = -1; yMax = 1;
                }
                else if (yMin < 0)
                {
                    yMin *= 1.2;
                    yMax *= 0.8;
                }
                else
                {
                    yMin *= 0.8;
                    yMax *= 1.2;
                }
            }
            else
            {
                yMin *= 0.9;
            }

            YScale = new Linear()
            {
                DomainStart = yMin,
                DomainEnd = yMax,
                RangeStart = ChartAreaEndY,
                RangeEnd = Const.PaddingTop
            };

            YScale.Nice();

            XScale = new Ordinal()
            {
                RangeStart = VerticalAxisCanvasLeft,
                RangeEnd = ChartAreaEndX
            };

            foreach (BarChartDatum datum in Data)
            {
                XScale.Domain.Add(datum);
            }

            // getter가 아닌 경우 재대입을 해야함 예를 들어 visibliity나 Scale등

            HandleRectangleElement.Data = D3Data;

            RectangleElement.Data = D3Data;
            //ForegroundRectangleElement.Data = D3Data;

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

            LegendHandleRectangleElement.Data = D3Data;
            LegendHandleRectangleElement.Visibility = LegendVisibility;

            LegendRectangleElement.Data = D3Data;
            LegendRectangleElement.Visibility = LegendVisibility;

            LegendTextElement.Data = D3Data;
            LegendTextElement.Visibility = LegendVisibility;

            IndicatorTextElement.Data = D3Data;

            LegendHandleRectangleElement.Update();
            HandleRectangleElement.Update();
            RectangleElement.Update();
            //ForegroundRectangleElement.Update();
            IndicatorTextElement.Update();
            HorizontalAxis.Update();
            VerticalAxis.Update();
        }

        public void ClearSelection(Boolean withHandler)
        {
            selectedRows.Clear();

            if (withHandler && SelectionChanged != null)
                SelectionChanged(null, null, selectedRows, 0);

            RectangleElement.Update(true);
            //ForegroundRectangleElement.Update(true);
            IndicatorTextElement.Update(true);
            if (LegendVisibility == Visibility.Visible)
            {
                LegendRectangleElement.Update(true);
                LegendTextElement.Update(true);
            }
        }
    }
}
