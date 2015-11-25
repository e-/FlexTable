﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using d3;
using d3.ColorScheme;
using d3.Scale;
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
    public sealed partial class LineChart : UserControl
    {
        const Double StrikeThroughMinWidth = 50;
        const Double StrikeThroughMaxHeight = 30;

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

        public IList<LineChartDatum> Data { get; set; }
        public Data D3Data { get; set; }

        /// <summary>
        /// Circle을 그리기 위한 데이터 = indicator를 그리기 위한 데이터
        /// </summary>
        List<DataPoint> CircleData { get; set; }
        public Data D3CircleData { get; set; }

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

        public static readonly DependencyProperty HorizontalAxisTitleProperty =
            DependencyProperty.Register(nameof(HorizontalAxisTitle), typeof(String), typeof(LineChart), new PropertyMetadata(String.Empty));

        public String HorizontalAxisTitle
        {
            get { return (String)GetValue(HorizontalAxisTitleProperty); }
            set { SetValue(HorizontalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty VerticalAxisTitleProperty =
            DependencyProperty.Register(nameof(VerticalAxisTitle), typeof(String), typeof(LineChart), new PropertyMetadata(String.Empty));

        public String VerticalAxisTitle
        {
            get { return (String)GetValue(VerticalAxisTitleProperty); }
            set { SetValue(VerticalAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty AutoColorProperty =
            DependencyProperty.Register("AutoColor", typeof(Boolean), typeof(LineChart), new PropertyMetadata(true));

        public Boolean AutoColor
        {
            get { return (Boolean)GetValue(AutoColorProperty); }
            set { SetValue(AutoColorProperty, value); }
        }

        public static readonly DependencyProperty YStartsFromZeroProperty =
            DependencyProperty.Register(nameof(YStartsFromZero), typeof(Boolean), typeof(LineChart), new PropertyMetadata(false));

        public bool YStartsFromZero
        {
            get { return (Boolean)GetValue(YStartsFromZeroProperty); }
            set { SetValue(YStartsFromZeroProperty, value); }
        }

        public Ordinal XScale { get; set; } = new Ordinal();
        public Linear YScale { get; set; } = new Linear();

        public Double ChartAreaEndX { get; set; }
        public Double ChartAreaEndY { get; set; } = 300;

        public Func<Object, Int32, List<Point>> LineCoordinateGetter
        {
            get
            {
                return (d, index) => (d as LineChartDatum).DataPoints.Select<DataPoint, Point>(dp => new Point(
                   XScale.Map(dp.Item1),
                   YScale.Map(dp.Item2)
                   )).ToList();
            }
        }

        public Func<Object, Int32, Color> ColorGetter { get { return (bin, index) => (AutoColor ? Category10.Colors[index % 10] : Category10.Colors.First()); } }
        public Func<Object, Int32, Color> LineStrokeGetter { get { return (bin, index) => (AutoColor ? Category10.Colors[index % 10] : Category10.Colors.First()); } }
        public Func<Object, Int32, Double> LineOpacityGetter { get {
                return (d, index) => ((selectedKeys.Count == 0) ? 0.8 :
                    (selectedKeys.IndexOf((d as LineChartDatum).Key) >= 0 ? 0.9 : 0.1));
                    } }

        public Func<Object, Int32, Double> CircleXGetter { get { return (d, index) => XScale.Map((d as DataPoint).Item1); } }
        public Func<Object, Int32, Double> CircleYGetter { get { return (d, index) => YScale.Map((d as DataPoint).Item2); } }
        public Func<Object, Int32, Color> CircleColorGetter { get { return (d, index) => Category10.Colors[Data.IndexOf((d as DataPoint).Parent) % 10]; } }
        public Func<Object, Int32, Double> CircleOpacityGetter{ get {
                return (d, index) => ((selectedKeys.Count == 0) ? 0.9 :
                    (selectedKeys.IndexOf((d as DataPoint).Parent.Key) >= 0 ? 1 : 0.1));
            } }

        public Func<Object, Int32, Double> LegendHandleWidthGetter { get { return (d, index) => LegendAreaWidth; } }
        public Func<Object, Int32, Double> LegendHandleYGetter
        {
            get
            {
                return
                    (d, index) => (Height - Data.Count() * LegendPatchHeight - (Data.Count() - 1) * LegendPatchSpace) / 2 + index * (LegendPatchHeight + LegendPatchSpace) - LegendPatchSpace / 2;
            }
        }

        public Func<Object, Int32, Double> LegendPatchYGetter
        {
            get
            {
                return (d, index) => (Height - Data.Count() * LegendPatchHeight - (Data.Count() - 1) * LegendPatchSpace) / 2 + index * (LegendPatchHeight + LegendPatchSpace);
            }
        }
        public Func<Object, Int32, Double> LegendTextXGetter { get { return (d, index) => LegendPatchWidth + LegendPatchSpace; } }
        public Func<Object, Int32, String> LegendTextGetter { get { return (d, index) => (d as LineChartDatum).Key.ToString(); } }
        public Func<Object, Int32, Double> LegendOpacityGetter { get
            {
                return (d, index) => ((selectedKeys.Count == 0) ? 1 :
                    (selectedKeys.IndexOf((d as LineChartDatum).Key) >= 0 ? 1 : 0.2));
            } }
        public Func<TextBlock, Object, Int32, Double> LegendTextOpacityGetter { get {
                return (textBlock, d, index) => ((selectedKeys.Count == 0) ? 1 :
                    (selectedKeys.IndexOf((d as LineChartDatum).Key) >= 0 ? 1 : 0.2));
            } }

        public Func<Object, Int32, Double> IndicatorWidthGetter { get { return (d, index) => XScale.RangeBand; } }
        public Func<Object, Int32, String> IndicatorTextGetter { get { return (d, index) => d3.Format.IntegerBalanced.Format((Double)(d as DataPoint).Item2); } }
        public Func<Object, Int32, Double> IndicatorXGetter { get { return (d, index) => XScale.Map((d as DataPoint).Item1) - XScale.RangeBand / 2; } }
        public Func<Object, Int32, Double> IndicatorYGetter { get { return (d, index) => YScale.Map((d as DataPoint).Item2) - 22; } }

        public Func<TextBlock, Object, Int32, Double> IndicatorTextOpacityGetter
        {
            get
            {
                return (textBlock, d, index) => ((selectedKeys.Count == 0) ? 0 : 
                    (selectedKeys.IndexOf((d as DataPoint).Parent.Key) >= 0 ? 1 : 0));
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

        public event Event.EventHandler SelectionChanged;
        public event Event.EventHandler FilterIn;
        public event Event.EventHandler FilterOut;

        /// <summary>
        /// BarChartDatum 의 key가 들어가야함
        /// </summary>
        private List<Object> selectedKeys = new List<Object>();

        Drawable drawable = new Drawable()
        {
            IgnoreSmallStrokes = true
        };

        public LineChart()
        {
            this.InitializeComponent();
            
            HandleLineElement.Data = D3Data;
            HandleLineElement.CoordinateGetter = LineCoordinateGetter;
            HandleLineElement.StrokeThicknessGetter = d3.Util.CreateConstantGetter<Double>(20);
            HandleLineElement.StrokeGetter = d3.Util.CreateConstantGetter<Color>(Colors.Transparent);
            HandleLineElement.OpacityGetter = d3.Util.CreateConstantGetter<Double>(1);

            LineElement.Data = D3Data;
            LineElement.CoordinateGetter = LineCoordinateGetter;
            LineElement.StrokeThicknessGetter = d3.Util.CreateConstantGetter<Double>(5);
            LineElement.StrokeGetter = LineStrokeGetter;
            LineElement.OpacityGetter = LineOpacityGetter;

            CircleElement.Data = D3Data;
            CircleElement.XGetter = CircleXGetter;
            CircleElement.YGetter = CircleYGetter;
            CircleElement.RadiusGetter = d3.Util.CreateConstantGetter<Double>(10);
            CircleElement.ColorGetter = CircleColorGetter;
            CircleElement.OpacityGetter = CircleOpacityGetter;

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
            LegendRectangleElement.OpacityGetter = LegendOpacityGetter;
            LegendRectangleElement.Visibility = LegendVisibility;

            LegendTextElement.Data = D3Data;
            LegendTextElement.TextGetter = LegendTextGetter;
            LegendTextElement.HeightGetter = d3.Util.CreateConstantGetter<Double>(LegendPatchHeight);
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
            
            HandleLineElement.LineTapped += LegendHandleRectangleElement_RectangleTapped;

            LegendHandleRectangleElement.RectangleTapped += LegendHandleRectangleElement_RectangleTapped;

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
                List<LineChartDatum> selected = new List<LineChartDatum>();
                Boolean isAllSelected = true;
                Boolean isLegendStrikeThrough = false;
                LineChartDatum victim = null;

                index = 0;
                foreach (LineChartDatum datum in Data)
                {
                    List<Point> circlePoints = LineCoordinateGetter(datum, index);

                    if(circlePoints.Exists(cp => boundingRect.Contains(cp))) // 하나라도 포함되는 포인트가 있으면 예를 선택
                    {
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
                        LineChartDatum datum = Data[index];

                        if (boundingRect.Height < StrikeThroughMaxHeight && boundingRect.Width > StrikeThroughMinWidth) // legend strike through면
                        {
                            isLegendStrikeThrough = true;
                            victim = datum;
                            break;
                        }
                        else
                        {
                            if (selectedKeys.IndexOf(datum.Key) < 0) //선택 안된 데이터가 있으면
                            {
                                isAllSelected = false;
                            }
                            selected.Add(datum);
                        }
                    }
                    index++;
                }

                if (isLegendStrikeThrough)
                {
                    selectedKeys.Remove(victim.Key);
                    if (FilterOut != null)
                    {
                        FilterOut(this, null, new List<LineChartDatum>() { victim }, index);
                    }
                }
                else if (isAllSelected) // 모두가 선택되었다면 선택 해제를 하면 됨
                {
                    foreach (LineChartDatum datum in selected)
                    {
                        selectedKeys.Remove(datum.Key);
                    }
                }
                else // 하나라도 선택 안된게 있으면 선택
                {
                    foreach (LineChartDatum datum in selected)
                    {
                        if (selectedKeys.IndexOf(datum.Key) < 0) { selectedKeys.Add(datum.Key); }
                    }
                }

                if (selectedKeys.Count == Data.Count)
                {
                    selectedKeys.Clear();
                }

                if (SelectionChanged != null)
                    SelectionChanged(this, null, Data.Where(d => selectedKeys.IndexOf(d.Key) >= 0), index);

                LineElement.Update(true);
                CircleElement.Update(true, false);
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
                LineChartDatum lineChartDatum = datum as LineChartDatum;
                if (selectedKeys.IndexOf(lineChartDatum.Key) < 0)
                {
                    selectedKeys.Add(lineChartDatum.Key);
                }
                else
                {
                    selectedKeys.Remove(lineChartDatum.Key);
                }

                if (selectedKeys.Count == Data.Count)
                {
                    selectedKeys.Clear();
                }

                if (SelectionChanged != null)
                    SelectionChanged(sender, e, Data.Where(d => selectedKeys.IndexOf(d.Key) >= 0), index);

                LineElement.Update(true);
                CircleElement.Update(true, false);
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

            CircleData = Data.SelectMany(l => l.DataPoints).ToList();
            D3CircleData = new Data()
            {
                List = CircleData.Select(d => d as Object).ToList()
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

            Double yMin = CircleData.Select(dp => (Double)dp.Item2).Min(),
                   yMax = CircleData.Select(dp => (Double)dp.Item2).Max();

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
                RangeEnd = ChartAreaEndX + PaddingLeft
            };

            foreach(DataPoint dp in Data[0].DataPoints)
            {
                XScale.Domain.Add(dp.Item1);
            }

            HandleLineElement.Data = D3Data;

            LineElement.Data = D3Data;

            CircleElement.Data = D3CircleData;

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

            IndicatorTextElement.Data = D3CircleData;

            HandleLineElement.Update();
            LineElement.Update();
            CircleElement.Update(false, false);
            LegendHandleRectangleElement.Update();
            IndicatorTextElement.Update();
            HorizontalAxis.Update();
            VerticalAxis.Update();
        }
    }
}
