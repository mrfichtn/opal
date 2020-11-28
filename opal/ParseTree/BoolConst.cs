namespace Opal.ParseTree
{
    public class BoolConst: Constant<bool>
    {
        public BoolConst(Segment segment, bool value)
            : base(segment, value)
        { }

        public override string ToString() => 
            Value ? "true" : "false";
    }
}
