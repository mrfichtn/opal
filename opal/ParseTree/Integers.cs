namespace Opal.ParseTree
{
    public class Integer: Constant<int>
    {
        public Integer(Segment segment, int value)
            : base(segment, value)
        {
        }
    }

    public class DecInteger: Integer
    {
        public DecInteger(Token t)
            : base(t, int.Parse(t.Value))
        {
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
