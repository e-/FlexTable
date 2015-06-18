using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace FlexTable
{
    /// <summary>
    /// 자체에서 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page, IMainPage
    {
        ViewModel.MainPageViewModel mainPageViewModel;
        Util.CsvLoader csvLoader = new Util.CsvLoader();
        InkManager inkManager = new InkManager();
        InkDrawingAttributes inkDrawingAttributes = new InkDrawingAttributes();
        Dictionary<uint, Point> pointerDictionary = new Dictionary<uint, Point>();

        public MainPage()
        {
            mainPageViewModel = new ViewModel.MainPageViewModel(this);
            this.DataContext = mainPageViewModel;
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Model.Sheet sheet = await csvLoader.Load();
            sheet.MeasureColumnWidth(DummyCell);
            sheet.UpdateColumnX();
            sheet.GuessColumnType();
            sheet.CreateColumnSummary();
            mainPageViewModel.Sheet = sheet;

            for (Int32 i = 0; i < sheet.RowCount - 1; ++i)
            {
                RowDefinition rd = new RowDefinition()
                {
                    Height = new GridLength((Double)App.Current.Resources["RowHeight"])
                };
                GuidelineGrid.RowDefinitions.Add(rd);

                Rectangle rectangle = new Rectangle()
                {
                    Style = (Style)App.Current.Resources["RowGuidelineStyle" + (i%2).ToString()]
                };
                Grid.SetRow(rectangle, i);

                GuidelineGrid.Children.Add(rectangle);
            }

            foreach (Model.Row row in sheet.Rows)
            {
                View.RowPresenter rowPresenter = new View.RowPresenter(row);
                TableCanvas.Children.Add(rowPresenter);
                rowPresenter.Update();
                mainPageViewModel.RowPresenters.Add(rowPresenter);
            }

            inkDrawingAttributes.Color = Windows.UI.Colors.Black;
            inkDrawingAttributes.Size = new Size(10, 10);
            inkManager.SetDefaultDrawingAttributes(inkDrawingAttributes);
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            RowHeader.VerticalOffset = sv.VerticalOffset;
            TopColumnHeader.HorizontalOffset = sv.HorizontalOffset;
            BottomColumnHeader.HorizontalOffset = sv.HorizontalOffset;
        }

        private void PenCanvas_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                PointerPoint pointerPoint = e.GetCurrentPoint(this);
                if (!pointerPoint.Properties.IsEraser)
                {
                    inkManager.Mode = InkManipulationMode.Inking;
                }
                else
                {
                    inkManager.Mode = InkManipulationMode.Erasing;
                }

                inkManager.ProcessPointerDown(pointerPoint);
                pointerDictionary.Add(e.Pointer.PointerId, pointerPoint.Position);
                e.Handled = true;
            }
        }

        private void PenCanvas_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                PointerPoint pointerPoint = e.GetCurrentPoint(this);
                uint id = pointerPoint.PointerId;

                if (pointerDictionary.ContainsKey(id))
                {
                    foreach (PointerPoint point in Enumerable.Reverse(e.GetIntermediatePoints(this))/*.Reverse()*/)
                    {
                        // Give PointerPoint to InkManager
                        object obj = inkManager.ProcessPointerUpdate(point);

                        // Render the line
                        if (inkManager.Mode == InkManipulationMode.Erasing)
                        {
                            Rect rect = (Rect)obj;
                            if (rect.Width != 0 && rect.Height != 0)
                            {
                                RenderAllStrokes();
                            }
                        }
                        else
                        {
                            Point point1 = pointerDictionary[id];
                            Point point2 = pointerPoint.Position;

                            Line line = new Line
                            {
                                X1 = point1.X,
                                Y1 = point1.Y,
                                X2 = point2.X,
                                Y2 = point2.Y,
                                Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                                StrokeThickness = inkDrawingAttributes.Size.Width * pointerPoint.Properties.Pressure,
                                StrokeStartLineCap = PenLineCap.Round,
                                StrokeEndLineCap = PenLineCap.Round
                            };
                            NewStrokeGrid.Children.Add(line);
                            pointerDictionary[id] = point2;
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void PenCanvas_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                PointerPoint pointerPoint = e.GetCurrentPoint(this);
                uint id = pointerPoint.PointerId;

                if (pointerDictionary.ContainsKey(id))
                {
                    // Give PointerPoint to InkManager
                    inkManager.ProcessPointerUp(pointerPoint);

                    if (inkManager.Mode == InkManipulationMode.Inking)
                    {
                        // Get rid of the little Line segments
                        NewStrokeGrid.Children.Clear();

                        // Render the new stroke
                        IReadOnlyList<InkStroke> inkStrokes = inkManager.GetStrokes();
                        InkStroke inkStroke = inkStrokes[inkStrokes.Count - 1];

                        if (inkStroke.BoundingRect.Width < 10 && inkStroke.BoundingRect.Height < 10)
                        {
                            inkStroke.Selected = true;
                            inkManager.DeleteSelected();
                        }
                        else
                        {
                            RenderStroke(inkStroke);
                        }
                    }

                    pointerDictionary.Remove(id);
                    RecognizeStrokes();
                }

                e.Handled = true;
            }
        }

        void RenderAllStrokes()
        {
            StrokeGrid.Children.Clear();
            foreach (InkStroke inkStroke in inkManager.GetStrokes())
                RenderStroke(inkStroke);
        }

        void RenderStroke(InkStroke inkStroke)
        {
            Brush brush = new SolidColorBrush(inkStroke.DrawingAttributes.Color);
            IReadOnlyList<InkStrokeRenderingSegment> inkSegments = inkStroke.GetRenderingSegments();
            for (int i = 1; i < inkSegments.Count; i++)
            {
                InkStrokeRenderingSegment inkSegment = inkSegments[i];
                BezierSegment bezierSegment = new BezierSegment
                {
                    Point1 = inkSegment.BezierControlPoint1,
                    Point2 = inkSegment.BezierControlPoint2,
                    Point3 = inkSegment.Position
                };
                PathFigure pathFigure = new PathFigure
                {
                    StartPoint = inkSegments[i - 1].Position,
                    IsClosed = false,
                    IsFilled = false
                };
                pathFigure.Segments.Add(bezierSegment);
                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);
                Path path = new Path
                {
                    Stroke = brush,
                    StrokeThickness = inkStroke.DrawingAttributes.Size.Width *
                    inkSegment.Pressure,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Data = pathGeometry
                };
                StrokeGrid.Children.Add(path);

#if DEBUG
                Ellipse el = new Ellipse()
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(el, inkSegment.Position.X - 5);
                Canvas.SetTop(el, inkSegment.Position.Y - 5);
                //TestCanvas.Children.Add(el);
#endif
            }
        }

        void RemoveAllStrokes()
        {
            foreach (InkStroke stroke in inkManager.GetStrokes())
            {
                stroke.Selected = true;
            }
            inkManager.DeleteSelected();
            RenderAllStrokes();
        }

        async Task RecognizeStrokes()
        {
            IReadOnlyList<InkStroke> strokes = inkManager.GetStrokes();

            if (strokes.Count == 0) return;

            Double centerX = strokes[0].BoundingRect.X + strokes[0].BoundingRect.Width / 2 - (Double)App.Current.Resources["RowHeaderWidth"] + TableScrollViewer.HorizontalOffset;
            Int32 columnIndex = -1, index = 0;
            foreach (Model.Column column in mainPageViewModel.Sheet.Columns)
            {
                if (column.X <= centerX && centerX < column.X + column.Width)
                {
                    columnIndex = index;
                    break;
                }
                index++;
            }

            /*if (strokes.Count == 1 && strokes[0].BoundingRect.Width < 40 && strokes[0].BoundingRect.Height > 100)
            {
                var segments = strokes[0].GetRenderingSegments();
                InkStrokeRenderingSegment first = segments.First(), last = segments.Last();               

                if (columnIndex >= 0)
                {
                    if (first.Position.Y < last.Position.Y)
                    {
                        mainPageViewModel.Sort(columnIndex, false); //오름차순
                    }
                    else
                    {
                        mainPageViewModel.Sort(columnIndex, true); // 내림차순
                    }
                }

                RemoveAllStrokes();
                return;
            }*/
            
            IReadOnlyList<InkRecognitionResult> results = await this.inkManager.RecognizeAsync(InkRecognitionTarget.All);

            foreach (InkRecognitionResult result in results)
            {
                foreach (String candidate in result.GetTextCandidates())
                {
                    Debug.WriteLine(candidate);

                    if (columnIndex >=0 && (candidate == "x" || candidate == "X"))
                    {
                        mainPageViewModel.MarkColumnDisabled(mainPageViewModel.Sheet.Columns[columnIndex]);
                        TopColumnHeader.Update();
                        BottomColumnHeader.Update();
                        RemoveAllStrokes();
                        return;
                    }

                    if (columnIndex >= 0 && (candidate == "o" || candidate == "O" || candidate == "0"))
                    {
                        mainPageViewModel.MarkColumnEnabled(mainPageViewModel.Sheet.Columns[columnIndex]);
                        TopColumnHeader.Update();
                        BottomColumnHeader.Update();
                        RemoveAllStrokes();
                        return;
                    }
                }
            }
        }

        public void ScrollToColumn(Model.Column column)
        {
            Double offset = TableScrollViewer.HorizontalOffset,
                   width = mainPageViewModel.SheetViewWidth,
                   x1 = column.X,
                   x2 = column.X + column.Width;

            Double? to = null;

            if (x1 < offset)
            {
                to = x1 - 20;
            }
            else if (offset + width < x2)
            {
                to = x2 - width + 20;
            }

            TableScrollViewer.ChangeView(to, null, null);

            if (to == null)
            {
                mainPageViewModel.ScrollLeft = TableScrollViewer.HorizontalOffset;
            }
            else
            {
                mainPageViewModel.ScrollLeft = (Double)to;
            }
        }

        public void UpdateColumnHeaders()
        {
            TopColumnHeader.Update();
            BottomColumnHeader.Update();
        }
    }
}
