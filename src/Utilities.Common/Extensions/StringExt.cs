using System;
using System.Globalization;

namespace Utilities.Common.Extensions
{
    public static class StringExt
    {
        private static readonly TextInfo MixedCase_textInfo = CultureInfo.CurrentCulture.TextInfo;
        public static string MixedCase(this string input)
        {
            if (input == null) return string.Empty;
#pragma warning disable CA1304 // Specify CultureInfo
            return MixedCase_textInfo.ToTitleCase(input.ToLower());
#pragma warning restore CA1304 // Specify CultureInfo

        }


        /// <summary> Not case sensitive. </summary>
        public static bool ContainsExt(this string source, params string[] compares)
        {
            if (source is not null)
            {
                foreach (var comparer in compares)
                {
                    if (comparer is not null)
                    {
                        if (source.IndexOf(comparer, StringComparison.InvariantCultureIgnoreCase) >= 0) return true;
                    }
                }
            }

            return false;
        }

        /// <summary> Trim and Not case sensitive. </summary>
        public static bool IsEqualExtTrim(this string source, string comparer)
        {
            return source.Trim().Equals(comparer.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>Not case sensitive. </summary>
        public static bool IsEqualExt(this string source, string comparer)
        {
            return source.Equals(comparer, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsEqualOrNull(this string source, string comparer)
        {
            if (source is null)
            {
                return comparer is null;
            }
            else
            {
                if (comparer is null) return false;
                else return source.Equals(comparer, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public static bool IsNullOrWhiteSpace(this string source) => string.IsNullOrWhiteSpace(source);

        public static bool IsNullOrEmpty(this string source) => string.IsNullOrEmpty(source);

        /// <summary>Not case sensitive. </summary>
        public static bool IsStartsWith(this string source, string comparer)
        {
            if (source is null || comparer is null) return false;

            return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(source, comparer, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols);
        }

        /// <summary>Adds text. </summary>
        public static string Add(this string source, string comparer)
        {
            if (source is null) return comparer;
            if (comparer is null) return source;

            return $"{source}{comparer}";
        }
        /// <summary>Adds text with separator. </summary>
        public static string Add(this string source, string comparer, string separator)
        {
            if (source is null) return comparer;
            if (comparer is null) return source;

            if (separator is null) return $"{source}{comparer}";
            return $"{source}{separator}{comparer}";
        }
    }
}
