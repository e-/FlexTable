using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using d3;
using d3.Scale;
using FlexTable.Model;
using FlexTable.Util;
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
    public sealed partial class GroupedBarChart : UserControl
    {
        

        public IList<GroupedBarChartDatum> Data { get; set; }
        public Data D3Data { get; set; }

        /// <summary>
        /// BarChartDatum의 리스트지만 value는 아무 의미가 없음. 여기 데이터의 key만 사용함
        /// </summary>
        List<BarChartDatum> LegendData { get; set; } = new List<BarChartDatum>();

        /// <summary>
        /// Bar를 그리기 위한 데이터 flatten(Data)라고 생각하면 됨
        /// </summary>
        List<BarChartDatum> ChartData { get; set; } = new List<BarChartDatum>();

        public static readonly DependencyProperty LegendVisibilityProperty =
            DependencyProperty.Register("LegendVisibility", typeof(Visibility), typeof(GroupedBarChart), new PropertyMetadata(Visibility.Visible));

        public Visibility LegendVisibility
        {
            get { return (Visibility)GetValue(LegendVisibilityProperty); }
            set { SetValue(LegendVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisVisibilityProperty =
            DependencyProperty.Register("HorizontalAxisVisibility", typeof(Visibility), typeof(GroupedBarChart), new PropertyMetadata(Visibility.Visible));

        public Visibility HorizontalAxisVisibility
        {
            get { return (Visibility)GetValue(HorizontalAxisVisibilityProperty); }
            set { SetValue(HorizontalAxisVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HorizontalAxisTitleProperty =
            DependencyProperty.Register(nameof(HorizontalAxisTitle), typeof(String), typeof(GroupedBarChart), new PropertyMetadata(String.Empty));

        public String HorizontalAxisTitle
        {
            get { return (String)GetValue(HorizontalAxisTitleProperty); }
            set { SetValue(HorizontalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty VerticalAxisTitleProperty =
            DependencyProperty.Register(nameof(VerticalAxisTitle), typeof(String), typeof(GroupedBarChart), new PropertyMetadata(String.Empty));

        public String VerticalAxisTitle
        {
            get { return (String)GetValue(VerticalAxisTitleProperty); }
            set { SetValue(VerticalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty YStartsFromZeroProperty =
            DependencyProperty.Register(nameof(YStartsFromZero), typeof(Boolean), typeof(GroupedBarChart), new PropertyMetadata(false));

        public bool YStartsFromZero
        {
            get { return (Boolean)GetValue(YStartsFromZeroProperty); }
            set { SetValue(YStartsFromZeroProperty, value); }
        }

        public Ordinal XScale { get; set; } = new Ordinal();
        public Linear YScale { get; set; } = new Linear();

        public Double ChartAreaEndX { get; set; }
        public Double ChartAreaEndY { get; set; } = 300;


        public Double BarWidth { get { return Math.Min(60, XScale.RangeBand * 0.8 / MaxBarCountInAGroup); } }
        public Int32 MaxBarCountInAGroup { get; set; }

        public Func<Object, Int32, Double> WidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, Double> HeightGetter { get { return (d, index) => ChartAreaEndY - YScale.Map((d as BarChartDatum).Value); } }
        public Func<Object, Int32, Double> XGetter
        {
            get
            {
                return (d, index) =>
                {
                    BarChartDatum datum = d as BarChartDatum;
                    return XScale.Map(datum.Parent) - BarWidth * datum.Parent.Children.Count() / 2 + datum.Parent.Children.IndexOf(datum) * BarWidth;
                };
            }
        }
        public Func<Object, Int32, Double> YGetter { get { return (d, index) => YScale.Map((d as BarChartDatum).Value); } }
        public Func<Object, Int32, Color> ColorGetter { get { return (d, index) => ((d as BarChartDatum).Key as Category).Color; } }
        public Func<Object, Int32, Double> OpacityGetter { get {
                return (d, index) =>
                    selectedKeys.Count == 0 ? 1 : (
                    selectedKeys.Count(key => key.Key1 == ((BarChartDatum)d).Parent.Key && key.Key2 == ((BarChartDatum)d).Key) > 0 ? 1 : 0.2);
        } }

        public Func<Object, Int32, Double> HandleWidthGetter { get { return (d, index) => BarWidth; } }
        public Func<Object, Int32, Double> HandleHeightGetter { get { return (d, index) => ChartAreaEndY - Const.PaddingTop; } }
        public Func<Object, Int32, Double> HandleXGetter { get { return (d, index) => XGetter(d, index); } }

        /*public Func<TextBlock, Double, Double> LabelFontSizeGetter
        {
            get
            {
                return (textBlock, currentSize) => textBlock.ActualWidth > XScale.RangeBand ? currentSize * XScale.RangeBand / textBlock.ActualWidth * 0.9 : currentSize;
            }
        }*/

        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - LegendData.Count() * Const.LegendPatchHeight - (LegendData.Count() - 1) * Const.LegendPatchSpace) / 2 + index * (Const.LegendPatchHeight + Const.LegendPatchSpace);
            }
        }
        public Func<Object, Int32, Double> LegendPatchOpacityGetter
        {
            get
            {
                return (d, index) =>
                    selectedKeys.Count == 0 ? 1 : (
                    selectedKeys.Count(key => key.Key2 == ((BarChartDatum)d).Key) > 0 ? 1 : 0.2);
            }
        }

        public Func<Object, Int32, Double> LegendHandleWidthGetter { get { return (d, index) => LegendAreaWidth; } }
        public Func<Object, Int32, Double> LegendHandleYGetter
        {
            get
            {
                return (d, index) => (Height - LegendData.Count() * Const.LegendPatchHeight - (LegendData.Count() - 1) * Const.LegendPatchSpace) / 2 + index * (Const.LegendPatchHeight + Const.LegendPatchSpace) - Const.LegendPatchSpace / 2;
            }
        }

        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => Const.LegendPatchWidth + Const.LegendPatchSpace; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as BarChartDatum).Key.ToString(); } }

        //public Func<TextBlock, Object, Int32, Double> LegendTextOpacityGetter { get { return (textBlock, d, index) => IsSelecting ? (d.ToString() == selectedKey2.ToString() ? 1.0 : 0.2) : 1.0; } }
        public Func<TextBlock, Object, Int32, Double> LegendTextOpacityGetter { get {
                return (textBlock, d, index) =>
                    selectedKeys.Count == 0 ? 1 : (
                    selectedKeys.Count(key => key.Key2 == ((BarChartDatum)d).Key) > 0 ? 1 : 0.2);
        } }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => 100; } }// XScale.RangeBand; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => d3.Format.IntegerBalanced.Format((d as BarChartDatum).Value); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) => XGetter(d, index) + BarWidth / 2 - 50; } }// XScale.Map((d as BarChartDatum).Key) - XScale.RangeBand / 2; } }
        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => YScale.Map((d as BarChartDatum).Value) - 18; } }
        public Func<TextBlock, Object, Int32, Double> IndicatorTextOpacityGetter { get {
                return (textBlock, d, index) =>
                    selectedKeys.Count == 0 ? 0 : (
                    selectedKeys.Count(key => key.Key1 == ((BarChartDatum)d).Parent.Key && key.Key2 == ((BarChartDatum)d).Key) > 0 ? 1 : 0);
            } }

        public Double HorizontalAxisLabelCanvasTop { get; set; }
        public Double HorizontalAxisLabelCanvasLeft { get; set; }
        public Double HorizontalAxisLabelWidth { get; set; }
        public Double VerticalAxisLabelCanvasTop { get; set; }
        public Double VerticalAxisLabelCanvasLeft { get; set; }
        public Double VerticalAxisLabelHeight { get; set; }
        public Double VerticalAxisCanvasLeft { get; set; }

        public Double LegendAreaWidth { get; set; } = 140;

        /*private Double DragToFilterYDelta = 0;
        private Double DragToFilterOpacity = 1;
        private BarChartDatum DragToFilterFocusedBar = null;*/

        public event Event.EventHandler SelectionChanged;
        private List<Key> selectedKeys = new List<Key>();

        public event Event.EventHandler FilterIn;
        public event Event.EventHandler FilterOut;

        private List<BarChartDatum> selectedData = new List<BarChartDatum>();

        Drawable drawable = new Drawable()
        {
            //IgnoreSmallStrokes = true
        };

        public GroupedBarChart()
        {
            this.InitializeComponent();

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
            LegendRectangleElement.ColorGetter = ColorGetter;
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

            IndicatorTextElement.Data = D3Data;
            IndicatorTextElement.TextGetter = IndicatorTextGetter;
            IndicatorTextElement.WidthGetter = IndicatorWidthGetter;
            IndicatorTextElement.XGetter = IndicatorXGetter;
            IndicatorTextElement.YGetter = IndicatorYGetter;
            IndicatorTextElement.OpacityGetter = IndicatorTextOpacityGetter;
            
            HandleRectangleElement.RectangleTapped += RectangleElement_RectangleTapped;
            LegendHandleRectangleElement.RectangleTapped += LegendHandleRectangleElement_RectangleTapped; ;
            /*HandleRectangleElement.RectangleManipulationDelta += HandleRectangleElement_RectangleManipulationDelta;
            HandleRectangleElement.RectangleManipulationCompleted += HandleRectangleElement_RectangleManipulationCompleted;*/

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
                Boolean isLegendStrikeThrough = false;
                BarChartDatum victim = null;

                index = 0;
                foreach (Rectangle rect in RectangleElement.Children)
                {
                    if (boundingRect.Left <= Canvas.GetLeft(rect) + rect.Width && Canvas.GetLeft(rect) <= boundingRect.Left + boundingRect.Width)
                    {
                        BarChartDatum datum = ChartData[index];
                        if (selectedKeys.Count(k => k.Key1 == datum.Parent.Key && k.Key2 == datum.Key) == 0) //선택 안된 데이터가 있으면
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
                    if (Canvas.GetLeft(LegendPanel) + Canvas.GetLeft(rect) <= boundingRect.Right && boundingRect.Top <= Canvas.GetTop(rect) + rect.Height && Canvas.GetTop(rect) <= boundingRect.Top + boundingRect.Height)
                    {
                        BarChartDatum datum = LegendData[index];

                        if (boundingRect.Height < Const.StrikeThroughMaxHeight && boundingRect.Width > Const.StrikeThroughMinWidth) // legend strike through면
                        {
                            isLegendStrikeThrough = true;
                            victim = datum;
                            break;
                        }
                        else
                        {
                            foreach (BarChartDatum bcd in Data.SelectMany(d => d.Children).Where(bcd => bcd.Key == datum.Key))
                            {
                                if (selectedKeys.Count(k => k.Key1 == bcd.Parent.Key && k.Key2 == bcd.Key) == 0) //선택 안된 데이터가 있으면
                                {
                                    isAllSelected = false;
                                }
                                selected.Add(bcd);
                            }
                        }
                    }
                    index++;
                }

                if (isLegendStrikeThrough)
                {
                    if (!(victim.Key as Category).IsVirtual)
                    {
                        selectedKeys.RemoveAll(key => key.Key2 == victim.Key);
                        selectedData.RemoveAll(datum => datum.Key == victim.Key);

                        if (FilterOut != null)
                        {
                            FilterOut(this, null, ChartData.Where(datum => datum.Key == victim.Key), index);
                        }
                    }
                }
                else if (isAllSelected) // 모두가 선택되었다면 선택 해제를 하면 됨
                {
                    selectedData = selectedData.Except(selected).ToList();
                    selectedKeys = selectedData.Select(d => new Key(d.Parent.Key, d.Key)).ToList();
                }
                else // 하나라도 선택 안된게 있으면 선택
                {
                    foreach (BarChartDatum datum in selected)
                    {
                        if(selectedKeys.Count(k => k.Key1 == datum.Parent.Key && k.Key2 == datum.Key) == 0)
                        {
                            selectedKeys.Add(new Key(datum.Parent.Key, datum.Key));
                            selectedData.Add(datum);
                        }
                    }
                }

                if (Data.SelectMany(d => d.Children).Count() == selectedKeys.Count)
                {
                    selectedKeys.Clear();
                    selectedData.Clear();
                }

                if (SelectionChanged != null)
                    SelectionChanged(this, null, selectedData, index);

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

        private void LegendHandleRectangleElement_RectangleTapped(object sender, object e, object datum, int index)
        {
            TappedRoutedEventArgs args = e as TappedRoutedEventArgs;
            if (args.PointerDeviceType == PointerDeviceType.Touch)
            {
                BarChartDatum barChartDatum = datum as BarChartDatum;

                if (selectedKeys.Count(k => k.Key2 == barChartDatum.Key) == Data.SelectMany(d => d.Children).Count(d => d.Key == barChartDatum.Key)) // 현재 키 모두 선택됐음
                {
                    selectedKeys.RemoveAll(k => k.Key2 == barChartDatum.Key);
                    selectedData.RemoveAll(d => d.Key == barChartDatum.Key);
                }
                else // 하나라도 선택 안된것 있음 그러면 모두 선택
                {
                    foreach(BarChartDatum bcd in Data.SelectMany(d => d.Children).Where(bcd => bcd.Key == barChartDatum.Key))
                    {
                        if(selectedKeys.Count(k => k.Key1 == bcd.Parent.Key && k.Key2 == bcd.Key) == 0) // 없으면 추가
                        {
                            selectedKeys.Add(new Key(bcd.Parent.Key, bcd.Key));
                            selectedData.Add(bcd);
                        }
                    }
                }

                if(Data.SelectMany(d => d.Children).Count() == selectedKeys.Count)
                {
                    selectedKeys.Clear();
                    selectedData.Clear();
                }

                if (SelectionChanged != null)
                    SelectionChanged(sender, e, selectedData, index);

                RectangleElement.Update(true);
                IndicatorTextElement.Update(true);
                if (LegendVisibility == Visibility.Visible)
                {
                    LegendRectangleElement.Update(true);
                    LegendTextElement.Update(true);
                }
                args.Handled = true;
            }
        }


        void RectangleElement_RectangleTapped(object sender, object e, object datum, Int32 index)
        {
            TappedRoutedEventArgs args = e as TappedRoutedEventArgs;
            if (args.PointerDeviceType == PointerDeviceType.Touch)
            {
                BarChartDatum barChartDatum = datum as BarChartDatum;
                Key key = new Key(barChartDatum.Parent.Key, barChartDatum.Key);

                if (selectedKeys.Count(d => d.Equals(key)) == 0)
                {
                    selectedKeys.Add(key);
                    selectedData.Add(barChartDatum);
                }
                else
                {
                    selectedKeys.Remove(key);
                    selectedData.Remove(barChartDatum);
                }

                if (Data.SelectMany(d => d.Children).Count() == selectedKeys.Count)
                {
                    selectedKeys.Clear();
                    selectedData.Clear();
                }

                if (SelectionChanged != null)
                    SelectionChanged(sender, e, selectedData, index);

                RectangleElement.Update(true);
                IndicatorTextElement.Update(true);
                if (LegendVisibility == Visibility.Visible)
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

            /*
                기존 데이터를 여러 각도로 수정해야함
                1. 레전드용 데이터: 두번째 키만 모아야함. 이것은 색깔을 배치할때도 쓰임
                2. 사각형 그리기용 데이터: 왜냐하면 hierarchical하게 있으면 안되므로
            */

            // legend 데이터 수정

            if (LegendVisibility == Visibility.Visible)
            {
                LegendData.Clear();
                foreach (GroupedBarChartDatum gbcd in Data)
                {
                    foreach (BarChartDatum bcd in gbcd.Children)
                    {
                        if (LegendData.Select(d => d.Key).Count(d => d == bcd.Key) == 0)
                        {
                            LegendData.Add(bcd);
                        }
                    }
                }

                LegendRectangleElement.Data = new Data() { List = LegendData.Select(d => d as Object).ToList() };
                LegendRectangleElement.Update();

                LegendTextElement.Data = new Data() { List = LegendData.Select(d => d as Object).ToList() };
                LegendTextElement.Update();

                LegendAreaWidth = Math.Max(LegendTextElement.MaxActualWidth + Const.LegendPatchWidth + Const.LegendPatchSpace + Const.PaddingRight, Const.MinimumLegendWidth);
            }

            // chart data 수정
            ChartData = Data.SelectMany(d => d.Children).ToList();

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

            IEnumerable<Double> values = Data.SelectMany(d => d.Children).Select(d => d.Value);

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
                RangeEnd = ChartAreaEndX + Const.PaddingLeft
            };

            foreach (GroupedBarChartDatum datum in Data)
            {
                XScale.Domain.Add(datum);
            }

            MaxBarCountInAGroup = Data.Select(d => d.Children.Count()).Max();


            // update 시 재대입 할 것들 대입

            HandleRectangleElement.Data = new Data() { List = ChartData.Select(d => d as Object).ToList() }; ;

            RectangleElement.Data = new Data() { List = ChartData.Select(d => d as Object).ToList() }; ;

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

            LegendHandleRectangleElement.Data = new Data() { List = LegendData.Select(d => d as Object).ToList() };
            LegendHandleRectangleElement.Visibility = LegendVisibility;

            LegendRectangleElement.Data = new Data() { List = LegendData.Select(d => d as Object).ToList() };
            LegendRectangleElement.Visibility = LegendVisibility;

            LegendTextElement.Data = new Data() { List = LegendData.Select(d => d as Object).ToList() };
            LegendTextElement.Visibility = LegendVisibility;

            IndicatorTextElement.Data = new Data() { List = ChartData.Select(d => d as Object).ToList() };

            LegendHandleRectangleElement.Update();
            HandleRectangleElement.Update();
            RectangleElement.Update();
            IndicatorTextElement.Update();
            HorizontalAxis.Update();
            VerticalAxis.Update();
        }

        public void ClearSelection(Boolean withHandler)
        {
            selectedKeys.Clear();
            selectedData.Clear();

            if (withHandler && SelectionChanged != null)
                SelectionChanged(null, null, selectedData, 0);

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
