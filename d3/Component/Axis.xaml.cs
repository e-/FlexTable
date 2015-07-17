﻿using System;
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
using Windows.UI.Xaml.Media.Animation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace d3.Component
{
    public sealed partial class Axis : UserControl
    {
        public enum Orientations
        {
            Horizontal,
            Vertical
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientations), typeof(Axis), new PropertyMetadata(Orientations.Horizontal, new PropertyChangedCallback(OrientationChanged)));

        public Orientations Orientation
        {
            get { return (Orientations)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty ScaleProperty = 
            DependencyProperty.Register("Scale", typeof(d3.Scale.ScaleBase), typeof(Axis), new PropertyMetadata(new Linear(), new PropertyChangedCallback(ScaleChanged)));

        private ScaleBase previousScale = new Linear();
        public ScaleBase PreviousScale { get { return previousScale; } set { previousScale = value; } }

        public ScaleBase Scale
        {
            get { return (ScaleBase)GetValue(ScaleProperty); }
            set {
                SetValue(ScaleProperty, value); 
            }
        }

        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register("Transition", typeof(Boolean), typeof(Axis), new PropertyMetadata(true, new PropertyChangedCallback(OrientationChanged)));

        public Boolean Transition
        {
            get { return (Boolean)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        private DependencyProperty linePrimary1, linePrimary2, lineSecondary1, lineSecondary2;
        private DependencyProperty canvasPrimary, canvasSecondary;
        private DependencyProperty tickLabelSizeProperty;
        private String canvasPrimaryString;

        private readonly TimeSpan Duration = TimeSpan.FromMilliseconds(500);
        private readonly EasingFunctionBase EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut };

        public Axis()
        {
            this.InitializeComponent();
            UpdateOrientation();
        }

        private static void OrientationChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Axis axis = source as Axis;
            axis.UpdateOrientation();
        }

        private static void ScaleChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Axis axis = source as Axis;
            axis.PreviousScale = e.OldValue as ScaleBase;
            axis.Update();
        }

        public void UpdateOrientation()
        {
            switch (Orientation)
            {
                case Orientations.Horizontal:
                    linePrimary1 = Line.X1Property;
                    linePrimary2 = Line.X2Property;
                    lineSecondary1 = Line.Y1Property;
                    lineSecondary2 = Line.Y2Property;
                    canvasPrimary = Canvas.LeftProperty;
                    canvasSecondary = Canvas.TopProperty;
                    tickLabelSizeProperty = TextBlock.ActualWidthProperty;
                    canvasPrimaryString = "(Canvas.Left)";
                    break;

                case Orientations.Vertical:
                    linePrimary1 = Line.Y1Property;
                    linePrimary2 = Line.Y2Property;
                    lineSecondary1 = Line.X1Property;
                    lineSecondary2 = Line.X2Property;
                    canvasPrimary = Canvas.TopProperty;
                    canvasSecondary = Canvas.LeftProperty;
                    tickLabelSizeProperty = TextBlock.ActualHeightProperty;
                    canvasPrimaryString = "(Canvas.Top)";
                    break;
            }
        }

        private List<TextBlock> tickLabels;

        public void Update()
        {
            Int32 tickCount = Scale.TickCount;

            AxisLine.SetValue(linePrimary1, Scale.RangeStart);
            AxisLine.SetValue(linePrimary2, Scale.RangeEnd);
            AxisLine.SetValue(lineSecondary1, 0);
            AxisLine.SetValue(lineSecondary2, 0);

            if (Transition)
            {
                List<TextBlock> previousTickLabels = tickLabels;
                Storyboard tickLabelsStoryboard = new Storyboard();
                //remove previous ticks
                if (previousTickLabels != null)
                {
                    Int32 index = 0;
                    List<Tick> ticks = previousScale.GetTicks();
                    foreach (TextBlock tickLabel in previousTickLabels)
                    {
                        Tick tick = ticks[index];
                        DoubleAnimation positionAnimation = new DoubleAnimation()
                        {
                            To = Scale.ClampedMap(tick.DomainValue) - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2,
                            Duration = Duration,
                            EasingFunction = EasingFunction
                        };
                        Storyboard.SetTarget(positionAnimation, tickLabel);
                        Storyboard.SetTargetProperty(positionAnimation, canvasPrimaryString);

                        if (previousScale.GetType() == Scale.GetType() && !(Scale is Scale.Ordinal)) // 같고 ordinal이 아니어야 (linear)야 position animation 가능
                        {
                            tickLabelsStoryboard.Children.Add(positionAnimation);
                        }

                        DoubleAnimation opacityAnimation = new DoubleAnimation()
                        {
                            To = 0,
                            Duration = Duration,
                            EasingFunction = EasingFunction
                        };
                        Storyboard.SetTarget(opacityAnimation, tickLabel);
                        Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

                        tickLabel.SetValue(canvasSecondary, Orientation == Orientations.Horizontal ? 3 : -tickLabel.ActualWidth - 10);

                        tickLabelsStoryboard.Children.Add(opacityAnimation);
                        index++;
                    }
                }


                //add new ticks
                tickLabels = new List<TextBlock>();
                foreach (Tick tick in Scale.GetTicks())
                {
                    TextBlock tickLabel = new TextBlock()
                    {
                        Text = tick.Label,
                        Style = Resources["TickLabelStyle"] as Style,
                        Opacity = 0
                    };

                    AxisCanvas.Children.Add(tickLabel);
                    tickLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));

                    tickLabel.SetValue(canvasPrimary, previousScale.ClampedMap(tick.DomainValue) - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2);
                    tickLabel.SetValue(canvasSecondary, Orientation == Orientations.Horizontal ? 3 : -tickLabel.ActualWidth - 10);

                    DoubleAnimation positionAnimation = new DoubleAnimation()
                    {
                        To = tick.RangeValue - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2,
                        Duration = Duration,
                        EasingFunction = EasingFunction
                    };
                    Storyboard.SetTarget(positionAnimation, tickLabel);
                    Storyboard.SetTargetProperty(positionAnimation, canvasPrimaryString);

                    if (previousScale.GetType() != Scale.GetType() || Scale is Scale.Ordinal) // position animation disabled because two scales have different types
                    {
                        tickLabel.SetValue(canvasPrimary, positionAnimation.To);
                    }

                    DoubleAnimation opacityAnimation = new DoubleAnimation()
                    {
                        To = 1,
                        Duration = Duration,
                        EasingFunction = EasingFunction
                    };
                    Storyboard.SetTarget(opacityAnimation, tickLabel);
                    Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

                    tickLabelsStoryboard.Children.Add(positionAnimation);
                    tickLabelsStoryboard.Children.Add(opacityAnimation);

                    tickLabels.Add(tickLabel);
                }

                tickLabelsStoryboard.Completed += delegate
                {
                    if (previousTickLabels != null)
                    {
                        foreach (TextBlock tickLabel in previousTickLabels)
                        {
                            AxisCanvas.Children.Remove(tickLabel);
                        }
                    }
                };
                tickLabelsStoryboard.Begin();
            }
            else
            {
                List<TextBlock> previousTickLabels = tickLabels;
                if (previousTickLabels != null)
                {
                    foreach (TextBlock tickLabel in previousTickLabels)
                    {
                        AxisCanvas.Children.Remove(tickLabel);
                    }
                }

                //add new ticks
                tickLabels = new List<TextBlock>();
                foreach (Tick tick in Scale.GetTicks())
                {
                    TextBlock tickLabel = new TextBlock()
                    {
                        Text = tick.Label,
                        Style = Resources["TickLabelStyle"] as Style,
                        Opacity = 1
                    };

                    AxisCanvas.Children.Add(tickLabel);
                    tickLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));

                    tickLabel.SetValue(canvasPrimary, previousScale.ClampedMap(tick.DomainValue) - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2);
                    tickLabel.SetValue(canvasSecondary, Orientation == Orientations.Horizontal ? 3 : -tickLabel.ActualWidth - 10);
                    tickLabel.SetValue(canvasPrimary, tick.RangeValue - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2);
                    tickLabels.Add(tickLabel);
                }
            }
        }
    }
}