using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionIntConstant : ActionExpr
    {
        private readonly Integer value;

        public ActionIntConstant(Integer value) =>
            this.value = value;

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add("int");

        public override IReductionExpr Reduce(ReduceContext context) =>
            new ValueReductionExpr(value.ToString()!);
    }
}
