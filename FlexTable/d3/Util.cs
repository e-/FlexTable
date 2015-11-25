using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace d3
{
    public class Util
    {
        static QuarticEase qe = new QuarticEase() { EasingMode = EasingMode.EaseInOut };
        static Duration duration = TimeSpan.FromMilliseconds(300);
        
        public static DoubleAnimation GenerateDoubleAnimation(DependencyObject obj, String path, double? to)
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                To = to,
                Duration = duration,
                EasingFunction = qe
            };

            Storyboard.SetTarget(da, obj);
            Storyboard.SetTargetProperty(da, path);

            return da;
        }

        public static DoubleAnimation GenerateDoubleAnimation(DependencyObject obj, String path, double? to, Boolean enableDependentAnimation)
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                To = to,
                Duration = duration,
                EasingFunction = qe,
                EnableDependentAnimation = enableDependentAnimation
            };

            Storyboard.SetTarget(da, obj);
            Storyboard.SetTargetProperty(da, path);

            return da;
        }

        public static ColorAnimation GenerateColorAnimation(DependencyObject obj, String path, Color? to)
        {
            ColorAnimation da = new ColorAnimation()
            {
                To = to,
                Duration = duration,
                EasingFunction = qe
            };

            Storyboard.SetTarget(da, obj);
            Storyboard.SetTargetProperty(da, path);

            return da;
        }

        public static Func<Object, Int32, T> CreateConstantGetter<T>(T constant)
        {
            return (d, i) => constant;
        }
        
        public static bool TestPointInPolygon(Point point, List<Point> points)
        {
            // Get the angle between the point and the
            // first and last vertices.
            int max_point = points.Count - 1;
            Double total_angle = GetAngle(
                points[max_point].X, points[max_point].Y,
                point.X, point.Y,
                points[0].X, points[0].Y);

            // Add the angles from the point
            // to each other pair of vertices.
            for (int i = 0; i < max_point; i++)
            {
                total_angle += GetAngle(
                    points[i].X, points[i].Y,
                    point.X, point.Y,
                    points[i + 1].X,points[i + 1].Y);
            }

            // The total angle should be 2 * PI or -2 * PI if
            // the point is in the polygon and close to zero
            // if the point is outside the polygon.
            return (Math.Abs(total_angle) > 0.000001);
        }

        static double GetAngle(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            // Get the dot product.
            double dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

            // Get the cross product.
            double cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

            // Calculate the angle.
            return (double)Math.Atan2(cross_product, dot_product);
        }

        static double DotProduct(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            // Get the vectors' coordinates.
            double BAx = Ax - Bx;
            double BAy = Ay - By;
            double BCx = Cx - Bx;
            double BCy = Cy - By;

            // Calculate the dot product.
            return (BAx * BCx + BAy * BCy);
        }

        static double CrossProductLength(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            // Get the vectors' coordinates.
            double BAx = Ax - Bx;
            double BAy = Ay - By;
            double BCx = Cx - Bx;
            double BCy = Cy - By;

            // Calculate the Z coordinate of the cross product.
            return (BAx * BCy - BAy * BCx);
        }
    }
}
