using Opal.ParseTree;


namespace Opal.Productions
{
    public abstract class AttributeBase
    {
        public virtual void WriteEmptyAction(ActionWriteContext context) =>
            context.Write("null");

        public virtual void WriteEmptyAction(ActionWriteContext context,
            SingleTerminal terminal) =>
            context.Write("At(0)");
        
        public abstract void WriteEmptyAction(ActionWriteContext context, 
            Terminals terminals);
    }

    public class IgnoreAttribute: AttributeBase
    {
        public override void WriteEmptyAction(ActionWriteContext context, 
            SingleTerminal terminal)
            => context.Write("null");

        public override void WriteEmptyAction(ActionWriteContext context,
            Terminals terminals) =>
            context.Write("null");
    }

    public class MethodAttribute: AttributeBase
    {
        private readonly Identifier option;

        public MethodAttribute(Identifier option) =>
            this.option = option;

        public override void WriteEmptyAction(ActionWriteContext context, 
            SingleTerminal terminal)
        {
            context
                .Write(option)
                .Write('(')
                .Write(terminal.Arg(context.Grammar))
                .Write(")");
        }

        public override void WriteEmptyAction(ActionWriteContext context,
            Terminals terminals)
        {
            var args = terminals.ArgList(context.Grammar);
            context
                .Write(option)
                .Write('(')
                .Write(args)
                .Write(")");
        }
    }

    public class ValueAttribute: AttributeBase
    {
        private readonly string value;

        public ValueAttribute(string value) =>
            this.value = value;

        public override void WriteEmptyAction(ActionWriteContext context) =>
            context.Write("{0}", value);

        public override void WriteEmptyAction(ActionWriteContext context, SingleTerminal _) 
            => WriteEmptyAction(context);

        public override void WriteEmptyAction(ActionWriteContext context, Terminals _) => 
            WriteEmptyAction(context);
    }

    public class NewAttribute: AttributeBase
    {
        private readonly string type;

        public NewAttribute(Identifier type) =>
            this.type = type.Value;

        public override void WriteEmptyAction(ActionWriteContext context, SingleTerminal terminal)
        {
            context.Write("new {0}(", type)
                    .Write(terminal.Arg(context.Grammar))
                    .Write(")");
        }

        public override void WriteEmptyAction(ActionWriteContext context, 
            Terminals terminals)
        {
            var args = terminals.ArgList(context.Grammar);
            context.Write("new {0}(", type)
                    .Write(args)
                    .Write(")");
        }
    }

    public class NoAttribute: AttributeBase
    {
        public override void WriteEmptyAction(ActionWriteContext context, 
            Terminals terminals) =>
            context.NoAction.Write(context, terminals);
    }
}
