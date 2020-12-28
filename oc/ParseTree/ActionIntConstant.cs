using Opal.Productions;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ActionIntConstant : ActionExpr
    {
        private readonly Integer value;

        public ActionIntConstant(Integer value) =>
            this.value = value;

        public override void Write(ActionWriteContext context) =>
            context.Write(value.ToString());

        public override void GetTypes(HashSet<string> types) =>
            types.Add("int");

        public override bool TryGetType(out string? type)
        {
            type = "int";
            return true;
        }
    }
}
