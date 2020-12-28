using Generators;
using System.Text;

namespace Opal.ParseTree
{
    public class Using: Segment
    {
        private readonly string value;
        public Using(Identifier name)
            : base(name)
        {
            value = name.Value;
        }

        public void Write(IGenerator generator)
        {
            generator.Write("using ")
                .Write(value)
                .WriteLine(";");
        }

        public void AppendTo(StringBuilder builder)
        {
            builder.Append("using ")
                .Append(value)
                .Append(';')
                .AppendLine();
        }
    }
}
