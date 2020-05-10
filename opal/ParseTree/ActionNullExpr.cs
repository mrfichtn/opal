namespace Opal.ParseTree
{
    public class ActionNullExpr : ActionExpr
    {
        public ActionNullExpr()
        {}

        public ActionNullExpr(Token t) : base(t)
        {}

        public override void Write(ActionWriteContext context) =>
            context.Write("null");
    }
}
