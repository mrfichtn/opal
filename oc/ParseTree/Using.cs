using Generators;

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

        public void Write(Generator generator)
        {
            generator.Write("using ")
                .Write(value)
                .WriteLine(";");
        }
    }
}
