using System.Text;

namespace Opal.ParseTree
{
    public class GenericArgs: Segment
    {
        private readonly StringBuilder _builder;

        public GenericArgs(Identifier arg)
            : base(arg)
        {
            _builder = new StringBuilder(arg.Value);
        }

        public void Add(Identifier arg)
        {
            _builder.Append(',')
                .Append(arg);
            End = arg.End;
        }

        public static GenericArgs Add(GenericArgs args, Identifier arg)
        {
            args.Add(arg);
            return args;
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
