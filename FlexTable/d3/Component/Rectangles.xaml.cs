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
using Windows.UI.Xaml.Media.Animation;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace d3.Component
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Rectangles : UserControl
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Data), typeof(Rectangles), new PropertyMetadata(null));
        
        public event Event.EventHandler RectanglePointerPressed;
        public event Event.EventHandler RectanglePointerReleased;

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

        public static readonly DependencyProperty OpacityGetterProperty =
            DependencyProperty.Register("OpacityGetter", typeof(Func<Object, Int32, Color>), typeof(Rectangles), new PropertyMetadata(default(Func<Object, Int32, Color>)));

        public Func<Object, Int32, Double> OpacityGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(OpacityGetterProperty); }
            set { SetValue(OpacityGetterProperty, value); }
        }

        public Rectangles()
        {
            this.InitializeComponent();
        }        

        public void Update()
        {
            Update(false);
        }

        Storyboard previousStoryboard = null;

        public void Update(Boolean allowTransition)
        {
            if (allowTransition)
            {
                Int32 index = 0;
                Storyboard sb = new Storyboard()
                {
                    //BeginTime = Const.AnimationDelay
                };
                foreach (Object datum in Data.List)
                {
                    Rectangle rect = null;

                    if (index >= RectangleCanvas.Children.Count)
                    {
                        Int32 localIndex = index;
                        rect = new Rectangle()
                        {
                            Width = WidthGetter(datum, index),
                            Height = HeightGetter(datum, index),
                            Fill = ColorGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(ColorGetter(datum, index)),
                            Opacity = OpacityGetter == null ? 1 : OpacityGetter(datum, index)
                        };
                        rect.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
                        {
                            if (RectanglePointerPressed != null)
                            {
                                RectanglePointerPressed(rect, datum, localIndex);
                                e.Handled = true;
                            }
                        };

                        rect.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
                        {
                            if (RectanglePointerReleased != null)
                            {
                                RectanglePointerReleased(rect, datum, localIndex);
                                e.Handled = true;
                            }
                        };
                        rect.Tapped += rect_Tapped;
                        RectangleCanvas.Children.Add(rect);
                    }
                    else
                    {
                        rect = RectangleCanvas.Children[index] as Rectangle;
                        
                        if(XGetter(datum, index) != Canvas.GetLeft(rect))
                        {
                            sb.Children.Add(Util.GenerateDoubleAnimation(rect, "(Canvas.Left)", XGetter(datum, index)));
                        }

                        if (YGetter(datum, index) != Canvas.GetTop(rect))
                        {
                            sb.Children.Add(Util.GenerateDoubleAnimation(rect, "(Canvas.Top)", YGetter(datum, index)));
                        }

                        if (ColorGetter != null && ColorGetter(datum, index) != (rect.Fill as SolidColorBrush).Color)
                        {
                            sb.Children.Add(Util.GenerateColorAnimation(rect, "Fill.Color", ColorGetter(datum, index)));
                        }

                        if (OpacityGetter != null && OpacityGetter(datum, index) != rect.Opacity)
                        {
                            sb.Children.Add(Util.GenerateDoubleAnimation(rect, "Opacity", OpacityGetter(datum, index)));
                        }
                    }

                    index++;                    
                }

                for (Int32 i = RectangleCanvas.Children.Count - 1; i >= index; --i)
                {
                    RectangleCanvas.Children.RemoveAt(i);
                }

                if (previousStoryboard != null) previousStoryboard.Pause();
                sb.Begin();
                previousStoryboard = sb;
            }
            else
            {
                RectangleCanvas.Children.Clear();

                Int32 index = 0;
                foreach (Object datum in Data.List)
                {
                    Int32 localIndex = index;

                    Rectangle rect = new Rectangle()
                    {
                        Width = WidthGetter(datum, index),
                        Height = HeightGetter(datum, index),
                        Fill = ColorGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(ColorGetter(datum, index)),
                        Opacity = OpacityGetter == null ? 1 : OpacityGetter(datum, index)
                    };

                    rect.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
                    {
                        rect.CapturePointer(e.Pointer);
                        if (RectanglePointerPressed != null)
                        {
                            RectanglePointerPressed(rect, datum, localIndex);
                            e.Handled = true;
                        }
                    };
                    
                    rect.PointerCaptureLost += delegate (object sender, PointerRoutedEventArgs e)
                    {
                        if (RectanglePointerReleased != null)
                        {
                            RectanglePointerReleased(rect, datum, localIndex);
                            e.Handled = true;
                        }
                    };

                    /*rect.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
                    {
                        if (RectanglePointerReleased != null)
                        {
                            RectanglePointerReleased(rect, datum, localIndex);
                            e.Handled = true;
                        }
                    };*/

                    rect.Tapped += rect_Tapped;


                    Canvas.SetLeft(rect, XGetter(datum, index));
                    Canvas.SetTop(rect, YGetter(datum, index));
                    index++;
                    RectangleCanvas.Children.Add(rect);
                }
            }
        }

        void rect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
