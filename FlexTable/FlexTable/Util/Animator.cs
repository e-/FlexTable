using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace FlexTable.Util
{
    public class Animator
    {
        public static DoubleAnimation Generate(DependencyObject obj, String path, double? to)
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(da, obj);
            Storyboard.SetTargetProperty(da, path);

            return da;
        }

        public static DoubleAnimation Generate(DependencyObject obj, String path, double? to, double duration)
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(da, obj);
            Storyboard.SetTargetProperty(da, path);

            return da;
        }

        public static DoubleAnimation Generate(DependencyObject obj, String path, double? to, double duration, double beginTime)
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(duration),
                BeginTime = TimeSpan.FromMilliseconds(beginTime),
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(da, obj);
            Storyboard.SetTargetProperty(da, path);

            return da;
        }
    }
}
