using System;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Containers
{
    public static class HashExt
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> collection) =>
            new HashSet<T>(collection);

        public static HashSet<T> ToSet<T, U>(this IEnumerable<U> collection, Func<U, T> selector) =>
            new HashSet<T>(collection.Select(selector));

        public static void CopyFrom<T>(this HashSet<T> result, IEnumerable<T> collection)
        {
            result.Clear();
            foreach (var item in collection)
                result.Add(item);
        }

        public static HashSet<T> Intersect<T>(this HashSet<T> left, HashSet<T> right)
        {
            var result = new HashSet<T>();
            foreach (var item in left)
            {
                if (right.Contains(item))
                    result.Add(item);
            }
            return result;
        }

        public static HashSet<T> DisjointUnion<T>(this HashSet<T> left, HashSet<T> right)
        {
            var result = new HashSet<T>();
            foreach (var item in left)
            {
                if (!right.Contains(item))
                    result.Add(item);
            }
            return result;
        }
    }
}
