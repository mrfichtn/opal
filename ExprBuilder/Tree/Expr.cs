using System;
using System.Linq.Expressions;

namespace ExprBuilder.Tree
{
    public abstract class Expr : Segment
    {
        public Expr(Segment seg)
            : base(seg)
        {
        }

        public virtual void Resolve(Context ctx)
        { }

        public abstract Expression CreateExpression(Context ctx);
    }
}
