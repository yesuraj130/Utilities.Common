using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Utilities.Common.Extensions
{
    public static class EnumExt
    {
        private static readonly Dictionary<Type, TypeConverter> Cache = new Dictionary<Type, TypeConverter>();

        public static string ToTypeConverterString(this Enum code)
        {
            var Type = code.GetType();
            if (Cache.TryGetValue(Type, out var match))
            {
                return (string)match.ConvertTo(code, typeof(string));
            }
            else
            {
                var NewType = TypeDescriptor.GetConverter(code.GetType());
                try { Cache.Add(Type, NewType); } catch { }
                return (string)NewType.ConvertTo(code, typeof(string));
            }

        }
    }
}
