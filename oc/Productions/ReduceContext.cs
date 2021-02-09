using Opal.ParseTree;

namespace Opal.Productions
{
    public class ReduceContext
    {
        private readonly TypeTable typeTable;
        private readonly ITerminals terminals;
        private readonly IReducer action;
        private readonly INoAction noAction;

        public ReduceContext(TypeTable typeTable,
            ITerminals terminals,
            ActionExpr action,
            INoAction noAction,
            int id)
        {
            this.typeTable = typeTable;
            this.terminals = terminals;
            this.action = action;
            this.noAction = noAction;
            Id = id;
        }

        public int Id { get; }

        public IReduceExpr ActionReduce() => action.Reduce(this);

        public IReduceExpr TerminalsReduce() => terminals.Reduce(this);

        public IReduceExpr DefaultReduce() => noAction.Reduce(this);

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

        public IReduceExpr[] CreateArgs() => terminals.CreateArgs(this);
    }
}
