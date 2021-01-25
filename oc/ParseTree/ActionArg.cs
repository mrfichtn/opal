using Opal.Productions;
using System.Text;

namespace Opal.ParseTree
{
    public class ActionArg : ActionExpr
    {
        protected readonly int position;

        public ActionArg(Token t) 
            : base(t)
        {
            position = int.Parse(t.Value.Substring(1));
        }


        public override void AddType(DefinitionActionTypeContext context) =>
            context.AddFromActionExpr(position);

        public override IReductionExpr Reduce(ReduceContext context)
        {
            return context.TryFindProductionType(position, out var type) ?
                new CastedArgReductionExpr(position, type!) :
                new ArgReductionExpr(position);
        }

        public override IReductionExpr TopReduce(ReduceContext context) =>
            new ArgReductionExpr(position);

        public override string ToString() =>
            new StringBuilder('$')
                .Append(position)
                .ToString();
    }
}
