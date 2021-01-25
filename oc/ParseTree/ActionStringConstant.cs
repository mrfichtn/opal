using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionStringConstant : ActionExpr
    {
        private readonly StringConst value;

        public ActionStringConstant(StringConst value) =>
            this.value = value;

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add("string");

        public override IReductionExpr Reduce(ReduceContext context) =>
            new StringReductionExpr(value.Value);
    }
}
