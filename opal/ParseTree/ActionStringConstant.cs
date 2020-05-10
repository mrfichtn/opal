using Generators;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ActionStringConstant : ActionExpr
    {
        private readonly StringConst value;

        public ActionStringConstant(StringConst value) =>
            this.value = value;

        public override void Write(ActionWriteContext context) =>
            context.Write(value.ToString());

        public override void GetTypes(HashSet<string> types) =>
            types.Add("string");
    }
}
