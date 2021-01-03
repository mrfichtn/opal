using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionStringConstant : ActionExpr
    {
        private readonly StringConst value;

        public ActionStringConstant(StringConst value) =>
            this.value = value;

        public override void Write(ActionWriteContext context) =>
            context.Write(value.ToString());

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add("string");
    }
}
