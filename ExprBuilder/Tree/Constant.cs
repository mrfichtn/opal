using System.Linq.Expressions;

namespace ExprBuilder.Tree
{
    public class Constant<T>: Expr
    {
        public Constant(Segment seg, T value)
            : base(seg)
        {
            Value = value;
        }

        public T Value { get; }

        public override Expression CreateExpression(Context ctx)
        {
            return Expression.Constant(Value);
        }
    }
}
