using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckLogWorker.Enumerable
{
    public static class AsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, Task<TResult>> selector)
        {
            foreach (var item in enumerable)
            {
                yield return await selector(item);
            }
        }

        public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
        {
            await foreach (var item in enumerable)
            {
                yield return selector(item);
            }
        }
        public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, Task<TResult>> selector)
        {
            await foreach (var item in enumerable)
            {
                yield return await selector(item);
            }
        }
        public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, int, TResult> selector)
        {
            var index = 0;

            await foreach (var item in enumerable)
            {
                yield return selector(item, index++);
            }
        }
        public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, int, Task<TResult>> selector)
        {
            var index = 0;

            await foreach (var item in enumerable)
            {
                yield return await selector(item, index++);
            }
        }

        public static async IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            await foreach (var item in enumerable)
            {
                if (predicate(item))
                {
                    yield return item;
                }
            }
        }
        public static async IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> enumerable, Func<T, Task<bool>> predicate)
        {
            await foreach (var item in enumerable)
            {
                if (await predicate(item))
                {
                    yield return item;
                }
            }
        }

        public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            var list = await enumerable.ToListAsync();

            return list.ToArray();
        }
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            var list = new List<T>();

            await foreach (var item in enumerable)
            {
                list.Add(item);
            }

            return list;
        }

        public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            await foreach (var item in enumerable)
            {
                return item;
            }

            throw new InvalidOperationException("The sequence is empty");
        }
        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            await foreach (var item in enumerable)
            {
                return item;
            }

            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable, Func<T, bool> predicate) => await enumerable.WhereAsync(predicate).FirstOrDefaultAsync();
        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable, Func<T, Task<bool>> predicate) => await enumerable.WhereAsync(predicate).FirstOrDefaultAsync();

        public static async Task<T> LastAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            T result = default;
            bool resulted = false;

            await foreach (var item in enumerable)
            {
                result = item;
                resulted = true;
            }

            if (!resulted)
            {
                throw new InvalidOperationException("The sequence is empty");
            }

            return result;
        }
        public static async Task<T> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            T result = default;

            await foreach (var item in enumerable)
            {
                result = item;
            }

            return result;
        }
    }
}
