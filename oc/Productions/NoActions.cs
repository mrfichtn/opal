namespace Opal.Productions
{
    public interface INoAction
    {
        void Write(ActionWriteContext context, Terminals terminals);
    }

    public class NullNoAction: INoAction
    {
        public void Write(ActionWriteContext context, Terminals terminals) =>
            context.Write("null");
    }

    public class FirstNoAction: INoAction
    {
        public void Write(ActionWriteContext context, Terminals terminals) =>
            context.Write("At(0)");
    }

    public class TupleNoAction: INoAction
    {
        public void Write(ActionWriteContext context, Terminals terminals)
        {
            var args = terminals.ArgList(context.Grammar);
            context.Write("Tuple.Create({0})", args);
        }
    }
}
