using Opal.Productions;

namespace Opal.ParseTree
{
    public interface IReducer
    {
        IReduceExpr Reduce(ReduceContext context);
    }
}
