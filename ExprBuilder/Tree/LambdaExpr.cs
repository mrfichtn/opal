using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExprBuilder.Tree
{
    public class LambdaExpr : Expr
    {
        private Identifier _id;
        private Expr _body;

        public LambdaExpr(Token t, Expr expr)
            : base(t)
        {
            _id = new Identifier(t);
            End = expr.End;
            _body = expr;
        }

        public override Expression CreateExpression(Context ctx)
        {
            var p = ctx.AddVariable(_id.Value, typeof(object));
            var body = _body.CreateExpression(ctx);
            ctx.RmVariable(_id.Value);
            return Expression.Lambda(body, p);
        }
    }
}
