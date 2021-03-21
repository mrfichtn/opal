using System.Text;

namespace Opal.Containers
{
    public static class StringBuilderExt
    {
        public static StringBuilder AppendIf(this StringBuilder builder, 
            bool condition, 
            string value) =>
            condition ? builder.Append(value) : builder;
    }
}
