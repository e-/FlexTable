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

        /*public static readonly DependencyProperty ReusePreviousElementProperty =
            DependencyProperty.Register("ReusePreviousElement", typeof(Boolean), typeof(Circles), new PropertyMetadata(true)
                );

        public Boolean ReusePreviousElement
        {
            get { return (Boolean)GetValue(ReusePreviousElementProperty); }
            set { SetValue(ReusePreviousElementProperty, value); }
        } */      

        public Circles()
        {
            this.InitializeComponent();
        }        

        Storyboard previousStoryboard;
        public void Update(TransitionType transitionType)
        {
            Int32 index = 0;
            Storyboard storyboard = new Storyboard();

            if (previousStoryboard != null)
                previousStoryboard.Pause();

        /*    if(deferredTransition)
            {
                storyboard.BeginTime = Const.AnimationDelay;
            }*/

            foreach (Object datum in Data.List)
            {
                Ellipse ellipse = null;

                if (index < CircleCanvas.Children.Count)
                {
                    ellipse = CircleCanvas.Children[index] as Ellipse;
                    ellipse.Visibility = Visibility.Visible;
                    ellipse.Fill = ColorGetter == null ? new SolidColorBrush(Colors.LightGray) : new SolidColorBrush(ColorGetter(datum, index));

                    Double newRadius = RadiusGetter(datum, index);
                    /*if (transitionType.HasFlag(TransitionType.Size))
                    {
                        if (ellipse.Height != newRadius)
                        {
                            storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "Height", newRadius, true));
                            storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "Width", newRadius, true));
                        }
                    }
                    else
                    {*/
                        ellipse.Height = newRadius;
                        ellipse.Width = newRadius;
                    //}


                    if (transitionType.HasFlag(TransitionType.Position))
                    {
                        Double newX = XGetter(datum, index) - newRadius / 2;
                        Double newY = YGetter(datum, index) - newRadius / 2;

                        if ((ellipse.RenderTransform as TranslateTransform).X != newX)
                            storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse.RenderTransform, "X", newX));
                        if((ellipse.RenderTransform as TranslateTransform).Y != newY)
                            storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse.RenderTransform, "Y", newY));
                    }
                    else
                    {
                        (ellipse.RenderTransform as TranslateTransform).X = XGetter(datum, index) - newRadius / 2;
                        (ellipse.RenderTransform as TranslateTransform).Y = YGetter(datum, index) - newRadius / 2;
                    }

                    if (transitionType.HasFlag(TransitionType.Opacity))
                    {
                        if (OpacityGetter != null && OpacityGetter(datum, index) != ellipse.Opacity)
                        {
                            storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "Opacity", OpacityGetter == null ? 1.0 : OpacityGetter(datum, index)));
                        }
                    }
                    else
                    {
                        ellipse.Opacity = OpacityGetter == null ? 1.0 : OpacityGetter(datum, index);
                    }
                }
                else
                {
                    ellipse = new Ellipse()
                    {
                        RenderTransform = new TranslateTransform(),
                        Width = RadiusGetter(datum, index),
                        Height = RadiusGetter(datum, index),
                        Opacity = 0
                    };
                    //처음 추가는 opacity만 빼고 바로 설정

                    (ellipse.RenderTransform as TranslateTransform).X = XGetter(datum, index) - RadiusGetter(datum, index) / 2;
                    (ellipse.RenderTransform as TranslateTransform).Y = YGetter(datum, index) - RadiusGetter(datum, index) / 2;

                    if (transitionType.HasFlag(TransitionType.Opacity))
                    {
                        storyboard.Children.Add(Util.GenerateDoubleAnimation(ellipse, "Opacity", OpacityGetter == null ? 1.0 : OpacityGetter(datum, index)));
                    }
                    else
                    {
                        ellipse.Opacity = OpacityGetter == null ? 1.0 : OpacityGetter(datum, index);
                    }

                    CircleCanvas.Children.Add(ellipse);
                }

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
    }
}
