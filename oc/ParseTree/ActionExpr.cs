using Opal.Productions;

namespace Opal.ParseTree
{
    public abstract class ActionExpr : Segment, IReducer
    {
        protected ActionExpr(Segment segment)
            : base(segment)
        { }

        protected ActionExpr()
        { }

        public static readonly ActionEmpty Empty = new ActionEmpty();

        public abstract IReductionExpr Reduce(ReduceContext context);

        public virtual void AddType(DefinitionActionTypeContext context)
        {
        }
    }
}
