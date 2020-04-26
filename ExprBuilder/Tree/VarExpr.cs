using System;
using System.Linq.Expressions;

namespace ExprBuilder.Tree
{
    public class VarExpr:Expr
    {
        public VarExpr(Identifier id)
            : base(id)
        {
            Value = id.Value;
        }

        public string Value { get; }

        public override Expression CreateExpression(Context ctx)
        {
            if (!ctx.TryGetVariable(Value, out var expr))
                throw new Exception(string.Format("Missing variable {0}", Value));

            return expr;
        }
    }
}
