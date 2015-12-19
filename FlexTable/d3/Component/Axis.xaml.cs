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

       /* public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register("Transition", typeof(Boolean), typeof(Axis), new PropertyMetadata(true, new PropertyChangedCallback(OrientationChanged)));

        public Boolean Transition
        {
            get { return (Boolean)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }*/

        public static readonly DependencyProperty LabelOpacityGetterProperty =
            DependencyProperty.Register("LabelOpacityGetter", typeof(Func<Object, Int32, TextBlock, Double>), typeof(Axis), new PropertyMetadata(default(Func<Object, Int32, TextBlock, Double>)));

        public Func<Object, Int32, TextBlock, Double> LabelOpacityGetter
        {
            get { return (Func<Object, Int32, TextBlock, Double>)GetValue(LabelOpacityGetterProperty); }
            set { SetValue(LabelOpacityGetterProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeGetterProperty =
            DependencyProperty.Register("LabelFontSizeGetter", typeof(Func<TextBlock, Double, Double>), typeof(Axis), new PropertyMetadata(default(Func<TextBlock, Double, Double>)));

        public Func<TextBlock, Double, Double> LabelFontSizeGetter
        {
            get { return (Func<TextBlock, Double, Double>)GetValue(LabelFontSizeGetterProperty); }
            set { SetValue(LabelFontSizeGetterProperty, value); }
        }

        public static readonly DependencyProperty LabelYGetterProperty =
            DependencyProperty.Register("LabelYGetter", typeof(Func<Object, Int32, TextBlock, Double>), typeof(Axis), new PropertyMetadata(default(Func<Object, Int32, TextBlock, Double>)));

        public Func<Object, Int32, TextBlock, Double> LabelYGetter
        {
            get { return (Func<Object, Int32, TextBlock, Double>)GetValue(LabelYGetterProperty); }
            set { SetValue(LabelYGetterProperty, value); }
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
                    //tickLabelSizeProperty = TextBlock.WidthProperty;
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
                    //tickLabelSizeProperty = TextBlock.HeightProperty;
                    canvasPrimaryString = "(Canvas.Top)";
                    linePrimary1String = "Y1";
                    linePrimary2String = "Y2";
                    break;
            }
        }

        private List<TextBlock> tickLabels;
        public IEnumerable<TextBlock> TickLabels
        {
            get { return tickLabels; }
        }

        private List<Line> tickMarkers;

        Boolean Equal(ScaleBase s1, ScaleBase s2)
        {
            if (s1.GetType() != s2.GetType()) return false;

            if (s1 is Linear)
            {
                var ss1 = s1 as Linear;
                var ss2 = s2 as Linear;

                return ss1.DomainStart == ss2.DomainStart && ss1.DomainEnd == ss2.DomainEnd && ss1.RangeStart == ss2.RangeStart && ss1.RangeEnd == ss2.RangeEnd && ss1.TickCount == ss2.TickCount;
            }
            else if (s1 is Ordinal)
            {
                var ss1 = s1 as Ordinal;
                var ss2 = s2 as Ordinal;

                if (ss1.Domain.Count != ss2.Domain.Count) return false;
                Int32 n = ss1.Domain.Count, i;
                for(i=0;i< n; ++i)
                {
                    if (ss1.Domain[i] != ss2.Domain[i]) return false;
                }
                return ss1.RangeStart == ss2.RangeStart && ss1.RangeEnd == ss2.RangeEnd;
            }

            return false;
        }

        public void UpdateWithSameScale(Boolean useTransition)
        {
            // tickLabels
            // tickMarkers

            Int32 i;
            List<Tick> ticks = Scale.GetTicks();

            Storyboard axisStoryboard = new Storyboard()
            {
            };

            for (i = 0; i < tickLabels.Count; ++i)
            {
                TextBlock label = tickLabels[i];
                Line marker = tickMarkers[i];
                Tick tick = ticks[i];

                if (LabelOpacityGetter != null)
                {
                    if (useTransition)
                    {
                        axisStoryboard.Children.Add(Util.GenerateDoubleAnimation(label, "Opacity", LabelOpacityGetter(tick.DomainValue, i, label)));
                    }
                    else
                    {
                        label.Opacity = LabelOpacityGetter(tick.DomainValue, i, label);
                    }
                }

                if (LabelYGetter != null)
                {
                    Double original = Orientation == Orientations.Horizontal ? (24 - label.ActualHeight) / 2 : -label.ActualWidth - 10;
                    if (useTransition)
                    {
                        axisStoryboard.Children.Add(Util.GenerateDoubleAnimation(label, "(Canvas.Top)", original + LabelYGetter(tick.DomainValue, i, label)));
                    }
                    else
                    {
                        Canvas.SetTop(label, original + LabelYGetter(tick.DomainValue, i, label));
                    }
                }
            }

            axisStoryboard.Begin();
        }

        public void Update(Boolean useTransition)
        {
            if (Equal(previousScale, Scale))
            {
                UpdateWithSameScale(useTransition);
                return;
            }

            AxisLine.SetValue(linePrimary1, Scale.RangeStart);
            AxisLine.SetValue(linePrimary2, Scale.RangeEnd);
            AxisLine.SetValue(lineSecondary1, 0);
            AxisLine.SetValue(lineSecondary2, 0);

            List<TextBlock> previousTickLabels = tickLabels;
            List<Line> previousTickMarkers = tickMarkers;

            Int32 tickCount = Scale.TickCount;

            Storyboard axisStoryboard = new Storyboard()
            {
                //BeginTime = Const.AnimationDelay
            };

            #region remove previous ticks

            if (previousTickLabels != null)
            {
                if (useTransition)
                {
                    Int32 index = 0;
                    List<Tick> ticks = previousScale.GetTicks();
                    foreach (TextBlock tickLabel in previousTickLabels)
                    {
                        Tick tick = ticks[index];
                        Line tickMarker = previousTickMarkers[index];

                        if (previousScale.GetType() == Scale.GetType() && !(Scale is Ordinal)) // 같고 ordinal이 아니어야 (linear)야 position animation 가능
                        {
                            // text block animation
                            axisStoryboard.Children.Add(
                                Util.GenerateDoubleAnimation(tickLabel, canvasPrimaryString, Scale.ClampedMap(tick.DomainValue) - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2)
                            );

                            // tick marker animation 1 & 2 (for two Xs or Ys)
                            axisStoryboard.Children.Add(
                                Util.GenerateDoubleAnimation(tickMarker, linePrimary1String, Scale.ClampedMap(tick.DomainValue), true)
                                );

                            axisStoryboard.Children.Add(
                                Util.GenerateDoubleAnimation(tickMarker, linePrimary2String, Scale.ClampedMap(tick.DomainValue), true)
                                );
                        }

                        // make text block opacity 0
                        axisStoryboard.Children.Add(
                            Util.GenerateDoubleAnimation(tickLabel, "Opacity", 0)
                            );

                        // make tick marker opacity 0
                        axisStoryboard.Children.Add(
                            Util.GenerateDoubleAnimation(tickMarker, "Opacity", 0)
                            );

                        tickLabel.SetValue(canvasSecondary, Orientation == Orientations.Horizontal ? (24 - tickLabel.ActualHeight) / 2 : -tickLabel.ActualWidth - 10);

                        index++;
                    }
                }
                else
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
            }

            #endregion

            #region add new ticks

            tickLabels = new List<TextBlock>();
            tickMarkers = new List<Line>();

            var currentTicks = Scale.GetTicks();

            foreach (Tick tick in currentTicks)
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

                if(Orientation == Orientations.Vertical)
                {
                    if(tickLabel.ActualWidth > 28)
                    {
                        tickLabel.FontSize = tickLabel.FontSize * 28 / tickLabel.ActualWidth;
                        tickLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));
                    }
                    tickLabel.Height = tickLabel.ActualHeight;
                }
                else if(Orientation == Orientations.Horizontal)
                {
                    tickLabel.MaxWidth = Math.Abs(Scale.RangeEnd - Scale.RangeStart) / currentTicks.Count;
                    tickLabel.Measure(new Size(Double.MaxValue, Double.MaxValue));
                    //tickLabel.Width = tickLabel.ActualWidth;
                }

                tickLabel.SetValue(canvasPrimary, previousScale.ClampedMap(tick.DomainValue) - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2);
                tickLabel.SetValue(canvasSecondary, Orientation == Orientations.Horizontal ? (24 - tickLabel.ActualHeight) / 2 : -tickLabel.ActualWidth - 10);

                AxisCanvas.Children.Add(tickMarker);

                tickMarker.SetValue(linePrimary1, previousScale.ClampedMap(tick.DomainValue));
                tickMarker.SetValue(linePrimary2, previousScale.ClampedMap(tick.DomainValue));
                tickMarker.SetValue(lineSecondary1, 0);
                tickMarker.SetValue(lineSecondary2, Orientation == Orientations.Horizontal ? 3 : -3);

                if (!useTransition || (previousScale.GetType() != Scale.GetType() || Scale is Ordinal)) // position animation disabled because two scales have different types
                {
                    tickLabel.SetValue(canvasPrimary, tick.RangeValue - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2);
                    tickMarker.SetValue(linePrimary1, tick.RangeValue);
                    tickMarker.SetValue(linePrimary2, tick.RangeValue);
                }
                else
                {
                    axisStoryboard.Children.Add(
                        Util.GenerateDoubleAnimation(tickLabel, canvasPrimaryString, tick.RangeValue - (Double)tickLabel.GetValue(tickLabelSizeProperty) / 2)
                        );

                    axisStoryboard.Children.Add(
                        Util.GenerateDoubleAnimation(tickMarker, linePrimary1String, tick.RangeValue, true)
                        );

                    axisStoryboard.Children.Add(
                        Util.GenerateDoubleAnimation(tickMarker, linePrimary2String, tick.RangeValue, true)
                        );
                }

                if (useTransition)
                {
                    axisStoryboard.Children.Add(
                        Util.GenerateDoubleAnimation(tickLabel, "Opacity", LabelOpacityGetter == null ? 1 : LabelOpacityGetter(tick.DomainValue, 0, tickLabel))
                        );

                    axisStoryboard.Children.Add(
                        Util.GenerateDoubleAnimation(tickMarker, "Opacity", 1)
                        );
                }
                else
                {
                    tickLabel.Opacity = LabelOpacityGetter == null ? 1 : LabelOpacityGetter(tick.DomainValue, 0, tickLabel);
                    tickMarker.Opacity = 1;
                }

                tickLabels.Add(tickLabel);
                tickMarkers.Add(tickMarker);
            }
            #endregion

            axisStoryboard.Completed += delegate
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

            axisStoryboard.Begin();

            previousScale = Scale.Clone();
        }
    }
}
