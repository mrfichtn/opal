using Opal.Containers;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Nfa
{
    public class Move
    {
        private readonly NfaNodes nodes;

        public Move(Graph graph) => nodes = graph.Machine.Nodes;


        public int Find(int classId, IEnumerable<int> nfaStates, List<int> result)
        {
            return result.SetFrom(nfaStates
                    .Select(x => nodes[x])
                    .Where(x => x.Match == classId)
                    .Select(x => x.Left))
                .Count;
        }
    }
}
