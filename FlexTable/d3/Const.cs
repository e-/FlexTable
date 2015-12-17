using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d3
{
    [Flags]
    public enum TransitionType
    {
        None = 0,
        Position = 1 << 0,
        Size = 1 << 1,
        Opacity = 1 << 2,
        Color = 1 << 3,
        All = 15
    }

    [Flags]
    public enum TransitionPivotType
    {
        Top = 1 << 0,
        Bottom = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3
    }

    public static class Const
    {
        public static readonly TimeSpan AnimationDelay = TimeSpan.FromMilliseconds(200);
    }
}
