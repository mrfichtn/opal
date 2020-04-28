using System.Collections.Generic;

namespace Opal.Nfa
{
    public class NfaSearch: NfaSearchAlgorithm
    {
        public NfaSearch(Graph graph)
            : base(graph)
        {
            States = new List<int> { graph.Start };
        }

        public List<int> States { get; }

        public int Search(int @class)
        {
            Search(@class, States);
            return States.Count;
        }
    }
}
