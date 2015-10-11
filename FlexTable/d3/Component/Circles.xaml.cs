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
            DependencyProperty.Register("Data", typeof(Data), typeof(Circles), new PropertyMetadata(null, new PropertyChangedCallback(DataChanged)));
        
        public event Event.EventHandler CirclePointerPressed;
        public event Event.EventHandler CirclePointerReleased;

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

        public Circles()
        {
            this.InitializeComponent();
        }

        private static void DataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
        }

        List<Ellipse> previousCircles = new List<Ellipse>();

        Storyboard previousStoryboard;

        public void Update()
        {
            Int32 index = 0;
            Storyboard storyboard = new Storyboard()
            {
                BeginTime = TimeSpan.FromMilliseconds(100)
            };
        
            if (previousStoryboard != null)
                previousStoryboard.Pause();

            foreach (Object datum in Data.List)
            {
                Ellipse ellipse = null;
                Boolean newbie = false;

                if (index < CirclesCanvas.Children.Count)
                {
                    ellipse = CirclesCanvas.Children[index] as Ellipse;
                }
                else
                {
                    newbie = true;
                    ellipse = new Ellipse()
                    {
                    };

                    ellipse.PointerPressed += delegate(object sender, PointerRoutedEventArgs e)
                    {
                        if (CirclePointerPressed != null)
                        {
                            CirclePointerPressed(ellipse, datum);
                            e.Handled = true;
                        }
                    };

                    ellipse.PointerReleased += delegate(object sender, PointerRoutedEventArgs e)
                    {
                        if (CirclePointerReleased != null)
                        {
                            CirclePointerReleased(ellipse, datum);
                            e.Handled = true;
                        }
                    };

                    ellipse.Tapped += circle_Tapped;

                    CirclesCanvas.Children.Add(ellipse);
                    previousCircles.Add(ellipse);
                }

                if (newbie)
                {
                    Canvas.SetLeft(ellipse, XGetter(datum, index) - RadiusGetter(datum, index) / 2);
                    Canvas.SetTop(ellipse, YGetter(datum, index) - RadiusGetter(datum, index) / 2);
                }
                else
                {
                    storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "(Canvas.Left)", XGetter(datum, index) - RadiusGetter(datum, index) / 2));
                    storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "(Canvas.Top)", YGetter(datum, index) - RadiusGetter(datum, index) / 2));   
                }
                
                
                ellipse.Width = RadiusGetter(datum, index);
                ellipse.Height = RadiusGetter(datum, index);
                ellipse.Fill = ColorGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(ColorGetter(datum, index));
                ellipse.Opacity = OpacityGetter == null ? 1.0 : OpacityGetter(datum, index);

                
                index++;
            }
            storyboard.Begin();
            previousStoryboard = storyboard;

            for (Int32 i = CirclesCanvas.Children.Count - 1; i >= index; --i)
            {
                Ellipse child = CirclesCanvas.Children[i] as Ellipse;
                CirclesCanvas.Children.Remove(child);
                previousCircles.Remove(child);
            }
        }

        void circle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
