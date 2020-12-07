using Opal.CodeGenerators;
using Opal.Containers;
using System.Text;

namespace Opal.ParseTree
{
    public class ActionArg : ActionExpr
    {
        private readonly int position;

        public ActionArg(Token t) : base(t)
        {
            position = int.Parse(t.Value![1..]);
        }

        /// <summary>
        /// action_primary_expr = arg action_cast		{ new ActionArg($0, $1: Identifier); }
        /// </summary>
        /// <param name="t">Token</param>
        /// <param name="cast">Cast</param>
        public ActionArg(Token t, Identifier cast)
            : this(t)
        {
            Cast = cast;
        }

        public Identifier? Cast { get; private set; }

        /// <summary>
        /// True if written from production
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="prods"></param>
        /// <param name="prod"></param>
        /// <param name="root"></param>
        public override void Write(ActionWriteContext context)
        {
            //Attempt to find a default type
            var productionType = context.FindProductionType(position);

            //If true, appends .Value to token to access token value
            //  (production type is token and the user specified a 'string' cast)
            var tokenValue = false;

            context.Write("At");

            if (Cast == null)
                NoCast(context, productionType);
            else if (Cast.Value == "string")
                tokenValue = StringCast(context, productionType);
            else if (Cast.Value != "object")
                ObjectCast(context);
            
            context.Write($"({position})")
                .WriteIf(tokenValue, ".Value");
        }

        public static void NoCast(ActionWriteContext context, string? productionType)
        {
            if ((productionType != null) && !context.Root)
                context.Write("<{0}>", productionType);
        }
        public bool StringCast(ActionWriteContext context, string? productionType)
        {
            context.Write('<');
            var tokenValue = (productionType == "Token");
            if (tokenValue)
                context.Write(productionType!);
            else
                context.Write(Cast!);
            context.Write('>');
            return tokenValue;
        }

        public void ObjectCast(ActionWriteContext context) =>
            context.Write('<').Write(Cast!).Write(">");

        public override string ToString() =>
            new StringBuilder('$')
                .Append(position)
                .AppendIf(Cast != null, Cast!.Value)
                .ToString();
    }
}
