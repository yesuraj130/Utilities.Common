namespace Utilities.Common.Extensions
{
    public static class ObjectExt
    {
        public static bool IsNotNullAndEqual(this object Actual, object Comparer)
        {
            return !(Actual is null) && !(Comparer is null) && Actual.Equals(Comparer);
        }
        public static bool IsNullOrEqual(this object Actual, object Comparer)
        {
            if (Actual is null) return Comparer is null;
            else if (Comparer is null) return false;
            else return Actual.Equals(Comparer);
        }
    }
}
