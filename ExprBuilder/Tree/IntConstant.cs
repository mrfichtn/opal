namespace ExprBuilder.Tree
{
    public class IntConstant: Constant<int>
    {
        public IntConstant(Token t)
            : base(t, int.Parse(t.Value))
        {
        }
    }
}
