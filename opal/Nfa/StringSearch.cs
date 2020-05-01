using System.Collections.Generic;
using System.Linq;

namespace Opal.Nfa
{
    /// <summary>
    /// Returns states that match search string
    /// </summary>
    public class StringSearch
    {
        private readonly Graph graph;

        public StringSearch(Graph graph)
        {
            this.graph = graph;
        }

        public IEnumerable<int> Search(string text)
        {
            var machine = graph.Machine;
            var matches = machine.Matches;

            var nfaSearch = new NfaSearch(graph);
            foreach (var ch in text)
            {
                //Find class for character
                if (!matches.TryGet(ch, out var @class) || (nfaSearch.Search(@class) == 0))
                    return Enumerable.Empty<int>();
            }
            return nfaSearch.States;
        }

    }
}
