﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace FlexTable.Util
{
    class Drawable
    {
        public delegate void StrokeAddingEventHandler(Rect boundingRect);
        public delegate void StrokeAddedEventHandler(InkManager inkManager);

        public event StrokeAddingEventHandler StrokeAdding;
        public event StrokeAddedEventHandler StrokeAdded;
        
        
        private Boolean ignoreSmallStrokes = true;
        public Boolean IgnoreSmallStrokes { get { return ignoreSmallStrokes; } set { ignoreSmallStrokes = value;  } }

        InkManager inkManager = new InkManager();
        InkDrawingAttributes inkDrawingAttributes = new InkDrawingAttributes();
        Dictionary<uint, Point> pointerDictionary = new Dictionary<uint, Point>();
        UIElement root;
        Grid StrokeGrid;
        Grid NewStrokeGrid;

        public Drawable()
        {
            inkDrawingAttributes.Color = Colors.Black;
            inkDrawingAttributes.Size = new Size(3, 3);
            inkManager.SetDefaultDrawingAttributes(inkDrawingAttributes);
        }

        public void Attach(UIElement root, Grid StrokeGrid, Grid NewStrokeGrid)
        {
            this.root = root;
            this.StrokeGrid = StrokeGrid;
            this.NewStrokeGrid = NewStrokeGrid;

            root.PointerPressed += PointerPressed;
            root.PointerMoved += PointerMoved;
            root.PointerReleased += PointerReleased;
            root.PointerExited += root_PointerExited;
        }

        void root_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("Exited");
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                PointerReleased(sender, e);
                e.Handled = true;
            }
        }

        private void PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen && e.Pointer.IsInContact)
            {
                //Debug.WriteLine("Pressed");
                PointerPoint pointerPoint = e.GetCurrentPoint(root);
                if (!pointerPoint.Properties.IsEraser)
                {
                    inkManager.Mode = InkManipulationMode.Inking;
                }
                else
                {
                    inkManager.Mode = InkManipulationMode.Erasing;
                }

                if (pointerDictionary.ContainsKey(e.Pointer.PointerId))
                {
                    pointerDictionary[e.Pointer.PointerId] = pointerPoint.Position;
                }
                else {
                    inkManager.ProcessPointerDown(pointerPoint);
                }
                pointerDictionary[e.Pointer.PointerId] = pointerPoint.Position;
                e.Handled = true;
                boundingRect = new Rect()
                {
                    X = pointerPoint.Position.X,
                    Y = pointerPoint.Position.Y,
                    Width = 0,
                    Height = 0
                };
            }
        }

        Rect boundingRect = new Rect();

        private void PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen && e.Pointer.IsInContact)
            {
                //Debug.WriteLine("Moved");
                
                PointerPoint pointerPoint = e.GetCurrentPoint(root);
                uint id = pointerPoint.PointerId;

                //Debug.WriteLine(id) ;
                if (pointerDictionary.ContainsKey(id))
                {
                    foreach (PointerPoint point in Enumerable.Reverse(e.GetIntermediatePoints(root))/*.Reverse()*/)
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
                                StrokeThickness = inkDrawingAttributes.Size.Width,// * pointerPoint.Properties.Pressure,
                                StrokeStartLineCap = PenLineCap.Round,
                                StrokeEndLineCap = PenLineCap.Round
                            };

                            boundingRect.Union(point2);
                            NewStrokeGrid.Children.Add(line);
                            pointerDictionary[id] = point2;
                        }
                    }

                    if (StrokeAdding != null) StrokeAdding(boundingRect);
                }
                e.Handled = true;
            }
        }

        private void PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("Released");
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
            {
                //Debug.WriteLine("Released");
                PointerPoint pointerPoint = e.GetCurrentPoint(root);
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

                        if (IgnoreSmallStrokes && inkStroke.BoundingRect.Width < 10 && inkStroke.BoundingRect.Height < 10)
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
                    
                    if(StrokeAdded != null) StrokeAdded(inkManager);
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
                    StrokeThickness = inkStroke.DrawingAttributes.Size.Width,// * inkSegment.Pressure,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Data = pathGeometry
                };
                StrokeGrid.Children.Add(path);                
            }
        }

        public void RemoveAllStrokes()
        {
            foreach (InkStroke stroke in inkManager.GetStrokes())
            {
                stroke.Selected = true;
            }
            inkManager.DeleteSelected();
            RenderAllStrokes();
        }        
    }
}
