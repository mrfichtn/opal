using Opal.Productions;

namespace Opal.ParseTree
{
    /// <summary>
    /// Production with no semantic action
    /// </summary>
    public class ActionEmpty: ActionExpr
    {
        public override void Write(ActionWriteContext context) =>
            context.Production.Right.WriteForEmptyAction(context);
    }
}
