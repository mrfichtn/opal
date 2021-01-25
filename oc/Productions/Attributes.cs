using Opal.ParseTree;


namespace Opal.Productions
{
    public abstract class AttributeBase
    {
        public virtual IReductionExpr Reduction(ReduceContext context) =>
            new NullReductionExpr();

        public virtual IReductionExpr Reduction(ReduceContext context,
            SingleTerminal terminal) =>
            new ArgReductionExpr(0);

        public abstract IReductionExpr Reduction(ReduceContext context,
            Terminals terminals);
    }

    public class IgnoreAttribute: AttributeBase
    {
        public override IReductionExpr Reduction(ReduceContext context,
            SingleTerminal terminal) =>
            new NullReductionExpr();

        public override IReductionExpr Reduction(ReduceContext context,
            Terminals terminals) =>
            new NullReductionExpr();
    }

    public class MethodAttribute: AttributeBase
    {
        private readonly Identifier option;

        public MethodAttribute(Identifier option) =>
            this.option = option;

        public override IReductionExpr Reduction(ReduceContext context, 
            SingleTerminal terminal) =>
            new MethodReductionExpr(option.Value, terminal.Reduction(context));

        public override IReductionExpr Reduction(ReduceContext context,
            Terminals terminals) =>
            new MethodReductionExpr(option.Value,
                terminals.Reduction(context));
    }

    public class ValueAttribute: AttributeBase
    {
        private readonly string value;

        public ValueAttribute(string value) =>
            this.value = value;

        public override IReductionExpr Reduction(ReduceContext context) =>
            new ValueReductionExpr(value);

        public override IReductionExpr Reduction(ReduceContext context, 
            SingleTerminal terminal) =>
            Reduction(context);

        public override IReductionExpr Reduction(ReduceContext context, 
            Terminals terminals) =>
            Reduction(context);
    }

    public class NewAttribute: AttributeBase
    {
        private readonly string type;

        public NewAttribute(Identifier type) =>
            this.type = type.Value;

        public override IReductionExpr Reduction(ReduceContext context,
            SingleTerminal terminal) =>
            new NewReductionExpr (type, terminal.Reduction(context));

        public override IReductionExpr Reduction(ReduceContext context,
            Terminals terminals) =>
            new NewReductionExpr(type,
                terminals.Reduction(context));
    }

    public class NoAttribute: AttributeBase
    {
        public override IReductionExpr Reduction(ReduceContext context, 
            Terminals terminals) =>
            context.NoAction.Reduce(context, terminals);
    }
}
