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
        
        public Point CalculateCartesianCoordinate(Double angle, Double radius)
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

        const Double AngleSpan = Math.PI / 2 / 9 * 7;
        readonly Color WrapperFillColor = Color.FromArgb(50, 200, 200, 200);

        public void Update() // 처음에 초기화하거나 컬럼의 순서가 바뀌면 이게 호출되어야함
        {
            TableViewModel tvm = this.DataContext as TableViewModel;
            List<ColumnViewModel> sorted = tvm.SheetViewModel.ColumnViewModels.Where(cvm => !cvm.IsSelected).OrderBy(cvm => cvm.Order).ToList();
            Int32 index = 0;
            Double anglePerMenu = AngleSpan / sorted.Count;
            Double height = (Double)App.Current.Resources["ColumnIndexerHeight"];

            IndexHelperWrapperElement.Children.Clear();
            IndexHelperElement.Children.Clear();

            {
                Path path = DrawArc(height, 0, Math.PI / 2);
                IndexHelperWrapperElement.Children.Add(path);
                Canvas.SetTop(path, height);
                path.Fill = new SolidColorBrush(WrapperFillColor);
            }

            foreach (ColumnViewModel cvm in sorted)
            {
                Path path = null;
                //TextBlock textBlock = null;

                if (index == 0)
                {
                    path = DrawArc(height, 0, anglePerMenu * (index + 1));
                }
                else if (index < sorted.Count - 1)
                {
                    path = DrawArc(height, anglePerMenu * index, anglePerMenu * (index + 1));
                }
                else // 5개 이하일때 마지막은 모두 포함하도록
                {
                    path = DrawArc(height, anglePerMenu * index, Math.PI / 2);
                }

                /*Int32 indexCopied = index;
                path.PointerEntered += (o, e) =>
                {
                    TableViewModel.IndexColumn(e.GetCurrentPoint(this).PointerId, indexCopied);
                };*/

                Canvas.SetTop(path, height);

                IndexHelperElement.Children.Add(path);

                path.Stroke = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
                path.StrokeThickness = 1;
                path.Fill = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
                index++;
            }            
        }

        public void Reset()
        {
            IndexHelperWrapperElement.Children.Clear();
        }

        Int32 GetActivatedIndex(Point position)
        {
            TableViewModel tvm = this.DataContext as TableViewModel;
            List<ColumnViewModel> sorted = tvm.SheetViewModel.ColumnViewModels.Where(cvm => !cvm.IsSelected).OrderBy(cvm => cvm.Order).ToList();
            Int32 index = 0;
            Double anglePerMenu = AngleSpan / sorted.Count;
            Double height = (Double)App.Current.Resources["ColumnIndexerHeight"];
            Double x = position.X;
            Double y = height - position.Y;

            if (x < 0 || x * x + y * y < 50 * 50) return 0;
            if (y < 0) return sorted.Count - 1;

            Double angle = Math.PI / 2 - Math.Atan2(y, x);

            foreach (ColumnViewModel cvm in sorted)
            {
                if (index == 0)
                {
                    if (0 <= angle && angle < anglePerMenu) break;
                }
                else if (index < sorted.Count - 1)
                {
                    if (anglePerMenu * index <= angle && angle < anglePerMenu * (index + 1)) break;
                }
                else // 5개 이하일때 마지막은 모두 포함하도록
                {
                    if (anglePerMenu * index <= angle && angle <= Math.PI / 2) break;
                }
                index++;
            }

            return index;
        }

        private void IndexHelperElement_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (TableViewModel.MainPageViewModel.View.TableView.ColumnHighlighter.ColumnViewModel != null) return;
            CapturePointer(e.Pointer);

            HideHelperStoryboard.Pause();
            ShowHelperStoryboard.Begin();
        }

        private void UserControl_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            ShowHelperStoryboard.Pause();
            HideHelperStoryboard.Begin();

            Point position = e.GetCurrentPoint(this).Position;
            TableViewModel tvm = this.DataContext as TableViewModel;

            var viewModel = tvm.MainPageViewModel.ExplorationViewModel.TopPageView.ViewModel;

            if (viewModel.IsEmpty)
            {
                TableViewModel.CancelIndexing(true);
                return;
            }

            if (viewModel.IsNoPossibleVisualizationWarningVisible)
            {
                TableViewModel.CancelIndexing(true);
                return;
            }

            viewModel.State = PageViewModel.PageViewState.Selected;
            viewModel.StateChanged(tvm.MainPageViewModel.ExplorationViewModel.TopPageView);
            TableViewModel.CancelIndexing(true);
        }

        private void UserControl_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            TableViewModel.CancelIndexing(true);
            ShowHelperStoryboard.Pause();
            HideHelperStoryboard.Begin();
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Point position = e.GetCurrentPoint(this).Position;

            TableViewModel.IndexColumn(e.GetCurrentPoint(this).PointerId, GetActivatedIndex(position));
        }
    }
}
