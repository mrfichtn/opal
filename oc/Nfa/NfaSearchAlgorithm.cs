using Opal.Containers;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Nfa
{
    /// <summary>
    /// Locates states reachable from starting state(s) through a match of @class
    /// </summary>
    public class NfaSearchAlgorithm
    {
        private readonly EpsilonClosure εClosure;
        private readonly NfaNodes nodes;

        public NfaSearchAlgorithm(Graph graph)
        {
            εClosure = new EpsilonClosure(graph);
            nodes = graph.Machine.Nodes;
        }

        public IEnumerable<int> Search(int @class, IEnumerable<int> states)
        {
            return εClosure.Find(states)
                .Select(x => nodes[x])
                .Where(x => x.Match == @class)
                .Select(x => x.Left);
        }
        public void Search(int @class, List<int> states) =>
            states.SetFrom(Search(@class, states as IEnumerable<int>));
    }
}
