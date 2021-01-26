using Opal.ParseTree;


namespace Opal.Productions
{
    public abstract class AttributeBase
    {
        public abstract IReductionExpr Reduce(ReduceContext context);
    }

    public class IgnoreAttribute: AttributeBase
    {
        public override IReductionExpr Reduce(ReduceContext context) =>
            new NullReductionExpr();
    }

    public class MethodAttribute: AttributeBase
    {
        private readonly Identifier option;

        public MethodAttribute(Identifier option) =>
            this.option = option;

        public override IReductionExpr Reduce(ReduceContext context) =>
            new MethodReductionExpr(option.Value, context.CreateArgs());
    }

    public class ValueAttribute: AttributeBase
    {
        private readonly string value;

        public ValueAttribute(string value) =>
            this.value = value;

        public override IReductionExpr Reduce(ReduceContext context) =>
            new ValueReductionExpr(value);
    }

    public class NewAttribute: AttributeBase
    {
        private readonly string type;

        public NewAttribute(Identifier type) =>
            this.type = type.Value;

        public override IReductionExpr Reduce(ReduceContext context) =>
            new NewReductionExpr(type, context.CreateArgs());
    }

    public class NoAttribute: AttributeBase
    {
        public override IReductionExpr Reduce(ReduceContext context) =>
            context.TerminalsReduce();
    }
}
