using Opal.Productions;

namespace Opal.ParseTree
{
    public interface IReducer
    {
        IReductionExpr Reduce(ReduceContext context);
    }
}
