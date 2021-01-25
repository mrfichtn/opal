using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionMember : ActionExpr
    {
        private readonly Identifier id;
        public ActionMember(Identifier id) =>
            this.id = id;

        public override IReductionExpr Reduce(ReduceContext context) =>
            new ValueReductionExpr(id.Value);
    }
}
