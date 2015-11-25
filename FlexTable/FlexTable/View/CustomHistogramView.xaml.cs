using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Input;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    class BinTick
    {
        public Line Line { get; set; }
        public Double DomainValue { get; set; }
        public Double RangeValue { get; set; }
        public Boolean IsRemovable { get; set; }
        public TextBlock TextBlock { get; set; }
    }

    public sealed partial class CustomHistogramView : UserControl
    {
        public d3.Component.Axis XAxis { get { return AxisElement; } }
        public Crayon.Chart.BarChart BarChart { get { return BarChartElement; } }
        List<BinTick> binTicks = new List<BinTick>();
        Util.Drawable binSplitDrawable = new Util.Drawable()
        {
            IgnoreSmallStrokes = false
        };

        IEnumerable<Double> data;

        DispatcherTimer timer = new DispatcherTimer(){
            Interval = TimeSpan.FromMilliseconds(500)
        };

        IReadOnlyList<InkRecognitionResult> lastResult;
        Double lastCenterX;
        d3.Scale.Linear xScale;

        public CustomHistogramView()
        {
            this.InitializeComponent();
            binSplitDrawable.Attach(DrawableGridElement, StrokeGrid, NewStrokeGrid);
            binSplitDrawable.StrokeAdded += binSplitDrawable_StrokeAdded;
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, object e)
        {
            timer.Stop();

            if (lastResult.Count == 1)
            {
                Boolean isAdding = false;
                foreach (String candidate in lastResult[0].GetTextCandidates())
                {
                    if (candidate == "1" || candidate == "|" || candidate == "l")
                    {
                        isAdding = true;
                        break;
                    }
                }

                if (isAdding)
                {
                    AddBinTick(lastCenterX);
                    return;
                }
            }

            Int32 index=0;
            foreach (InkRecognitionResult result in lastResult)
            {
                Debug.WriteLine("Result {0}", index++);
                foreach (String candidate in result.GetTextCandidates())
                {
                    Double number;

                    if (Double.TryParse(candidate, out number))
                    {
                        EditBinTick(lastCenterX, number);
                        binSplitDrawable.RemoveAllStrokes();
                        return;
                    }
                    Debug.WriteLine(candidate);
                }
            }

            binSplitDrawable.RemoveAllStrokes();
        }

        async void binSplitDrawable_StrokeAdded(InkManager inkManager)
        {
            try
            {
                IReadOnlyList<InkStroke> strokes = inkManager.GetStrokes();
                Double centerX = strokes[0].BoundingRect.X + strokes[0].BoundingRect.Width / 2;
                Boolean isRemoving = false;

                IReadOnlyList<InkRecognitionResult> results = await inkManager.RecognizeAsync(InkRecognitionTarget.All);

                foreach (InkRecognitionResult result in results)
                {
                    foreach (String candidate in result.GetTextCandidates())
                    {
                        if (candidate == "x" || candidate == "X")
                        {
                            isRemoving = true;
                            break;
                        }
                    }
                }

                timer.Stop();

                if (isRemoving) // 2초 기다리지 말고 지워버림
                {
                    RemoveBinTick(centerX);
                    binSplitDrawable.RemoveAllStrokes();
                }
                else
                {
                    lastResult = results;
                    lastCenterX = centerX;
                    timer.Start();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void AddBinTick(Double centerX)
        {
            IEnumerable<BinTick> targets = binTicks.OrderBy(bt => Math.Abs(bt.RangeValue - centerX));
            BinTick target = targets.First();

            if (Math.Abs(target.RangeValue - centerX) < 10) return;

            BinTick binTick = new BinTick()
            {
                DomainValue = xScale.Invert(centerX),
                RangeValue = centerX,
                Line = new Line()
                {
                    Style = this.Resources["BinTickLineStyle"] as Style
                },
                IsRemovable = true,
                TextBlock = new TextBlock()
                {
                    Style = this.Resources["BinTickTextBlockStyle"] as Style,
                    Text = Util.Formatter.FormatAuto3(xScale.Invert(centerX))
                }
            };

            Canvas.SetLeft(binTick.Line, binTick.RangeValue);
            Canvas.SetLeft(binTick.TextBlock, binTick.RangeValue - 50);

            DrawableCanvasElement.Children.Add(binTick.Line);
            DrawableCanvasElement.Children.Add(binTick.TextBlock);

            binTicks.Add(binTick);
            binTicks = binTicks.OrderBy(bt => bt.DomainValue).ToList();

            Storyboard sb = new Storyboard();
            sb.Children.Add(Util.Animator.Generate(binTick.Line, "Opacity", 1));
            sb.Children.Add(Util.Animator.Generate(binTick.TextBlock, "Opacity", 1));
            sb.Begin();

            UpdateBarChart();
            binSplitDrawable.RemoveAllStrokes();
        }

        public void RemoveBinTick(Double centerX)
        {
            IEnumerable<BinTick> targets = binTicks.Where(bt => bt.IsRemovable).OrderBy(bt => Math.Abs(bt.RangeValue - centerX));
            BinTick target;

            if (targets.Count() == 0) return;
            target = targets.First();

            if (Math.Abs(target.RangeValue - centerX) < 50)
            {
                binTicks.Remove(target);
                UpdateBarChart();

                Storyboard sb = new Storyboard();
                sb.Children.Add(Util.Animator.Generate(target.Line, "Opacity", 0));
                sb.Children.Add(Util.Animator.Generate(target.TextBlock, "Opacity", 0));

                sb.Completed += delegate
                {
                    DrawableCanvasElement.Children.Remove(target.Line);
                    DrawableCanvasElement.Children.Remove(target.TextBlock);
                };
                sb.Begin();
            }
        }

        public void EditBinTick(Double centerX, Double number)
        {
            IEnumerable<BinTick> targets = binTicks.Where(bt => bt.IsRemovable).OrderBy(bt => Math.Abs(bt.RangeValue - centerX));
            BinTick target;

            if (number < xScale.DomainStart) return;
            if (number > xScale.DomainEnd) return;
            if (targets.Count() == 0) return;
            target = targets.First();

            BinTick nearestInDomain = binTicks.OrderBy(bt => Math.Abs(bt.DomainValue - number)).First();
            if (target != nearestInDomain && Math.Abs(nearestInDomain.DomainValue - number) < xScale.Step / 10) return;

            if (Math.Abs(target.RangeValue - centerX) < 50)
            {
                target.DomainValue = number;
                target.RangeValue = xScale.Map(number);

                binTicks = binTicks.OrderBy(bt => bt.DomainValue).ToList();

                Storyboard sb = new Storyboard();

                sb.Children.Add(Util.Animator.Generate(target.Line, "(Canvas.Left)", target.RangeValue));
                sb.Children.Add(Util.Animator.Generate(target.TextBlock, "(Canvas.Left)", target.RangeValue - 50));

                sb.Completed += delegate{
                    target.TextBlock.Text = Util.Formatter.FormatAuto3(number);
                };
                sb.Begin();
                
                UpdateBarChart();
            }
        }

        public void Update(IEnumerable<Double> data)
        {
            this.data = data;

            Double min = data.Min(),
                   max = data.Max();

            // min, max 구해서 axis 그리기

            if (min == max)
            {
                min *= 0.9;
                max *= 1.1;
            }
            xScale = new d3.Scale.Linear()
            {
                DomainStart = min,
                DomainEnd = max,
                RangeStart = 40,
                RangeEnd = (Double)App.Current.Resources["ParagraphWidth"] - 40
            };

            xScale.Nice();

            AxisElement.Scale = xScale;
            AxisElement.Update();

            List<Double> pivots = new List<Double>(); 

            Int32 i = 0;
            for (i = 0; i <= (xScale.DomainEnd - xScale.DomainStart) / xScale.Step; ++i)
            {
                pivots.Add(min + i * xScale.Step);
            }
            
            SetBinTicks(pivots);
            UpdateBarChart();            

            // 테이블 하나 그리기

            // 수직선 위에서 나눌수 있도록 입력 받기

            // 수직선 나눠지면 테이블 및 바차트 수정하기

            // 수직선 위에 틱 지우기

            // 수직선 위에 글자 써서 틱 수정하기
        }

        public void UpdateBarChart()
        {
            /*BarChartElement.Data = CreateBins();
            BarChartElement.Update();*/
        }

        public void SetBinTicks(List<Double> pivots)
        {
            d3.Scale.Linear scale = AxisElement.Scale as d3.Scale.Linear;
            Storyboard sb = new Storyboard();

            foreach (BinTick binTick in binTicks)
            {
                Line line = binTick.Line;
                DoubleAnimation da = Util.Animator.Generate(binTick.Line, "Opacity", 0);
                da.Completed += delegate
                {
                    DrawableCanvasElement.Children.Remove(line);
                };
                sb.Children.Add(da);

                TextBlock textBlock = binTick.TextBlock;
                DoubleAnimation da2 = Util.Animator.Generate(textBlock, "Opacity", 0);
                da2.Completed += delegate
                {
                    DrawableCanvasElement.Children.Remove(textBlock);
                };
                sb.Children.Add(da2);
            }

            binTicks.Clear();
            Int32 index = 0;
            foreach (Double pivot in pivots)
            {
                BinTick binTick = new BinTick()
                {
                    DomainValue = pivot,
                    RangeValue = scale.Map(pivot),
                    Line = new Line()
                    {
                        Style = this.Resources["BinTickLineStyle"] as Style
                    },
                    IsRemovable = 0 < index && index < pivots.Count - 1,
                    TextBlock = new TextBlock()
                    {
                        Style = this.Resources["BinTickTextBlockStyle"] as Style,
                        Text = Util.Formatter.FormatAuto3(pivot)
                    }
                };

                Canvas.SetLeft(binTick.Line, binTick.RangeValue);
                //Canvas.SetTop(binTick.TextBlock, -16);
                Canvas.SetLeft(binTick.TextBlock, binTick.RangeValue - 50);

                DrawableCanvasElement.Children.Add(binTick.Line);
                DrawableCanvasElement.Children.Add(binTick.TextBlock);
                binTicks.Add(binTick);

                sb.Children.Add(Util.Animator.Generate(binTick.Line, "Opacity", 1));
                sb.Children.Add(Util.Animator.Generate(binTick.TextBlock, "Opacity", 1));
                index++;
            }

            sb.Begin();
        }

        public List<Tuple<Object, Double>> CreateBins()
        {
            // data와 pivots로 bin을 만든다.

            Int32 i,
                  length = binTicks.Count() - 1;

            List<Tuple<Object, Double>> result = new List<Tuple<Object, Double>>();

            for (i = 0; i < length; ++i)
            {
                Double r1 = binTicks[i].DomainValue,
                       r2 = binTicks[i + 1].DomainValue;

                Int32 count = 0;

                if (i == length - 1) // 마지막 빈이면 
                {
                    count = data.Count(d => r1 <= d && d <= r2); // 끝 구간 포함
                }
                else
                {
                    count = data.Count(d => r1 <= d && d < r2); // 끝 구간 미포함
                }

                String name = String.Format("{0} - {1}", Util.Formatter.FormatAuto3(r1), Util.Formatter.FormatAuto3(r2));

                result.Add(new Tuple<Object, Double>(name, count));
            }

            return result;
       }

        private void DrawableGridElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == PointerDeviceType.Pen)
            {
                e.Handled = true;
            }
        } 
    }
}
