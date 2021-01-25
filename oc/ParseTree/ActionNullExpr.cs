using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionNullExpr : ActionExpr
    {
        public ActionNullExpr()
        {}

        public ActionNullExpr(Token t) : base(t)
        {}

        public override IReductionExpr Reduce(ReduceContext context) =>
            new NullReductionExpr();
    }
}
