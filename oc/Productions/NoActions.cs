namespace Opal.Productions
{
    /// <summary>
    /// Generates reduction statement when no action has been specified for a production
    /// </summary>
    public interface INoAction
    {
        IReductionExpr Reduce(ReduceContext context, Terminals terminals);
    }

    public class NullNoAction: INoAction
    {
        public IReductionExpr Reduce(ReduceContext context, Terminals terminals) =>
            new NullReductionExpr();
    }

    public class FirstNoAction: INoAction
    {
        public IReductionExpr Reduce(ReduceContext context, Terminals terminals) =>
            new ArgReductionExpr(0);
    }

    public class TupleNoAction: INoAction
    {
        public IReductionExpr Reduce(ReduceContext context, Terminals terminals) =>
            new MethodReductionExpr("Tuple.Create",
                terminals.Reduction(context));
    }
}
