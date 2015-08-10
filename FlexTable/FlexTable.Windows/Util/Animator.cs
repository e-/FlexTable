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
    }
}
