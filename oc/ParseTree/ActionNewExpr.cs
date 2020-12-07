using Generators;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ActionNewExpr : ActionFuncExpr
    {
        public ActionNewExpr(Identifier id, ActionArgs args)
            : base(id, args)
        {
        }

        public override void Write(ActionWriteContext context)
        {
            context.Write("new ");
            base.Write(context);
        }

        public override void GetTypes(HashSet<string> types) => types.Add(id.Value);
    }
}
