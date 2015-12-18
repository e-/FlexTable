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
    public class D3Rectangle
    {
        public Double X { get; set; }
        public Double Y { get; set; }
        public Double Width { get; set; }
        public Double Height { get; set; }
        public Rectangle Rectangle { get; set; }
    }

    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Rectangles : UserControl
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Data), typeof(Rectangles), new PropertyMetadata(null));
        
        public event Event.EventHandler RectangleTapped;
        public event Event.EventHandler RectangleManipulationDelta;
        public event Event.EventHandler RectangleManipulationCompleted;

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

        public IEnumerable<D3Rectangle> ChildRectangles
        {
            get
            {
                return RectangleCanvas.Children.Select(child =>
                {
                    Rectangle rect = child as Rectangle;

                    return new D3Rectangle()
                    {
                        X = (rect.RenderTransform as CompositeTransform).TranslateX,
                        Y = (rect.RenderTransform as CompositeTransform).TranslateY,
                        Width = (rect.RenderTransform as CompositeTransform).ScaleX,
                        Height = (rect.RenderTransform as CompositeTransform).ScaleY,
                        Rectangle = rect
                    };
                });
            }
        }

        public IEnumerable<Rectangle> Children { get { return RectangleCanvas.Children.Select(child => child as Rectangle); } }
        public TransitionPivotType TransitionPivotType { get; set; }

        public Rectangles()
        {
            this.InitializeComponent();
        }        

        Storyboard previousStoryboard = null;

        public void Update(TransitionType transitionType)
        {

            Int32 index = 0;
            Storyboard sb = new Storyboard()
            {
                //BeginTime = Const.AnimationDelay
            };

            foreach (Object datum in Data.List)
            {
                Rectangle rect = null;

                if (index >= RectangleCanvas.Children.Count) // 없는 사각형은 새로 만듦
                {
                    Int32 localIndex = index;

                    rect = new Rectangle()
                    {
                        Width = 1, //WidthGetter(datum, index),
                        Height = 1, //HeightGetter(datum, index),
                        Fill = ColorGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(ColorGetter(datum, index)),
                        Opacity = OpacityGetter == null ? 1 : OpacityGetter(datum, index),
                        RenderTransform = new CompositeTransform()
                    };
                    
                    rect.Tapped += delegate (object sender, TappedRoutedEventArgs e)
                    {
                        if (RectangleTapped != null)
                        {
                            RectangleTapped(rect, e, Data.List[localIndex], localIndex);
                        }
                    };
                
                    if (RectangleManipulationDelta != null)
                    {
                        rect.ManipulationMode = ManipulationModes.TranslateY | ManipulationModes.System;
                        rect.ManipulationDelta += delegate (object sender, ManipulationDeltaRoutedEventArgs e)
                        {
                            RectangleManipulationDelta(sender, e, Data.List[localIndex], localIndex);
                        };
                    }

                    if (RectangleManipulationCompleted != null)
                    {
                        rect.ManipulationCompleted += delegate (object sender, ManipulationCompletedRoutedEventArgs e)
                        {
                            RectangleManipulationCompleted(sender, e, Data.List[localIndex], localIndex);
                        };
                    }

                    //Canvas.SetLeft(rect, XGetter(datum, index));
                    //Canvas.SetTop(rect, YGetter(datum, index));
                    (rect.RenderTransform as CompositeTransform).TranslateX = XGetter(datum, index);
                    (rect.RenderTransform as CompositeTransform).TranslateY = YGetter(datum, index);
                    (rect.RenderTransform as CompositeTransform).ScaleX = WidthGetter(datum, index);
                    (rect.RenderTransform as CompositeTransform).ScaleY = HeightGetter(datum, index);
                    RectangleCanvas.Children.Add(rect);
                }
                else
                {
                    rect = RectangleCanvas.Children[index] as Rectangle;
                    rect.Visibility = Visibility.Visible;

                    Double newWidth = WidthGetter(datum, index);
                    if (newWidth != rect.Width)
                    {
                        if (transitionType.HasFlag(TransitionType.Size))
                        {
                            sb.Children.Add(Util.GenerateDoubleAnimation(rect.RenderTransform, "ScaleX", newWidth));
                        }
                        else
                        {
                            (rect.RenderTransform as CompositeTransform).ScaleX = newWidth;
                        }
                    }

                    Double newHeight = HeightGetter(datum, index);
                    if (newHeight != rect.Height)
                    {
                        if (transitionType.HasFlag(TransitionType.Size))
                        {
                            sb.Children.Add(Util.GenerateDoubleAnimation(rect.RenderTransform, "ScaleY", newHeight));
                        }
                        else
                        {
                            (rect.RenderTransform as CompositeTransform).ScaleY = newHeight;
                        }
                    }

                    Double newX = XGetter(datum, index);
                    if (newX != (rect.RenderTransform as CompositeTransform).TranslateX)
                    {
                        if (transitionType.HasFlag(TransitionType.Position))
                        {
                            sb.Children.Add(Util.GenerateDoubleAnimation(rect.RenderTransform, "TranslateX", newX));
                        }
                        else
                        {
                            (rect.RenderTransform as CompositeTransform).TranslateX = newX;
                        }
                    }

                    Double newY = YGetter(datum, index);
                    if (newY != (rect.RenderTransform as CompositeTransform).TranslateY)
                    {
                        if (transitionType.HasFlag(TransitionType.Position))
                        {
                            sb.Children.Add(Util.GenerateDoubleAnimation(rect.RenderTransform, "TranslateY", newY));
                        }
                        else
                        {
                            (rect.RenderTransform as CompositeTransform).TranslateY = newY;
                        }
                    }

                    Color newColor = ColorGetter(datum, index);
                    if (newColor != (rect.Fill as SolidColorBrush).Color)
                    {
                        if (transitionType.HasFlag(TransitionType.Color))
                        {
                            sb.Children.Add(Util.GenerateColorAnimation(rect, "(Rectangle.Fill).(SolidColorBrush.Color)", newColor));
                        }
                        else
                        {
                            rect.Fill = new SolidColorBrush(newColor);
                        }
                    }

                    if (OpacityGetter != null)
                    {
                        Double newOpacity = OpacityGetter(datum, index);
                        if (newOpacity != rect.Opacity)
                        {
                            if (transitionType.HasFlag(TransitionType.Opacity))
                            {
                                sb.Children.Add(Util.GenerateDoubleAnimation(rect, "Opacity", newOpacity));
                            }
                            else
                            {
                                rect.Opacity = newOpacity;
                            }
                        }
                    }
                }

                index++;                    
            }

            for (Int32 i = RectangleCanvas.Children.Count - 1; i >= index; --i)
            {
                RectangleCanvas.Children[i].Visibility = Visibility.Collapsed;
            }

            if (previousStoryboard != null) previousStoryboard.Pause();
            if (sb.Children.Count > 0)
            {
                sb.Begin();
                previousStoryboard = sb;
            }
        }
    }
}
