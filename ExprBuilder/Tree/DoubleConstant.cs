namespace ExprBuilder.Tree
{
    public class DoubleConstant : Constant<double>
    {
        public DoubleConstant(Token t)
            : base(t, double.Parse(t.Value))
        {
        }

        public DoubleConstant(Segment t, double value)
            : base(t, value)
        {
        }

        public static DoubleConstant ParseInt(Token t)
        {
            var text = t.Value.Substring(0, t.Value.Length - 1);
            var value = double.Parse(text);
            return new DoubleConstant(t, value);
        }
    }
}
