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
                //previousScale = Scale;
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

        public static readonly DependencyProperty LabelOpacityGetterProperty =
            DependencyProperty.Register("LabelOpacityGetter", typeof(Func<TextBlock, Double>), typeof(Axis), new PropertyMetadata(default(Func<TextBlock, Double>)));

        public Func<TextBlock, Double> LabelOpacityGetter
        {
            get { return (Func<TextBlock, Double>)GetValue(LabelOpacityGetterProperty); }
            set { SetValue(LabelOpacityGetterProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeGetterProperty =
            DependencyProperty.Register("LabelFontSizeGetter", typeof(Func<TextBlock, Double, Double>), typeof(Axis), new PropertyMetadata(default(Func<TextBlock, Double, Double>)));

        public Func<TextBlock, Double, Double> LabelFontSizeGetter
        {
            get { return (Func<TextBlock, Double, Double>)GetValue(LabelFontSizeGetterProperty); }
            set { SetValue(LabelFontSizeGetterProperty, value); }
        }


        private DependencyProperty linePrimary1, linePrimary2, lineSecondary1, lineSecondary2;
        private DependencyProperty canvasPrimary, canvasSecondary;
        private DependencyProperty tickLabelSizeProperty;
        private String canvasPrimaryString;
        private String linePrimary1String;
        private String linePrimary2String;

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
            //axis.Update();
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
                    linePrimary1String = "X1";
                    linePrimary2String = "X2";
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
                    linePrimary1String = "Y1";
                    linePrimary2String = "Y2";
                    break;
            }
        }

        private List<TextBlock> tickLabels;
        private List<Line> tickMarkers;

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
                List<Line> previousTickMarkers = tickMarkers;

                Storyboard tickLabelsStoryboard = new Storyboard()
                {
                    BeginTime = Const.AnimationDelay
                };
                //remove previous ticks
                if (previousTickLabels != null)
                {
                    Int32 index = 0;
                    List<Tick> ticks = previousScale.GetTicks();
                    foreach (TextBlock tickLabel in previousTickLabels)
                    {
                        Tick tick = ticks[index];
                        Line tickMarker = previousTickMarkers[index];

                        DoubleAnimation positionAnimation = new DoubleAnimation()
                        {
                            To = Scale.ClampedMap(tick.DomainValue) - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2,
                            Duration = Duration,
                            EasingFunction = EasingFunction
                        };
                        Storyboard.SetTarget(positionAnimation, tickLabel);
                        Storyboard.SetTargetProperty(positionAnimation, canvasPrimaryString);

                        DoubleAnimation positionAnimation1 = new DoubleAnimation()
                        {
                            To = Scale.ClampedMap(tick.DomainValue),
                            Duration = Duration,
                            EasingFunction = EasingFunction,
                            EnableDependentAnimation = true
                        };
                        Storyboard.SetTarget(positionAnimation1, tickMarker);
                        Storyboard.SetTargetProperty(positionAnimation1, linePrimary1String);

                        DoubleAnimation positionAnimation2 = new DoubleAnimation()
                        {
                            To = Scale.ClampedMap(tick.DomainValue),
                            Duration = Duration,
                            EasingFunction = EasingFunction,
                            EnableDependentAnimation = true
                        };
                        Storyboard.SetTarget(positionAnimation2, tickMarker);
                        Storyboard.SetTargetProperty(positionAnimation2, linePrimary2String);


                        if (previousScale.GetType() == Scale.GetType() && !(Scale is Scale.Ordinal)) // 같고 ordinal이 아니어야 (linear)야 position animation 가능
                        {
                            tickLabelsStoryboard.Children.Add(positionAnimation);
                            tickLabelsStoryboard.Children.Add(positionAnimation1);
                            tickLabelsStoryboard.Children.Add(positionAnimation2);
                        }

                        DoubleAnimation opacityAnimation = new DoubleAnimation()
                        {
                            To = 0,
                            Duration = Duration,
                            EasingFunction = EasingFunction
                        };
                        Storyboard.SetTarget(opacityAnimation, tickLabel);
                        Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

                        DoubleAnimation opacityAnimation2 = new DoubleAnimation()
                        {
                            To = 0,
                            Duration = Duration,
                            EasingFunction = EasingFunction
                        };
                        Storyboard.SetTarget(opacityAnimation2, tickMarker);
                        Storyboard.SetTargetProperty(opacityAnimation2, "Opacity");

                        tickLabel.SetValue(canvasSecondary, Orientation == Orientations.Horizontal ? (24 - tickLabel.ActualHeight) / 2 : -tickLabel.ActualWidth - 10);

                        tickLabelsStoryboard.Children.Add(opacityAnimation);
                        tickLabelsStoryboard.Children.Add(opacityAnimation2);
                        index++;
                    }
                }


                //add new ticks
                tickLabels = new List<TextBlock>();
                tickMarkers = new List<Line>();
                foreach (Tick tick in Scale.GetTicks())
                {
                    TextBlock tickLabel = new TextBlock()
                    {
                        Text = tick.Label,
                        Style = Resources["TickLabelStyle"] as Style,
                        Opacity = 0
                    };

                    Line tickMarker = new Line()
                    {
                        Style = Resources["TickMarkerStyle"] as Style,
                        Opacity = 0
                    };

                    AxisCanvas.Children.Add(tickLabel);
                    tickLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));
                    if (LabelFontSizeGetter != null)
                    {
                        tickLabel.FontSize = LabelFontSizeGetter(tickLabel, tickLabel.FontSize);
                        tickLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));
                    }
                    else if(Orientation == Orientations.Vertical)
                    {
                        if(tickLabel.ActualWidth > 28)
                        {
                            tickLabel.FontSize = tickLabel.FontSize * 28 / tickLabel.ActualWidth;
                            tickLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));
                        }
                    }

                    tickLabel.SetValue(canvasPrimary, previousScale.ClampedMap(tick.DomainValue) - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2); // excpetion 발생함 왜이렇지? TODO
                    tickLabel.SetValue(canvasSecondary, Orientation == Orientations.Horizontal ? (24 - tickLabel.ActualHeight) / 2 : -tickLabel.ActualWidth - 10);

                    AxisCanvas.Children.Add(tickMarker);
                    tickMarker.SetValue(linePrimary1, previousScale.ClampedMap(tick.DomainValue));
                    tickMarker.SetValue(linePrimary2, previousScale.ClampedMap(tick.DomainValue));
                    tickMarker.SetValue(lineSecondary1, 0);
                    tickMarker.SetValue(lineSecondary2, Orientation == Orientations.Horizontal ? 3 : -3);

                    DoubleAnimation positionAnimation = new DoubleAnimation()
                    {
                        To = tick.RangeValue - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2,
                        Duration = Duration,
                        EasingFunction = EasingFunction
                    };
                    Storyboard.SetTarget(positionAnimation, tickLabel);
                    Storyboard.SetTargetProperty(positionAnimation, canvasPrimaryString);

                    DoubleAnimation positionAnimation1 = new DoubleAnimation()
                    {
                        To = tick.RangeValue,
                        Duration = Duration,
                        EasingFunction = EasingFunction,
                        EnableDependentAnimation = true
                    };
                    Storyboard.SetTarget(positionAnimation1, tickMarker);
                    Storyboard.SetTargetProperty(positionAnimation1, linePrimary1String);

                    DoubleAnimation positionAnimation2 = new DoubleAnimation()
                    {
                        To = tick.RangeValue,
                        Duration = Duration,
                        EasingFunction = EasingFunction,
                        EnableDependentAnimation = true
                    };
                    Storyboard.SetTarget(positionAnimation2, tickMarker);
                    Storyboard.SetTargetProperty(positionAnimation2, linePrimary2String);

                    if (previousScale.GetType() != Scale.GetType() || Scale is Ordinal) // position animation disabled because two scales have different types
                    {
                        tickLabel.SetValue(canvasPrimary, positionAnimation.To);
                        tickMarker.SetValue(linePrimary1, positionAnimation1.To);
                        tickMarker.SetValue(linePrimary2, positionAnimation2.To);
                    }

                    DoubleAnimation opacityAnimation = new DoubleAnimation()
                    {
                        To = LabelOpacityGetter == null ? 1 : LabelOpacityGetter(tickLabel),
                        Duration = Duration,
                        EasingFunction = EasingFunction
                    };
                    Storyboard.SetTarget(opacityAnimation, tickLabel);
                    Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

                    DoubleAnimation opacityAnimation2 = new DoubleAnimation()
                    {
                        To = 1,
                        Duration = Duration,
                        EasingFunction = EasingFunction
                    };
                    Storyboard.SetTarget(opacityAnimation2, tickMarker);
                    Storyboard.SetTargetProperty(opacityAnimation2, "Opacity");

                    tickLabelsStoryboard.Children.Add(positionAnimation);
                    tickLabelsStoryboard.Children.Add(positionAnimation1);
                    tickLabelsStoryboard.Children.Add(positionAnimation2);
                    tickLabelsStoryboard.Children.Add(opacityAnimation);
                    tickLabelsStoryboard.Children.Add(opacityAnimation2);

                    tickLabels.Add(tickLabel);
                    tickMarkers.Add(tickMarker);
                }

                tickLabelsStoryboard.Completed += delegate
                {
                    if (previousTickLabels != null)
                    {
                        foreach (TextBlock tickLabel in previousTickLabels)
                        {
                            AxisCanvas.Children.Remove(tickLabel);
                        }

                        foreach (Line tickMarker in previousTickMarkers)
                        {
                            AxisCanvas.Children.Remove(tickMarker);
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
                    if (LabelFontSizeGetter != null)
                    {
                        tickLabel.FontSize = LabelFontSizeGetter(tickLabel, tickLabel.FontSize);
                        tickLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));
                    }

                    tickLabel.SetValue(canvasPrimary, previousScale.ClampedMap(tick.DomainValue) - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2);
                    tickLabel.SetValue(canvasSecondary, Orientation == Orientations.Horizontal ? (24 - tickLabel.ActualHeight) / 2 : -tickLabel.ActualWidth - 10);

                    tickLabel.SetValue(canvasPrimary, tick.RangeValue - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2);
                    tickLabels.Add(tickLabel);
                }
            }

            previousScale = Scale.Clone();
        }
    }
}
