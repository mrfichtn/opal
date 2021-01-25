using Opal.Productions;
using System.Text;

namespace Opal.ParseTree
{
    public class CastedActionArg: ActionArg
    {
        public CastedActionArg(Token t, Identifier cast)
            : base(t)
        {
            Cast = cast;
        }

        public Identifier Cast { get; private set; }

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add(Cast.Value);

        public override string ToString() =>
            new StringBuilder('$')
                .Append(position)
                .Append(Cast.Value)
                .ToString();

        public override IReductionExpr TopReduce(ReduceContext context) =>
            Reduce(context);

        public override IReductionExpr Reduce(ReduceContext context)
        {
            context.TryFindProductionType(position, out var productionType);

            //If true, appends .Value to token to access token value
            //  (production type is token and the user specified a 'string' cast)
            if (Cast.Value == "string")
            {
                return (productionType == "Token") ?
                    new FieldReductionExpr(new CastedArgReductionExpr(position, "Token"), "Value") :
                    new CastedArgReductionExpr(position, Cast.Value);
            }
            else if (Cast.Value == "object")
            {
                return new ArgReductionExpr(position);
            }
            else
            {
                return new CastedArgReductionExpr(position, Cast.Value);
            }
        }
    }
}
