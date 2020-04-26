using System.Collections.Generic;

namespace Opal.Containers
{
    public static class ListExt
    {
        public static bool TryGetValue<T>(this IList<T> list, int index, out T item)
        {
            var isOk = ((index >= 0) && (index < list.Count));
            item = isOk ? list[index] : default;
            return isOk;
        }

        /// <summary>
        /// Clears and adds items from list
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">Target list</param>
        /// <param name="items">Source collection</param>
        /// <returns>Target list</returns>
        public static IList<T> SetFrom<T>(this IList<T> list, IEnumerable<T> items)
        {
            list.Clear();
            foreach (var item in items)
                list.Add(item);
            return list;
        }
    }
}
