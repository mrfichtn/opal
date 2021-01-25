using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionNewExpr : ActionFuncExpr
    {
        public ActionNewExpr(Identifier id, ActionArgs args)
            : base(id, args)
        {
        }

        public override IReductionExpr Reduce(ReduceContext context) =>
            new NewReductionExpr(id.Value, args.Reduce(context));

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add(id.Value);
    }
}
