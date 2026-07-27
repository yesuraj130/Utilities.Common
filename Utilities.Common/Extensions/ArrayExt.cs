using System;

namespace Utilities.Common.Extensions
{
    public static class ArrayExt
    {
        public static T[] Add<T>(this T[] TargetArray, T NewItem)
        {
            if (TargetArray == null) throw new ArgumentNullException(nameof(TargetArray));

            T[] result = new T[TargetArray.Length + 1];
            TargetArray.CopyTo(result, 0);
            result[TargetArray.Length] = NewItem;
            return result;
        }


        public static T[] RemoveAt<T>(this T[] TargetArray, int index)
        {
            if (TargetArray == null) throw new ArgumentNullException(nameof(TargetArray));
            T[] Result = new T[TargetArray.Length - 1];
            if (index > 0) Array.Copy(TargetArray, 0, Result, 0, index);
            if (index < TargetArray.Length - 1) Array.Copy(TargetArray, index + 1, Result, index, TargetArray.Length - index - 1);
            return Result;
        }
    }
}
