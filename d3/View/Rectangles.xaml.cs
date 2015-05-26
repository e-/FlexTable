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

namespace d3.View
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Rectangles : Page
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Selection.Data), typeof(Rectangles), new PropertyMetadata(null, new PropertyChangedCallback(DataChanged)));

        public Selection.Data Data
        {
            get { return (Selection.Data)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty WidthGetterProperty =
            DependencyProperty.Register("WidthGetter", typeof(Func<Object, Double>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Double> WidthGetter
        {
            get { return (Func<Object, Double>)GetValue(WidthGetterProperty); }
            set { SetValue(WidthGetterProperty, value); }
        }

        public static readonly DependencyProperty HeightGetterProperty =
            DependencyProperty.Register("HeightGetter", typeof(Func<Object, Double>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Double> HeightGetter
        {
            get { return (Func<Object, Double>)GetValue(HeightGetterProperty); }
            set { SetValue(HeightGetterProperty, value); }
        }


        public static readonly DependencyProperty XGetterProperty =
            DependencyProperty.Register("XGetter", typeof(Func<Object, Double>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Double> XGetter
        {
            get { return (Func<Object, Double>)GetValue(XGetterProperty); }
            set { SetValue(XGetterProperty, value); }
        }

        public static readonly DependencyProperty YGetterProperty =
            DependencyProperty.Register("YGetter", typeof(Func<Object, Double>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Double> YGetter
        {
            get { return (Func<Object, Double>)GetValue(YGetterProperty); }
            set { SetValue(YGetterProperty, value); }
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

            foreach (Object datum in Data.Real)
            {

                Debug.WriteLine(HeightGetter(datum));
                Rectangle rect = new Rectangle()
                {
                    Width = WidthGetter(datum),
                    Height = HeightGetter(datum),
                    Fill = new SolidColorBrush(Colors.LightGray)
                };

                Canvas.SetLeft(rect, XGetter(datum));
                Canvas.SetTop(rect, YGetter(datum));


                RectanglesCanvas.Children.Add(rect);
                previousRectangles.Add(rect);
            }
        }
    }
}
