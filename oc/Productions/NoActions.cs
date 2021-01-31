namespace Opal.Productions
{
    /// <summary>
    /// Generates reduction statement when no action has been specified for a production
    /// </summary>
    public interface INoAction
    {
        IReduceExpr Reduce(ReduceContext context);
    }

    public class NullNoAction: INoAction
    {
        public IReduceExpr Reduce(ReduceContext context) =>
            new ReduceNullExpr();
    }

    public class FirstNoAction: INoAction
    {
        public IReduceExpr Reduce(ReduceContext context) =>
            new ReduceArgExpr(0);
    }

    public class TupleNoAction: INoAction
    {
        public IReduceExpr Reduce(ReduceContext context) =>
            new ReduceMethodExpr("Tuple.Create", context.CreateArgs());
    }
}
