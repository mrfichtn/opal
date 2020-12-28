using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionFuncExpr : ActionExpr
    {
        protected readonly Identifier id;
        private readonly ActionArgs args;
        
        public ActionFuncExpr(Identifier id, ActionArgs args)
            : base(id)
        {
            this.id = id;
            this.args = args;
        }

        public override void Write(ActionWriteContext context)
        {
            context
                .Write(id)
                .Write('(')
                .Write(args)
                .Write(')');
        }
    }
}
