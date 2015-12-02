using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace FlexTable.Util
{
    public class Animator
    {
        static EasingFunctionBase EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut };

        public static DoubleAnimation Generate(DependencyObject obj, String path, double? to)
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = EasingFunction
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
                EasingFunction = EasingFunction
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

        public static ColorAnimation GenerateColorAnimation(DependencyObject obj, String path, Color? to, double duration, double beginTime)
        {
            ColorAnimation da = new ColorAnimation()
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(duration),
                BeginTime = TimeSpan.FromMilliseconds(beginTime),
                EasingFunction = EasingFunction
            };

            Storyboard.SetTarget(da, obj);
            Storyboard.SetTargetProperty(da, path);

            return da;
        }
    }
}
