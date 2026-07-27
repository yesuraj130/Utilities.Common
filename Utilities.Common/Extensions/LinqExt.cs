using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.Common.Extensions
{
    public static class LinqExt
    {
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            foreach (T item in sequence) action(item);
        }
        public static void ForEach<T>(this IEnumerable sequence, Action<T> action)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            foreach (T item in sequence) action(item);
        }

        public static ICollection<T> AddAndReturn<T>(this ICollection<T> sequence, T item)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            sequence.Add(item);
            return sequence;
        }

        private static bool InternalTryFirst<TSource>(IEnumerable<TSource> source, out TSource Result)
        {
            if (source != null)
            {
                if (source is IList<TSource> list)
                {
                    if (list.Count > 0)
                    {
                        Result = list[0];
                        return true;
                    }
                }
                else
                {
                    using (IEnumerator<TSource> e = source.GetEnumerator())
                    {
                        if (e.MoveNext())
                        {
                            Result = e.Current;
                            return true;
                        }
                    }
                }
            }

            Result = default;
            return false;
        }

        private static bool InternalTryFirst<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource Result)
        {
            if (source != null && predicate != null)
            {
                foreach (TSource element in source)
                {
                    if (predicate(element))
                    {
                        Result = element;
                        return true;
                    }
                }
            }

            Result = default;
            return false;
        }


        public static TSource FirstOrGiven<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource Given)
        {
            if (InternalTryFirst(source, predicate, out var result)) return result;
            return Given;
        }

        public static TSource FirstOrNull<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) where TSource : class
        {
            if (InternalTryFirst(source, predicate, out var result)) return result;
            return null;
        }

        public static bool TryFirst<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource Result)
        {
            return InternalTryFirst(source, predicate, out Result);
        }
        public static bool TryFirst<TSource>(this IEnumerable<TSource> source, out TSource Result)
        {
            return InternalTryFirst(source, out Result);
        }

        public static bool TryWhere<T>(this IEnumerable<T> SourceCollection, Func<T, bool> predicate, out IEnumerable<T> Result)
        {
            if (SourceCollection != null && predicate != null)
            {
                Result = SourceCollection.Where(predicate)/*.ToList()*/;
                return Result.Any();
            }
            else
            {
                Result = default;
                return false;
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> SourceCollection) => !TryAny(SourceCollection);

        /// <summary> Checks Null Before Linq.Any()  </summary>
        public static bool TryAny<T>(this IEnumerable<T> SourceCollection)
        {
            if (SourceCollection == null) return false;
            return SourceCollection.Any();
        }
        /// <summary> Checks Null Before Linq.Any()  </summary>
        public static bool TryAny(this IEnumerable SourceCollection)
        {
            if (SourceCollection == null) return false;
            return SourceCollection.Any();
        }
        public static bool Any(this IEnumerable SourceCollection)
        {
            if (SourceCollection is null) throw new ArgumentNullException(nameof(SourceCollection));

            if (SourceCollection is ICollection collection) return collection.Count > 0;

            var enumerator = SourceCollection.GetEnumerator();
            try { return enumerator.MoveNext(); }
            finally { if (enumerator is IDisposable disposable) disposable.Dispose(); }
        }

        public static bool TryAny<TSource>(this IEnumerable<TSource> SourceCollection, Func<TSource, bool> predicate)
        {
            if (SourceCollection == null) return false;
            return SourceCollection.Any(predicate);
        }



        //private static bool InternalAny<T>(this IEnumerable<T> SourceCollection, Func<T, bool> predicate)
        //{
        //    bool MatchFound = false;
        //    if (SourceCollection != null && predicate != null)
        //    {
        //        //Parallel.ForEach(SourceCollection, (element, state) =>
        //        foreach (var element in SourceCollection)
        //        {
        //            if (predicate(element))
        //            {
        //                MatchFound = true;
        //                //state.Stop();
        //            }
        //        }/*)*/;
        //    }
        //    return MatchFound;
        //}


        // For faster enumeration
        public static int MaxExt(this IList<int> source)
        {
            if (source.Count == 0) throw new InvalidOperationException("Empty Collection");
            int value = int.MinValue;

            foreach (int x in source)
            {
                if (x > value) value = x;
            }
            return value;

        }

        public static IEnumerable EmptyIfNull(this IEnumerable source) => source ?? Enumerable.Empty<object>();
        public static IEnumerable EmptyIfNull<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();
    }
}
