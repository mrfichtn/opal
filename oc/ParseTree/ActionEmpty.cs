using Opal.Productions;

namespace Opal.ParseTree
{
    /// <summary>
    /// Production with no semantic action
    /// </summary>
    public class ActionEmpty: ActionExpr
    {
        public override IReductionExpr Reduce(ReduceContext context) =>
            context.ReduceEmpty();
    }
}
