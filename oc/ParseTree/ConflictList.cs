using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ConflictList: List<Conflict>
    {
        public new ConflictList Add(Conflict conflict)
        {
            base.Add(conflict);
            return this;
        }
    }
}
