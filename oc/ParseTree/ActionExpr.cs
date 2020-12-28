using Opal.Productions;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ActionExpr : Segment
    {
        public ActionExpr(Segment segment)
            : base(segment)
        { }

        public ActionExpr()
        { }

        /// <summary>
        /// Writes action code
        /// </summary>
        /// <param name="context">Write context</param>
        public virtual void Write(ActionWriteContext context)
        { }

        /// <summary>
        /// Returns types found in action expression
        /// </summary>
        /// <param name="types"></param>
        public virtual void GetTypes(HashSet<string> types)
        { }

        public virtual bool TryGetType(out string? type)
        {
            type = null;
            return false;
        }
    }
}
