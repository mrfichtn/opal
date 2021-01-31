using Opal.ParseTree;


namespace Opal.Productions
{
    public abstract class AttributeBase
    {
        public abstract IReduceExpr Reduce(ReduceContext context);
    }

    public class IgnoreAttribute: AttributeBase
    {
        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceNullExpr();
    }

    public class MethodAttribute: AttributeBase
    {
        private readonly Identifier option;

        public MethodAttribute(Identifier option) =>
            this.option = option;

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceMethodExpr(option.Value, context.CreateArgs());
    }

    public class ValueAttribute: AttributeBase
    {
        private readonly string value;

        public ValueAttribute(string value) =>
            this.value = value;

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceValueExpr(value);
    }

    public class NewAttribute: AttributeBase
    {
        private readonly string type;

        public NewAttribute(Identifier type) =>
            this.type = type.Value;

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceNewExpr(type, context.CreateArgs());
    }

    public class NoAttribute: AttributeBase
    {
        public override IReduceExpr Reduce(ReduceContext context) =>
            context.TerminalsReduce();
    }
}
