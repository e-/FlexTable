﻿using System;
using System.Collections.Generic;
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
using d3.Scale;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Animation;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace d3.Component
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Lines : UserControl
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Data), typeof(Lines), new PropertyMetadata(null, new PropertyChangedCallback(DataChanged)));
        
        public event Event.EventHandler LinePointerPressed;
        public event Event.EventHandler LinePointerReleased;
        public event Event.EventHandler LineTapped;

        public Data Data
        {
            get { return (Data)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty CoordinateGetterProperty =
            DependencyProperty.Register("CoordinateGetter", typeof(Func<Object, Int32, List<Point>>), typeof(Lines), new PropertyMetadata(default(Func<Object, List<Point>>)));

        public Func<Object, Int32, List<Point>> CoordinateGetter
        {
            get { return (Func<Object, Int32, List<Point>>)GetValue(CoordinateGetterProperty); }
            set { SetValue(CoordinateGetterProperty, value); }
        }
        
        public static readonly DependencyProperty StrokeGetterProperty =
            DependencyProperty.Register(nameof(StrokeGetter), typeof(Func<Object, Int32, Color>), typeof(Lines), new PropertyMetadata(default(Func<Object, Int32, Color>)));

        public Func<Object, Int32, Color> StrokeGetter
        {
            get { return (Func<Object, Int32, Color>)GetValue(StrokeGetterProperty); }
            set { SetValue(StrokeGetterProperty, value); }
        }

        public static readonly DependencyProperty StrokeThicknessGetterProperty =
            DependencyProperty.Register(nameof(StrokeThicknessGetter), typeof(Func<Object, Int32, Double>), typeof(Lines), new PropertyMetadata(default(Func<Object, Int32, Double>)));

        public Func<Object, Int32, Double> StrokeThicknessGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(StrokeThicknessGetterProperty); }
            set { SetValue(StrokeThicknessGetterProperty, value); }
        }

        public static readonly DependencyProperty OpacityGetterProperty =
            DependencyProperty.Register(nameof(OpacityGetter), typeof(Func<Object, Int32, Double>), typeof(Lines), new PropertyMetadata(default(Func<Object, Int32, Double>)));

        public Func<Object, Int32, Double> OpacityGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(OpacityGetterProperty); }
            set { SetValue(OpacityGetterProperty, value); }
        }


        public Lines()
        {
            this.InitializeComponent();
        }

        private static void DataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            //Rectangles rectangles = source as Rectangles;
            //rectangles.Update();
        }

        List<Path> previousPaths = new List<Path>();
        Storyboard previousStoryboard = null;
        
        public void Update(TransitionType transitionType)
        {
            if (previousStoryboard != null) previousStoryboard.Pause();
            Storyboard sb = new Storyboard()
            {
                ///BeginTime = Const.AnimationDelay
            };

            Int32 index = 0;
            foreach (Object datum in Data.List)
            {
                Path path;
                Object localDatum = datum;

                if (LineCanvas.Children.Count > index)
                {
                    path = LineCanvas.Children[index] as Path;
                    path.Visibility = Visibility.Visible;
                }
                else
                {
                    path = new Path()
                    {
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round,
                        Stroke = StrokeGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(StrokeGetter(datum, index)),
                        StrokeThickness = StrokeThicknessGetter == null ? 2 : StrokeThicknessGetter(datum, index),
                        Opacity = 0
                    };

                    if (LinePointerPressed != null)
                    {
                        path.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
                        {
                            LinePointerPressed(path, e, localDatum);
                        };
                    }

                    if (LinePointerReleased != null)
                    {
                        path.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
                        {
                            LinePointerReleased(path, e, localDatum);
                        };
                    }

                    if (LineTapped != null)
                    {
                        path.Tapped += delegate (object sender, TappedRoutedEventArgs e)
                        {
                            LineTapped(path, e, localDatum);
                        };
                    }

                    LineCanvas.Children.Add(path);
                }

                List<Point> points = CoordinateGetter(datum, index);

                if (points.Count == 0)
                {
                    throw new Exception("No coordinate found");
                }


                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures = new PathFigureCollection();
                PathFigure pathFigure = null;

                foreach (Point point in points)
                {
                    if (point.X < 0)
                    {
                        if (pathFigure != null)
                        {
                            pathGeometry.Figures.Add(pathFigure);
                            pathFigure = null;
                        }
                    }
                    else {
                        if (pathFigure == null)
                        {
                            pathFigure = new PathFigure();
                            pathFigure.StartPoint = point;
                        }
                        else
                        {
                            LineSegment lineSegment = new LineSegment();
                            lineSegment.Point = point;
                            pathFigure.Segments.Add(lineSegment);
                        }
                    }
                }

                if (pathFigure != null)
                {
                    pathGeometry.Figures.Add(pathFigure);
                }

                path.Data = pathGeometry;

                if (transitionType.HasFlag(TransitionType.Opacity)) {
                    sb.Children.Add(Util.GenerateDoubleAnimation(path, "Opacity", OpacityGetter == null ? 1 : OpacityGetter(datum, index)));
                }
                else
                {
                    path.Opacity = OpacityGetter == null ? 1 : OpacityGetter(datum, index);
                }

                if (transitionType.HasFlag(TransitionType.Color))
                {
                    sb.Children.Add(Util.GenerateColorAnimation(path, "(Path.Fill).(SolidColorBrush.Color)",
                         StrokeGetter == null ? Colors.LightGray : StrokeGetter(datum, index)));
                }
                else
                {
                    path.Stroke = StrokeGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(StrokeGetter(datum, index));
                }
                index++;
            }

            previousStoryboard = sb;
            sb.Begin();

            for (Int32 i = LineCanvas.Children.Count - 1; i >= index; i--)
            {
                LineCanvas.Children[i].Visibility = Visibility.Collapsed;
            }
        }
        
    }
}
