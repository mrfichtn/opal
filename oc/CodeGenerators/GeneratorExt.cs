using System;
using System.Collections.Generic;
using System.Text;

namespace Generators
{
    public static class GeneratorExt
    {
        public static Generator Write<T>(this Generator generator, IEnumerable<T> items, string separator)
            where T : IGeneratable
        {
            var isFirst = true;
            foreach (var item in items)
            {
                if (isFirst)
                    isFirst = false;
                else
                    generator.Write(separator);
                generator.Write(item);
            }
            return generator;
        }

        public static Generator Write<T>(this Generator generator, IEnumerable<T> items, string separator,
            Action<Generator, T> write)
        {
            var isFirst = true;
            foreach (var item in items)
            {
                if (isFirst)
                    isFirst = false;
                else
                    generator.Write(separator);
                write(generator, item);
            }
            return generator;
        }

        public static IGenerator Write(this IGenerator generator, double value)
        {
            return generator.Write(value.ToString());
        }

    }
}
