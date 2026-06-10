using System;

namespace Utilities.Common.Extensions
{
    public static class DoubleExt
    {
        public static double Round(this double source, int digits) => Math.Round(source, digits);
    }
}
