
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Containers
{
    public static class EnumExt
	{
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static T First<T>(this IEnumerable<T> items, T @default)
        {
            T result;
            if (items != null)
            {
                using var e = items.GetEnumerator();
                result = e.MoveNext() ? e.Current : @default;
            }
            else
            {
                result = @default;
            }
            return result;
        }

        public static T First<T>(this IEnumerable<T> items, T @default, Func<T, bool> pred)
        {
            T result;
            if (items != null)
            {
                using (var e = items.Where(pred).GetEnumerator())
                    result = e.MoveNext() ? e.Current : @default;
            }
            else
            {
                result = @default;
            }
            return result;
        }

        public static bool TryFirst<T>(this IEnumerable<T> items, out T result)
        {
            var isFound = false;
            if (items != null)
            {
                using (var e = items.GetEnumerator())
                {
                    isFound = e.MoveNext();
                    result = isFound ? e.Current : default!;
                }
            }
            else
            {
                result = default!;
            }
            return isFound;
        }
    }
}
