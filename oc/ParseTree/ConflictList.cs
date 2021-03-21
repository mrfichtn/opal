using System.Collections;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ConflictList: IEnumerable<Conflict>
    {
        private readonly List<Conflict> data;

        public ConflictList()
        {
            data = new List<Conflict>();
        }
        
        public static ConflictList Add(ConflictList list, Conflict conflict)
        {
            list.data.Add(conflict);
            return list;
        }

        public IEnumerator<Conflict> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
