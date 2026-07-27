
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utilities.Common
{
    public static class AsyncExt
    {
        public static AsyncEnumerableWrapper<T> AsAsyncEnumerable<T>(this IEnumerable<T> sequence)
        {
            return new AsyncEnumerableWrapper<T>(sequence);
        }
    }


    public class AsyncEnumerableWrapper<T>
    {
        private readonly IEnumerable<T> _sequence;
        public AsyncEnumerableWrapper(IEnumerable<T> sequence) => _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        public AsyncEnumeratorWrapper<T> GetAsyncEnumerator() => new AsyncEnumeratorWrapper<T>(_sequence.GetEnumerator());
    }

    public class AsyncEnumeratorWrapper<T>
    {
        private readonly IEnumerator<T> _enumerator;
        public T Current { get; private set; }

        public AsyncEnumeratorWrapper(IEnumerator<T> enumerator) => _enumerator = enumerator;

        public async Task<bool> MoveNextAsync()
        {
            var hasNext = await Task.Run(() => _enumerator.MoveNext());
            Current = hasNext ? _enumerator.Current : default(T);
            return hasNext;
        }
    }
}
