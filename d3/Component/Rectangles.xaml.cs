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
using d3.Scale;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace d3.Component
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Rectangles : UserControl
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Data), typeof(Rectangles), new PropertyMetadata(null, new PropertyChangedCallback(DataChanged)));

        public Data Data
        {
            get { return (Data)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty WidthGetterProperty =
            DependencyProperty.Register("WidthGetter", typeof(Func<Object, Int32, Double>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> WidthGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(WidthGetterProperty); }
            set { SetValue(WidthGetterProperty, value); }
        }

        public static readonly DependencyProperty HeightGetterProperty =
            DependencyProperty.Register("HeightGetter", typeof(Func<Object, Int32, Double>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> HeightGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(HeightGetterProperty); }
            set { SetValue(HeightGetterProperty, value); }
        }


        public static readonly DependencyProperty XGetterProperty =
            DependencyProperty.Register("XGetter", typeof(Func<Object, Int32, Double>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> XGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(XGetterProperty); }
            set { SetValue(XGetterProperty, value); }
        }

        public static readonly DependencyProperty YGetterProperty =
            DependencyProperty.Register("YGetter", typeof(Func<Object, Int32, Double>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> YGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(YGetterProperty); }
            set { SetValue(YGetterProperty, value); }
        }

        public static readonly DependencyProperty ColorGetterProperty =
            DependencyProperty.Register("ColorGetter", typeof(Func<Object, Int32, Color>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Int32, Color>)));

        public Func<Object, Int32, Color> ColorGetter
        {
            get { return (Func<Object, Int32, Color>)GetValue(ColorGetterProperty); }
            set { SetValue(ColorGetterProperty, value); }
        }

        public Rectangles()
        {
            this.InitializeComponent();
        }

        private static void DataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Rectangles rectangles = source as Rectangles;
            rectangles.Update();
        }

        List<Rectangle> previousRectangles = new List<Rectangle>();

        public void Update()
        {
            foreach (Rectangle rect in previousRectangles)
            {
                RectanglesCanvas.Children.Remove(rect);
            }
            previousRectangles.Clear();

            Int32 index = 0;
            foreach (Object datum in Data.List)
            {
                Rectangle rect = new Rectangle()
                {
                    Width = WidthGetter(datum, index),
                    Height = HeightGetter(datum, index),
                    Fill = ColorGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(ColorGetter(datum, index))
                };

                Canvas.SetLeft(rect, XGetter(datum, index));
                Canvas.SetTop(rect, YGetter(datum, index));
                index++;
                RectanglesCanvas.Children.Add(rect);
                previousRectangles.Add(rect);
            }
        }
    }
}
