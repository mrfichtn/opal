using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionFuncExpr : ActionExpr
    {
        protected readonly Identifier id;
        protected readonly ActionArgs args;
        
        public ActionFuncExpr(Identifier id, ActionArgs args)
            : base(id)
        {
            this.id = id;
            this.args = args;
        }

        public override IReductionExpr Reduce(ReduceContext context) =>
            new MethodReductionExpr(id.Value,
                args.Reduce(context));
    }
}
