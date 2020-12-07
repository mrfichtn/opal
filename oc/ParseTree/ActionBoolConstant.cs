using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ActionBoolConstant: ActionExpr
    {
        private readonly BoolConst value;
        public ActionBoolConstant(BoolConst value) =>
            this.value = value;

        public override void Write(ActionWriteContext context) =>
            context.Write(value.ToString());

        public override void GetTypes(HashSet<string> types) =>
            types.Add("bool");
    }
}
