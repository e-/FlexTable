using System.Diagnostics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using FlexTable.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using System;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Input;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace FlexTable.View
{
    public sealed partial class ColumnIndexer : UserControl
    {
        TableViewModel TableViewModel => this.DataContext as TableViewModel;

        public ColumnIndexer()
        {
            this.InitializeComponent();
        }
        
        public static Point CalculateCartesianCoordinate(Double angle, Double radius)
        {
            return new Point(radius * Math.Sin(angle), -radius * Math.Cos(angle));
        }

        public Path DrawArc(Double radius, Double startAngle, Double endAngle)
        {
            Boolean largeArc = endAngle - startAngle > 180.0;
            Size outerArcSize = new Size(radius, radius);
            Size innerArcSize = new Size(0, 0);

            Point innerArcStartPoint = CalculateCartesianCoordinate(startAngle, 0);
            Point BottomLineEndPoint = CalculateCartesianCoordinate(startAngle, radius);
            Point OuterArcEndPoint = CalculateCartesianCoordinate(endAngle, radius);
            Point EndLineEndPoint = CalculateCartesianCoordinate(endAngle, 0);

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = innerArcStartPoint;

            ArcSegment InnerArc = new ArcSegment();
            InnerArc.Size = innerArcSize;
            InnerArc.SweepDirection = SweepDirection.Counterclockwise;
            InnerArc.Point = innerArcStartPoint;
            InnerArc.IsLargeArc = largeArc;

            LineSegment ButtomLine = new LineSegment();
            ButtomLine.Point = BottomLineEndPoint;

            ArcSegment OuterArc = new ArcSegment();
            OuterArc.SweepDirection = SweepDirection.Clockwise;
            OuterArc.Point = OuterArcEndPoint;
            OuterArc.Size = outerArcSize;
            OuterArc.IsLargeArc = largeArc;

            LineSegment EndLine = new LineSegment();
            EndLine.Point = EndLineEndPoint;
            pathFigure.Segments.Add(ButtomLine);
            pathFigure.Segments.Add(OuterArc);
            pathFigure.Segments.Add(EndLine);
            pathFigure.Segments.Add(InnerArc);

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);


            Path path = new Path();
            pathFigure.IsFilled = true;
            pathFigure.IsClosed = true;

            path.Data = pathGeometry;
            
            path.IsHitTestVisible = true;

            return path;
        }

        const Double AngleSpan = Math.PI / 2 / 9 * 6;

        public void Update() // 처음에 초기화하거나 컬럼의 순서가 바뀌면 이게 호출되어야함
        {
            TableViewModel tvm = this.DataContext as TableViewModel;
            List<ColumnViewModel> sorted = tvm.SheetViewModel.ColumnViewModels.Where(cvm => !cvm.IsSelected).OrderBy(cvm => cvm.Order).ToList();
            Int32 index = 0;
            Double anglePerMenu = AngleSpan / sorted.Count;

            IndexHelperWrapperElement.Children.Clear();

            foreach (ColumnViewModel cvm in sorted)
            {
                Path path = null;
                //TextBlock textBlock = null;

                if (index == 0)
                {
                    path = DrawArc((Double)App.Current.Resources["ColumnIndexerHeight"], -Math.PI / 18, anglePerMenu * (index + 1));
                }
                else if (index < sorted.Count - 1)
                {
                    path = DrawArc((Double)App.Current.Resources["ColumnIndexerHeight"], anglePerMenu * index, anglePerMenu * (index + 1));
                }
                else // 5개 이하일때 마지막은 모두 포함하도록
                {
                    path = DrawArc((Double)App.Current.Resources["ColumnIndexerHeight"], anglePerMenu * index, Math.PI / 2);
                }

                Int32 indexCopied = index;
                path.PointerEntered += (o, e) =>
                {
                    TableViewModel.IndexColumn(e.GetCurrentPoint(this).PointerId, indexCopied);
                    Debug.WriteLine(indexCopied);
                };

                Canvas.SetTop(path, (Double)App.Current.Resources["ColumnIndexerHeight"]);

                IndexHelperWrapperElement.Children.Add(path);

                path.Stroke = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
                path.StrokeThickness = 2;
                path.Fill = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
                index++;
            }

            /*foreach (ColumnViewModel cvm in sorted)
            {
                Path path = null;
                //TextBlock textBlock = null;

                if (IndexHelperWrapperElement.Children.Count > index)
                {
                    path = IndexHelperWrapperElement.Children[index] as Path;
                    //textBlock = border.Child as TextBlock;
                }
                else
                {
                    if(index == 0)
                    {
                        path = DrawArc((Double)App.Current.Resources["ColumnIndexerHeight"], -Math.PI / 18, anglePerMenu * (index + 1));
                    }
                    else if (index < sorted.Count - 1)
                    {
                        path = DrawArc((Double)App.Current.Resources["ColumnIndexerHeight"], anglePerMenu * index, anglePerMenu * (index + 1));
                    }
                    else // 5개 이하일때 마지막은 모두 포함하도록
                    {
                        path = DrawArc((Double)App.Current.Resources["ColumnIndexerHeight"], anglePerMenu * index, Math.PI / 2);
                    }

                    Int32 indexCopied = index;
                    path.PointerEntered += (o, e) =>
                    {
                        TableViewModel.IndexColumn(e.GetCurrentPoint(this).PointerId, indexCopied);
                        Debug.WriteLine(indexCopied);
                    };
                    
                    Canvas.SetTop(path, (Double)App.Current.Resources["ColumnIndexerHeight"]);

                    IndexHelperWrapperElement.Children.Add(path);
                }

                path.Stroke = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
                path.StrokeThickness = 2;
                path.Fill = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));              
                index++;
            }

            for (Int32 j = IndexHelperWrapperElement.Children.Count - 1; j >= index; --j)
            {
                IndexHelperWrapperElement.Children.RemoveAt(j);
            }
            */
        }

        private void Opener_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(this);

            if (point.PointerDevice.PointerDeviceType != PointerDeviceType.Touch) return;
            if (TableViewModel.MainPageViewModel.View.TableView.ColumnHighlighter.ColumnViewModel != null) return;

            IndexHelperWrapperElement.IsHitTestVisible = true;
            IndexHelperTransform.X = point.Position.X;
            IndexHelperTransform.Y = point.Position.Y - 20;
            HideHelperStoryboard.Pause();
            ShowHelperStoryboard.Begin();
        }

        private void Opener_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).PointerDevice.PointerDeviceType != PointerDeviceType.Touch) return;
            IndexHelperWrapperElement.IsHitTestVisible = false;
            ShowHelperStoryboard.Pause();
            HideHelperStoryboard.Begin();
        }

        private void IndexHelperWrapperElement_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TableViewModel.CancelIndexing(true);
            IndexHelperWrapperElement.IsHitTestVisible = false;
            ShowHelperStoryboard.Pause();
            HideHelperStoryboard.Begin();
        }

        public void Reset()
        {
            IndexHelperWrapperElement.Children.Clear();
        }
    }
}
