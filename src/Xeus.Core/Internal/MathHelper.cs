using System;
using System.Collections.Generic;
using System.Text;

namespace Xeus.Core.Internal
{
    internal static class MathHelper
    {
        public static ulong Roundup(ulong value, ulong unit)
        {
            if (value % unit == 0) return value;
            else return ((value / unit) + 1) * unit;
        }
    }
}
