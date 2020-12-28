using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ConflictList: List<Conflict>
    {
        public static ConflictList Add(ConflictList list, Conflict conflict)
        {
            list.Add(conflict);
            return list;
        }
    }
}
