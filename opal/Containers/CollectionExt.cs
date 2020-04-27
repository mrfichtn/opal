using System.Collections.Generic;

namespace Opal.Containers
{
    public static class CollectionExt
    {
        public static bool Compare(this ICollection<int> left, ICollection<int> right)
        {
            if (left.Count != right.Count)
                return false;
            foreach (var item in left)
            {
                if (!right.Contains(item))
                    return false;
            }
            return true;
        }
    }
}
