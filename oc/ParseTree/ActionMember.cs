using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionMember : ActionExpr
    {
        private readonly Identifier id;
        public ActionMember(Identifier id) =>
            this.id = id;

        public override void Write(ActionWriteContext context) =>
            context.Write(id);
    }
}
