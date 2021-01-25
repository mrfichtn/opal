using Opal.ParseTree;
using System;

namespace Opal.Productions
{
    public class ReduceContext
    {
        private readonly TypeTable typeTable;
        private readonly ITerminals terminals;
        private readonly ActionExpr action;
        private readonly INoAction noAction;

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
            Attr = attr;
            this.noAction = noAction;
            Id = id;
        }

        public AttributeBase Attr { get; }

        public int Id { get; }

        public IReduction Reduce() => terminals.Reduce(this);

        public IReductionExpr ReductionExpr() => action.TopReduce(this);

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

        public IReductionExpr ReduceEmpty() => terminals.ReduceEmpty(this);

        public IReductionExpr AttrReduce() => Attr.Reduction(this);

        public IReductionExpr DefaultReduce(Terminals terminals) =>
            noAction.Reduce(this, terminals);
    }
}
