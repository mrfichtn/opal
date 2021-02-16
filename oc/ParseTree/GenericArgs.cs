using System.Text;

namespace Opal.ParseTree
{
    public class GenericArgs: Segment
    {
        private readonly StringBuilder builder;

        public GenericArgs(Identifier arg)
            : base(arg)
        {
            builder = new StringBuilder(arg.Value);
        }

        public void Add(Identifier arg)
        {
            builder.Append(',')
                .Append(arg);
            End = arg.End;
        }

        public static GenericArgs Add(GenericArgs args, Identifier arg)
        {
            args.Add(arg);
            return args;
        }

        public override string ToString() => builder.ToString();
    }
}
