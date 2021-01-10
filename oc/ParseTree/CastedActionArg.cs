using Opal.CodeGenerators;
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

        /// <summary>
        /// True if written from production
        /// </summary>
        public override void Write(ActionWriteContext context)
        {
            //Attempt to find a default type
            var productionType = context.FindProductionType(position);

            //If true, appends .Value to token to access token value
            //  (production type is token and the user specified a 'string' cast)
            var tokenValue = false;

            context.Write("At");

            if (Cast.Value == "string")
                tokenValue = StringCast(context, productionType);
            else if (Cast.Value != "object")
                ObjectCast(context);

            context.Write($"({position})")
                .WriteIf(tokenValue, ".Value");
        }

        private bool StringCast(ActionWriteContext context, string? productionType)
        {
            context.Write('<');
            var tokenValue = (productionType == "Token");
            if (tokenValue)
                context.Write(productionType!);
            else
                context.Write(Cast);
            context.Write('>');
            return tokenValue;
        }

        private void ObjectCast(ActionWriteContext context) =>
            context.Write('<').Write(Cast).Write(">");

        public override string ToString() =>
            new StringBuilder('$')
                .Append(position)
                .Append(Cast.Value)
                .ToString();
    }
}
