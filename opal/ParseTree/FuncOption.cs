namespace Opal.ParseTree
{
    public class FuncOption: Segment
    {
        public FuncOption(Identifier id)
            : base(id)
        {
            ArgType = id;
        }

        public Identifier ArgType { get; }
    }
}
