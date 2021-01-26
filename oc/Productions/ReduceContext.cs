using Opal.ParseTree;

namespace Opal.Productions
{
    public class ReduceContext
    {
        private readonly TypeTable typeTable;
        private readonly ITerminals terminals;
        private readonly IReducer action;
        private readonly INoAction noAction;

        private readonly AttributeBase attr;

        public ReduceContext(TypeTable typeTable,
            ITerminals terminals,
            ActionExpr action,
            AttributeBase attr,
            INoAction noAction,
            int id)
        {
            this.typeTable = typeTable;
            this.terminals = terminals;
            this.action = action;
            this.attr = attr;
            this.noAction = noAction;
            Id = id;
        }

        public int Id { get; }

        public IReduction Reduce() => terminals.Reduction(this);

        public IReductionExpr ActionReduce() => action.Reduce(this);

        public IReductionExpr AttrReduce() => attr.Reduce(this);

        public IReductionExpr TerminalsReduce() => terminals.Reduce(this);

        public IReductionExpr DefaultReduce() => noAction.Reduce(this);

        public bool TryFindProductionType(int position, out string? type)
        {
            var found = (position < terminals.Length);
            if (found)
                found = TryFindType(terminals[position].Name, out type);
            else
                type = null;
            return found;
        }

        public bool TryFindType(string name, out string? type) =>
            typeTable.TryFind(name, out type);

        public IReductionExpr[] CreateArgs() => terminals.CreateArgs(this);
    }
}
