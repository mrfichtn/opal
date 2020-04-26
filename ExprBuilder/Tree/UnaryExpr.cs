using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExprBuilder.Tree
{
    public abstract class UnaryExpr: Expr
    {
        public UnaryExpr(Segment seg, Expr operand)
            : base(seg)
        {
            Operand = operand;
        }

        public Expr Operand { get; }

        public override Expression CreateExpression(Context ctx)
        {
            var expr = Operand.CreateExpression(ctx);
            return CreateExpression(ctx, expr);
        }

        protected abstract Expression CreateExpression(Context ctx, Expression expr);
    }

    public class NotUnary: UnaryExpr
    {
        public NotUnary(Segment seg, Expr operand) : base(seg, operand) { }

        protected override Expression CreateExpression(Context ctx, Expression expr)
        {
            return Expression.Not(expr);
        }
    }

    public class NegateUnary : UnaryExpr
    {
        public NegateUnary(Segment seg, Expr operand) : base(seg, operand) { }

        protected override Expression CreateExpression(Context ctx, Expression expr)
        {
            return Expression.Negate(expr);
        }
    }

    public class OnesComplementUnary : UnaryExpr
    {
        public OnesComplementUnary(Segment seg, Expr operand) : base(seg, operand) { }

        protected override Expression CreateExpression(Context ctx, Expression expr)
        {
            return Expression.OnesComplement(expr);
        }
    }

}
