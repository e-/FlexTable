using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace d3.Component
{
    public sealed partial class Circles : UserControl
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Data), typeof(Circles), new PropertyMetadata(null));
        
        //public event Event.EventHandler CirclePointerPressed;
        //public event Event.EventHandler CirclePointerReleased;

        public Data Data
        {
            get { return (Data)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty RadiusGetterProperty =
            DependencyProperty.Register("RadiusGetter", typeof(Func<Object, Int32, Double>), typeof(Circles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> RadiusGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(RadiusGetterProperty); }
            set { SetValue(RadiusGetterProperty, value); }
        }

        public static readonly DependencyProperty XGetterProperty =
            DependencyProperty.Register("XGetter", typeof(Func<Object, Int32, Double>), typeof(Circles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> XGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(XGetterProperty); }
            set { SetValue(XGetterProperty, value); }
        }

        public static readonly DependencyProperty YGetterProperty =
            DependencyProperty.Register("YGetter", typeof(Func<Object, Int32, Double>), typeof(Circles), new PropertyMetadata(default(Func<Object, Double>)));

        public Func<Object, Int32, Double> YGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(YGetterProperty); }
            set { SetValue(YGetterProperty, value); }
        }

        public static readonly DependencyProperty ColorGetterProperty =
            DependencyProperty.Register("ColorGetter", typeof(Func<Object, Int32, Color>), typeof(Circles), new PropertyMetadata(default(Func<Object, Int32, Color>)));

        public Func<Object, Int32, Color> ColorGetter
        {
            get { return (Func<Object, Int32, Color>)GetValue(ColorGetterProperty); }
            set { SetValue(ColorGetterProperty, value); }
        }

        public static readonly DependencyProperty OpacityGetterProperty =
            DependencyProperty.Register("OpacityGetter", typeof(Func<Object, Int32, Double>), typeof(Circles), new PropertyMetadata(default(Func<Object, Int32, Double>)));

        public Func<Object, Int32, Double> OpacityGetter
        {
            get { return (Func<Object, Int32, Double>)GetValue(OpacityGetterProperty); }
            set { SetValue(OpacityGetterProperty, value); }
        }

        public static readonly DependencyProperty ReusePreviousElementProperty =
            DependencyProperty.Register("ReusePreviousElement", typeof(Boolean), typeof(Circles), new PropertyMetadata(true)
                );

        public Boolean ReusePreviousElement
        {
            get { return (Boolean)GetValue(ReusePreviousElementProperty); }
            set { SetValue(ReusePreviousElementProperty, value); }
        }

        /*public static readonly DependencyProperty DeferredTransitionProperty =
            DependencyProperty.Register("DeferredTransition", typeof(Boolean), typeof(Circles), new PropertyMetadata(true)
                );
    
        public Boolean DeferredTransition
        {
            get { return (Boolean)GetValue(DeferredTransitionProperty); }
            set { SetValue(DeferredTransitionProperty, value); }
        }*/

        /*public static readonly DependencyProperty AllowTransitionProperty =
            DependencyProperty.Register("AllowTransition", typeof(Boolean), typeof(Circles), new PropertyMetadata(true)
                );

        public Boolean AllowTransition
        {
            get { return (Boolean)GetValue(AllowTransitionProperty); }
            set { SetValue(AllowTransitionProperty, value); }
        }*/

        public Circles()
        {
            this.InitializeComponent();
        }        

        Storyboard previousStoryboard;
        
        /*public void Update()
        {
            Update(false);
        }*/

        public void Update(Boolean useTransition, Boolean deferredTransition)
        {
            Int32 index = 0;
            Storyboard storyboard = new Storyboard();

            if (previousStoryboard != null)
                previousStoryboard.Pause();

            if(deferredTransition)
            {
                storyboard.BeginTime = Const.AnimationDelay;
            }

            if (true) // useTransition)
            {
                foreach (Object datum in Data.List)
                {
                    Ellipse ellipse = null;
                    Boolean newbie = false;

                    if (index < CircleCanvas.Children.Count)
                    {
                        ellipse = CircleCanvas.Children[index] as Ellipse;
                        ellipse.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        newbie = true;
                        ellipse = new Ellipse();

                        /*ellipse.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
                        {
                            if (CirclePointerPressed != null)
                            {
                                CirclePointerPressed(ellipse, e, datum, index);
                                e.Handled = true;
                            }
                        };

                        ellipse.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
                        {
                            if (CirclePointerReleased != null)
                            {
                                CirclePointerReleased(ellipse, e, datum, index);
                                e.Handled = true;
                            }
                        };*/

                        //ellipse.Tapped += circle_Tapped;

                        CircleCanvas.Children.Add(ellipse);
                    }

                    if (newbie || !useTransition)
                    {
                        Canvas.SetLeft(ellipse, XGetter(datum, index) - RadiusGetter(datum, index) / 2);
                        Canvas.SetTop(ellipse, YGetter(datum, index) - RadiusGetter(datum, index) / 2);
                        ellipse.Opacity = OpacityGetter == null ? 1.0 : OpacityGetter(datum, index);
                    }
                    else
                    {
                        storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "(Canvas.Left)", XGetter(datum, index) - RadiusGetter(datum, index) / 2));
                        storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "(Canvas.Top)", YGetter(datum, index) - RadiusGetter(datum, index) / 2));
                        storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "Opacity", OpacityGetter == null ? 1.0 : OpacityGetter(datum, index)));
                    }                    

                    ellipse.Width = RadiusGetter(datum, index);
                    ellipse.Height = RadiusGetter(datum, index);
                    ellipse.Fill = ColorGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(ColorGetter(datum, index));

                    index++;
                }
                storyboard.Begin();
                previousStoryboard = storyboard;

                for (Int32 i = CircleCanvas.Children.Count - 1; i >= index; --i)
                {
                    Ellipse child = CircleCanvas.Children[i] as Ellipse;
                    child.Visibility = Visibility.Collapsed;
                    //CircleCanvas.Children.Remove(child);
                }
            }
            else
            {
                CircleCanvas.Children.Clear();

                foreach (Object datum in Data.List)
                {
                    Ellipse ellipse = new Ellipse();

                    /*ellipse.PointerPressed += delegate (object sender, PointerRoutedEventArgs e)
                    {
                        if (CirclePointerPressed != null)
                        {
                            CirclePointerPressed(ellipse, e, datum, index);
                            e.Handled = true;
                        }
                    };

                    ellipse.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
                    {
                        if (CirclePointerReleased != null)
                        {
                            CirclePointerReleased(ellipse, e, datum, index);
                            e.Handled = true;
                        }
                    };

                    ellipse.Tapped += circle_Tapped;*/

                    CircleCanvas.Children.Add(ellipse);

                    Canvas.SetLeft(ellipse, XGetter(datum, index) - RadiusGetter(datum, index) / 2);
                    Canvas.SetTop(ellipse, YGetter(datum, index) - RadiusGetter(datum, index) / 2);
                    ellipse.Opacity = OpacityGetter == null ? 1.0 : OpacityGetter(datum, index);
                    ellipse.Width = RadiusGetter(datum, index);
                    ellipse.Height = RadiusGetter(datum, index);
                    ellipse.Fill = ColorGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(ColorGetter(datum, index));

                    index++;
                }
            }
        }
    }
}
