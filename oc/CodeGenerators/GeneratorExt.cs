using Opal.Containers;
using System;
using System.Collections.Generic;

namespace Generators
{
    public static class GeneratorExt
    {
        public static TGenerator Write<TItem, TGenerator>(this TGenerator generator, 
            IEnumerable<TItem> items, 
            string separator,
            Action<TGenerator, TItem> write)
            where TGenerator: Generator<TGenerator>
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

        public static T Write<T>(this T generator, double value)
            where T : Generator<T> =>
            generator.Write(value.ToString());

        public static T WriteIf<T>(this T generator, bool cond, string value)
            where T : Generator<T>
        {
            if (cond)
                generator.Write(value);
            return generator;
        }

        public static T WriteEsc<T>(this T generator, string value)
            where T : Generator<T>
        {
            if (value == null)
                generator.Write("null");
            else if (value.Length == 0)
                generator.Write("\"\"");
            else
            {
                generator.Write('\"');
                foreach (var ch in value.ToEscape())
                    generator.WriteChar(ch);
                generator.WriteChar('\"');
            }
            return generator;
        }

        public static T Join<T, U>(this T generator,
            IEnumerable<U> items,
            Action<T, U> writeItem,
            Action<T> separator)
            where T : Generator<T>
        {
            var isFirst = true;
            foreach (var item in items)
            {
                if (isFirst)
                    isFirst = false;
                else
                    separator(generator);
                writeItem(generator, item);
            }
            return generator;
        }

        public static T Join<T, U>(this T generator,
            IEnumerable<U> items,
            Action<T, U> writeItem,
            string separator)
            where T: Generator<T>
        {
            var isFirst = true;
            foreach (var item in items)
            {
                if (isFirst)
                    isFirst = false;
                else
                    generator.Write(separator);
                writeItem(generator, item);
            }
            return generator;
        }

    }
}
