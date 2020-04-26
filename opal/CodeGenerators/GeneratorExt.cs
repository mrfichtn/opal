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

        public static Generator WriteEsc(this Generator generator, string value)
        {
            if (value == null)
            {
                generator.Write("null");
            }
            else if (value.Length == 0)
            {
                generator.Write("\"\"");
            }
            else
            {
                var builder = new StringBuilder();
                builder.Append('\"');
                value.ToEsc(builder);
                builder.Append('\"');
                generator.Write(builder.ToString());
            }
            return generator;
        }
    }
}
