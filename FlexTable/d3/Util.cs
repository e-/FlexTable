using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
