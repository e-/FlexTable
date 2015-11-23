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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace FlexTable.Crayon.Chart
{
    public sealed partial class BarChart : UserControl
    {
        const Double PaddingLeft = 0;
        const Double PaddingTop = 30;
        public const Double PaddingRight = 10;
        const Double PaddingBottom = 0;
        const Double HorizontalAxisHeight = 25;
        const Double HorizontalAxisLabelHeight = 20;
        const Double VerticalAxisWidth = 40;
        const Double VerticalAxisLabelWidth = 20;
        public const Double LegendPatchWidth = 20;
        public const Double LegendPatchHeight = 20;
        public const Double LegendPatchSpace = 10;

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
        public Func<Object, Int32, Double> XGetter { get { return (d, index) => XScale.Map((d as BarChartDatum).Key) - BarWidth / 2; } }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => YScale.Map((d as BarChartDatum).Value); } }
        public Func<Object, Int32, Color> ColorGetter { get { return (bin, index) => (AutoColor ? Category10.Colors[index % 10] : Category10.Colors.First()); } }
        public Func<Object, Int32, Double> OpacityGetter { get { return (d, index) => (selectedKeys.Count == 0) ? 1 : (selectedKeys.IndexOf((d as BarChartDatum).Key) < 0 ? 0.2 : 1); } }

        public Func<Object, Int32, Double> HandleWidthGetter { get { return (d, index) => XScale.RangeBand; } }
        public Func<Object, Int32, Double> HandleHeightGetter { get { return (d, index) => ChartAreaEndY - PaddingTop; } }
        public Func<Object, Int32, Double> HandleXGetter { get { return (d, index) => XScale.Map((d as BarChartDatum).Key) - XScale.RangeBand / 2; } }

        public Func<TextBlock, Double, Double> LabelFontSizeGetter
        {
            get
            {
                return (textBlock, currentSize) => textBlock.ActualWidth > XScale.RangeBand ? currentSize * XScale.RangeBand / textBlock.ActualWidth * 0.9 : currentSize;
            }
        }

        public Func<Object, Int32, Double> LegendPatchWidthGetter { get { return (d, index) => LegendPatchWidth; } }
        public Func<Object, Int32, Double> LegendPatchHeightGetter { get { return (d, index) => LegendPatchHeight; } }
        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - Data.Count() * LegendPatchHeight - (Data.Count() - 1) * LegendPatchSpace) / 2 + index * (LegendPatchHeight + LegendPatchSpace);
            }
        }

        public Func<Object, Int32, Double> LegendHandleWidthGetter { get { return (d, index) => LegendAreaWidth; } }
        public Func<Object, Int32, Double> LegendHandleHeightGetter { get { return (d, index) => LegendPatchHeight + LegendPatchSpace; } }
        public Func<Object, Int32, Double> LegendHandleYGetter
        {
            get
            {
                return (d, index) => (Height - Data.Count() * LegendPatchHeight - (Data.Count() - 1) * LegendPatchSpace) / 2 + index * (LegendPatchHeight + LegendPatchSpace) - LegendPatchSpace / 2;
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => LegendPatchWidth + LegendPatchSpace; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as BarChartDatum).Key.ToString(); } }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => XScale.RangeBand; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => d3.Format.IntegerBalanced.Format((d as BarChartDatum).Value); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) => XScale.Map((d as BarChartDatum).Key) - XScale.RangeBand / 2; } }
        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => YScale.Map((d as BarChartDatum).Value) - 18; } }
        public Func<TextBlock, Object, Int32, Double> IndicatorTextOpacityGetter { get { return (textBlock, d, index) => (selectedKeys.IndexOf((d as BarChartDatum).Key) < 0 ? 0 : 1); } }
        public Func<TextBlock, Object, Int32, Double> TextOpacityGetter { get { return (textBlock, d, index) => (selectedKeys.Count == 0) ? 1 : (selectedKeys.IndexOf((d as BarChartDatum).Key) < 0 ? 0.2 : 1); } }

        public Double HorizontalAxisLabelCanvasTop { get; set; }
        public Double HorizontalAxisLabelCanvasLeft { get; set; }
        public Double HorizontalAxisLabelWidth { get; set; }
        public Double VerticalAxisLabelCanvasTop { get; set; }
        public Double VerticalAxisLabelCanvasLeft { get; set; }
        public Double VerticalAxisLabelHeight { get; set; }
        public Double VerticalAxisCanvasLeft { get; set; }

        public Double LegendAreaWidth { get; set; } = 140;


        //public static readonly DependencyProperty BarPointerPressedProperty = DependencyProperty.Register("BarPointerPressed", typeof(Event.EventHandler), typeof(BarChart), new PropertyMetadata(null));

        //event를 이렇게 low레벨에서 주는게 아니라 선택됐을때 이런 식으로 줘야함
        //public event Event.EventHandler BarPointerPressed;
        //public event Event.EventHandler BarPointerReleased;

        public event Event.EventHandler SelectionChanged;
        private List<Object> selectedKeys = new List<Object>();
        #endregion

        Drawable drawable = new Drawable()
        {
            IgnoreSmallStrokes = true
        };

        public BarChart()
        {
            this.InitializeComponent();

            // getter가 아닌 경우 재대입을 해야함 예를 들어 visibliity나 Scale등

            HandleRectangleElement.Data = D3Data;
            HandleRectangleElement.WidthGetter = HandleWidthGetter;
            HandleRectangleElement.HeightGetter = HandleHeightGetter;
            HandleRectangleElement.XGetter = HandleXGetter;
            HandleRectangleElement.YGetter = d3.Util.CreateConstantGetter<Double>(PaddingTop);

            HandleRectangleElement.ColorGetter = d3.Util.CreateConstantGetter<Color>(Colors.Transparent);

            RectangleElement.Data = D3Data;
            RectangleElement.WidthGetter = WidthGetter;
            RectangleElement.HeightGetter = HeightGetter;
            RectangleElement.XGetter = XGetter;
            RectangleElement.YGetter = YGetter;
            RectangleElement.ColorGetter = ColorGetter;
            RectangleElement.OpacityGetter = OpacityGetter;

            HorizontalAxis.Scale = XScale;
            Canvas.SetTop(HorizontalAxis, ChartAreaEndY);
            HorizontalAxis.Visibility = HorizontalAxisVisibility;
            HorizontalAxis.LabelFontSizeGetter = LabelFontSizeGetter;

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
            LegendHandleRectangleElement.HeightGetter = d3.Util.CreateConstantGetter<Double>(LegendPatchHeight + LegendPatchSpace);
            LegendHandleRectangleElement.XGetter = d3.Util.CreateConstantGetter<Double>(0);
            LegendHandleRectangleElement.YGetter = LegendHandleYGetter;
            LegendHandleRectangleElement.ColorGetter = d3.Util.CreateConstantGetter<Color>(Colors.Transparent);
            LegendHandleRectangleElement.Visibility = LegendVisibility;

            LegendRectangleElement.Data = D3Data;
            LegendRectangleElement.WidthGetter = d3.Util.CreateConstantGetter<Double>(LegendPatchWidth);
            LegendRectangleElement.HeightGetter = d3.Util.CreateConstantGetter<Double>(LegendPatchHeight);
            LegendRectangleElement.XGetter = d3.Util.CreateConstantGetter<Double>(0);
            LegendRectangleElement.YGetter = LegendPatchYGetter;
            LegendRectangleElement.ColorGetter = ColorGetter;
            LegendRectangleElement.OpacityGetter = OpacityGetter;
            LegendRectangleElement.Visibility = LegendVisibility;

            LegendTextElement.Data = D3Data;
            LegendTextElement.TextGetter = LegendTextGetter;
            LegendTextElement.HeightGetter = d3.Util.CreateConstantGetter<Double>(LegendPatchHeight);
            LegendTextElement.XGetter = LegendTextXGetter;
            LegendTextElement.YGetter = LegendPatchYGetter;
            LegendTextElement.ColorGetter = d3.Util.CreateConstantGetter<Color>(Colors.Black);
            LegendTextElement.OpacityGetter = TextOpacityGetter;
            LegendTextElement.Visibility = LegendVisibility;

            IndicatorTextElement.Data = D3Data;
            IndicatorTextElement.TextGetter = IndicatorTextGetter;
            IndicatorTextElement.WidthGetter = IndicatorWidthGetter;
            IndicatorTextElement.XGetter = IndicatorXGetter;
            IndicatorTextElement.YGetter = IndicatorYGetter;
            IndicatorTextElement.OpacityGetter = IndicatorTextOpacityGetter;

            HandleRectangleElement.RectangleTapped += RectangleElement_RectangleTapped;
            LegendHandleRectangleElement.RectangleTapped += RectangleElement_RectangleTapped;

            drawable.Attach(RootCanvas, StrokeGrid, NewStrokeGrid);
            drawable.StrokeAdded += Drawable_StrokeAdded;
        }

        private void Drawable_StrokeAdded(InkManager inkManager)
        {
            if (inkManager.GetStrokes().Count > 0)
            {
                List<Point> points = inkManager.GetStrokes()[0].GetInkPoints().Select(ip => ip.Position).ToList();
                Rect boundingRect = inkManager.GetStrokes()[0].BoundingRect;

                Int32 index = 0;
                List<BarChartDatum> selected = new List<BarChartDatum>();
                Boolean isAllSelected = true;

                index = 0;
                foreach (Rectangle rect in RectangleElement.Children)
                {
                    if (boundingRect.Left <= Canvas.GetLeft(rect) + rect.Width && Canvas.GetLeft(rect) <= boundingRect.Left + boundingRect.Width)
                    {
                        BarChartDatum datum = Data[index];
                        if (selectedKeys.IndexOf(datum.Key) < 0) //선택 안된 데이터가 있으면
                        {
                            isAllSelected = false;
                        }
                        selected.Add(datum);
                    }
                    index++;
                }

                index = 0;
                foreach (Rectangle rect in LegendHandleRectangleElement.Children)
                {
                    if (Canvas.GetLeft(LegendPanel) + Canvas.GetLeft(rect) <= boundingRect.Left && boundingRect.Top <= Canvas.GetTop(rect) + rect.Height && Canvas.GetTop(rect) <= boundingRect.Top + boundingRect.Height)
                    {
                        BarChartDatum datum = Data[index];
                        if (selectedKeys.IndexOf(datum.Key) < 0) //선택 안된 데이터가 있으면
                        {
                            isAllSelected = false;
                        }
                        selected.Add(datum);
                    }
                    index++;
                }

                if (isAllSelected) // 모두가 선택되었다면 선택 해제를 하면 됨
                {
                    foreach (BarChartDatum datum in selected)
                    {
                        selectedKeys.Remove(datum.Key);
                    }
                }
                else // 하나라도 선택 안된게 있으면 선택
                {
                    foreach (BarChartDatum datum in selected)
                    {
                        if (selectedKeys.IndexOf(datum.Key) < 0) { selectedKeys.Add(datum.Key); }
                    }
                }

                RectangleElement.Update(true);
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
                if (selectedKeys.IndexOf(barChartDatum.Key) < 0)
                {
                    selectedKeys.Add(barChartDatum.Key);
                }
                else
                {
                    selectedKeys.Remove(barChartDatum.Key);
                }
                
                if (SelectionChanged != null)
                    SelectionChanged(sender, e, Data.Where(d => selectedKeys.IndexOf(d.Key) >= 0), index);

                RectangleElement.Update(true);
                IndicatorTextElement.Update(true);
                if(LegendVisibility == Visibility.Visible)
                {
                    LegendRectangleElement.Update(true);
                    LegendTextElement.Update(true);
                }
                args.Handled = true;
            }

            /*if (BarPointerPressed != null)
                BarPointerPressed(sender, datum, index);*/
        }

        void RectangleElement_RectanglePointerReleased(object sender, object e, object datum, Int32 index)
        {
            /*if (BarPointerReleased != null)
                BarPointerReleased(sender, datum, index);*/

            /*viewModel.UnselectBar(index);
            RectangleElement.Update(true);
            IndicatorTextElement.Update(true);
            if (viewModel.IsLegendVisible)
            {
                LegendRectangleElement.Update(true);
                LegendTextElement.Update(true);
            }*/
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

                LegendAreaWidth = LegendTextElement.MaxActualWidth + LegendPatchWidth + LegendPatchSpace + PaddingRight;
            }

            Canvas.SetLeft(LegendPanel, this.Width - LegendAreaWidth);

            if (HorizontalAxisVisibility == Visibility.Visible)
            {
                ChartAreaEndY = this.Height - PaddingBottom - HorizontalAxisHeight - HorizontalAxisLabelHeight;
            }
            else
            {
                ChartAreaEndY = this.Height - PaddingBottom;
            }

            if (LegendVisibility == Visibility.Visible)
            {
                ChartAreaEndX = this.Width - PaddingRight - LegendAreaWidth;
            }
            else
            {
                ChartAreaEndX = this.Width - PaddingRight;
            }

            HorizontalAxisLabelCanvasLeft = PaddingLeft + VerticalAxisWidth + VerticalAxisLabelWidth;
            HorizontalAxisLabelCanvasTop = ChartAreaEndY + HorizontalAxisHeight;
            HorizontalAxisLabelWidth = ChartAreaEndX - PaddingLeft - VerticalAxisWidth - VerticalAxisLabelWidth;

            VerticalAxisCanvasLeft = PaddingLeft + VerticalAxisLabelWidth + VerticalAxisWidth;
            VerticalAxisLabelCanvasLeft = PaddingLeft + VerticalAxisLabelWidth / 2 - (ChartAreaEndY - PaddingTop) / 2;
            VerticalAxisLabelCanvasTop = PaddingTop + (ChartAreaEndY - PaddingTop) / 2;
            VerticalAxisLabelHeight = ChartAreaEndY - PaddingTop;
            
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

            YScale = new Linear()
            {
                DomainStart = yMin,
                DomainEnd = yMax,
                RangeStart = ChartAreaEndY,
                RangeEnd = PaddingTop
            };

            YScale.Nice();

            XScale = new Ordinal()
            {
                RangeStart = VerticalAxisCanvasLeft,
                RangeEnd = ChartAreaEndX
            };
            foreach (BarChartDatum datum in Data)
            {
                XScale.Domain.Add(datum.Key);
            }

            // getter가 아닌 경우 재대입을 해야함 예를 들어 visibliity나 Scale등

            HandleRectangleElement.Data = D3Data;

            RectangleElement.Data = D3Data;

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
            IndicatorTextElement.Update();
            HorizontalAxis.Update();
            VerticalAxis.Update();
        }

        public void ClearSelection()
        {
            selectedKeys.Clear();

            if (SelectionChanged != null)
                SelectionChanged(null, null, Data.Where(d => selectedKeys.IndexOf(d.Key) >= 0), 0);

            RectangleElement.Update(true);
            IndicatorTextElement.Update(true);
            if (LegendVisibility == Visibility.Visible)
            {
                LegendRectangleElement.Update(true);
                LegendTextElement.Update(true);
            }
        }
    }
}
