using Opal.ParseTree;

namespace Opal.Productions
{
    /// <summary>
    /// Generates reduction statement when no action has been specified for a production
    /// </summary>
    public interface INoAction: IReducer
    {
    }

    public class NullNoAction: INoAction
    {
        public IReductionExpr Reduce(ReduceContext context) =>
            new NullReductionExpr();
    }

    public class FirstNoAction: INoAction
    {
        public IReductionExpr Reduce(ReduceContext context) =>
            new ArgReductionExpr(0);
    }

    public class TupleNoAction: INoAction
    {
        public IReductionExpr Reduce(ReduceContext context) =>
            new MethodReductionExpr("Tuple.Create", context.CreateArgs());
    }
}
