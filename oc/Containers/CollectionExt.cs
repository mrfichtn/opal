using System.Collections.Generic;
using System.Text;

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

        public static string ToText(this int[,] array)
        {
            var builder = new StringBuilder();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    if (col > 0)
                        builder.Append(", ");
                    builder.Append(array[row, col]);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
