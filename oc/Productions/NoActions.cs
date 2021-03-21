namespace Opal.Productions
{
    /// <summary>
    /// Generates reduction statement when no action has been specified for a production
    /// </summary>
    public interface INoAction
    {
        void AddType(ParseTree.DefinitionActionTypeContext context);
        IReduceExpr Reduce(ReduceContext context);
    }

    public class NullNoAction: INoAction
    {
        public void AddType(ParseTree.DefinitionActionTypeContext context) =>
            context.Add(null);

        public IReduceExpr Reduce(ReduceContext context) =>
            new ReduceNullExpr();
    }

    public class FirstNoAction: INoAction
    {
        public void AddType(ParseTree.DefinitionActionTypeContext context) =>
            context.AddFromActionExpr(0);

        public IReduceExpr Reduce(ReduceContext context) =>
            new ReduceArgExpr(0);


    }

    public class TupleNoAction: INoAction
    {
        public void AddType(ParseTree.DefinitionActionTypeContext context) =>
            context.Add("Tuple<>");
        
        public IReduceExpr Reduce(ReduceContext context) =>
            new ReduceMethodExpr("Tuple.Create", context.CreateArgs());
    }
}
