using Opal.CodeGenerators;
using Opal.Containers;
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
            context.TryFind(position, this);

        /// <summary>
        /// True if written from production
        /// </summary>
        public override void Write(ActionWriteContext context)
        {
            //Attempt to find a default type
            var productionType = context.FindProductionType(position);

            context.Write("At");

            if ((productionType != null) && !context.Root)
                context.Write("<{0}>", productionType);

            context.Write($"({position})");
        }

        public override string ToString() =>
            new StringBuilder('$')
                .Append(position)
                .ToString();
    }
}
