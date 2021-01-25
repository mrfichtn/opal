using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionBoolConstant: ActionExpr
    {
        private readonly BoolConst value;
        public ActionBoolConstant(BoolConst value) =>
            this.value = value;

        public override IReductionExpr Reduce(ReduceContext context) =>
            new ValueReductionExpr(value.ToString());

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add("bool");
    }
}
