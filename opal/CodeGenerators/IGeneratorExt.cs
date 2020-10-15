using Generators;
using Opal.Containers;
using System.Text;

namespace Opal.CodeGenerators
{
    public static class IGeneratorExt
    {
        public static T WriteIf<T>(this T generator, bool cond, string value)
            where T: IGenerator
        {
            if (cond)
                generator.Write(value);
            return generator;
        }

        public static T WriteEsc<T>(this T generator, string value)
            where T: IGenerator
        {
            if (value == null)
                generator.Write("null");
            else if (value.Length == 0)
                generator.Write("\"\"");
            else
            {
                generator.WriteIndent();
                generator.WriteChar('\"');
                foreach (var ch in value.ToEscape())
                    generator.WriteChar(ch);
                generator.WriteChar('\"');
            }
            return generator;
        }
    }
}
