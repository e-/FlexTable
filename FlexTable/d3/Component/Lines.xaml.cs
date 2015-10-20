using System;
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

        public void Update()
        {
            Update(false);
        }

        public void Update(Boolean useTransition)
        {
            if (useTransition)
            {
                Int32 index = 0;
                if (previousStoryboard != null) previousStoryboard.Pause();
                Storyboard sb = new Storyboard()
                {
                    ///BeginTime = Const.AnimationDelay
                };
                foreach (Object datum in Data.List)
                {
                    Path path = LineCanvas.Children[index] as Path;

                    if (OpacityGetter != null && path.Opacity != OpacityGetter(datum, index))
                    {
                        sb.Children.Add(Util.GenerateDoubleAnimation(path, "Opacity", OpacityGetter(datum, index)));
                    }
                    index++;
                }
                previousStoryboard = sb;
                sb.Begin();
            }
            else
            {
                foreach (Path path in previousPaths)
                {
                    LineCanvas.Children.Remove(path);
                }
                previousPaths.Clear();

                Int32 index = 0;
                foreach (Object datum in Data.List)
                {
                    List<Point> points = CoordinateGetter(datum, index);

                    if (points.Count == 0)
                    {
                        throw new Exception("No coordinate found");
                    }

                    PathFigure pathFigure = new PathFigure();

                    pathFigure.StartPoint = points.First();

                    foreach (Point point in points.Where((p, i) => i > 0))
                    {
                        LineSegment lineSegment = new LineSegment();
                        lineSegment.Point = point;
                        pathFigure.Segments.Add(lineSegment);
                    }

                    PathGeometry pathGeometry = new PathGeometry();
                    pathGeometry.Figures = new PathFigureCollection();

                    pathGeometry.Figures.Add(pathFigure);

                    Path path = new Path()
                    {
                        Data = pathGeometry,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round,
                        Stroke = StrokeGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(StrokeGetter(datum, index)),
                        StrokeThickness = StrokeThicknessGetter == null ? 2 : StrokeThicknessGetter(datum, index),
                        Opacity = OpacityGetter == null ? 1 : OpacityGetter(datum, index)
                    };

                    path.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
                    {
                        if (LinePointerPressed != null)
                        {
                            LinePointerPressed(path, datum, index);
                            e.Handled = true;
                        }
                    };

                    path.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
                    {
                        if (LinePointerReleased != null)
                        {
                            LinePointerReleased(path, datum, index);
                            e.Handled = true;
                        }
                    };

                    path.Tapped += rect_Tapped;

                    index++;
                    LineCanvas.Children.Add(path);
                    previousPaths.Add(path);
                }
            }
        }

        void rect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
