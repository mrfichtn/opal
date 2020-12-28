namespace Opal.ParseTree
{
    public class FuncOption: Segment
    {
        public FuncOption(Token t, Identifier? id)
            : base(t)
        {
            ArgType = id;
        }

        public Identifier? ArgType { get; }

        public override string ToString()
        {
            return (ArgType != null) ?
                $"({ArgType})" :
                $"()";
        }
    }
}
