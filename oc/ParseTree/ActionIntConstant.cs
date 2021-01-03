using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionIntConstant : ActionExpr
    {
        private readonly Integer value;

        public ActionIntConstant(Integer value) =>
            this.value = value;

        public override void Write(ActionWriteContext context) =>
            context.Write(value.ToString());

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add("int");
    }
}
