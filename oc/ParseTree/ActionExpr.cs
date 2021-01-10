using Opal.Productions;

namespace Opal.ParseTree
{
    public class ActionExpr : Segment
    {
        public ActionExpr(Segment segment)
            : base(segment)
        { }

        public ActionExpr()
        { }

        public static readonly ActionEmpty Empty = new ActionEmpty();

        /// <summary>
        /// Writes action code
        /// </summary>
        /// <param name="context">Write context</param>
        public virtual void Write(ActionWriteContext context)
        { 
        }


        public virtual void AddType(DefinitionActionTypeContext context)
        {
        }
    }
}
