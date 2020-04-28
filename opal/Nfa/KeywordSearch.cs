namespace Opal.Nfa
{
    public class KeywordSearch
    {
        private readonly Graph graph;
        
        public KeywordSearch(Graph graph) => this.graph = graph;
        
        public int Search(string keyword)
        {
            var result = -1;

            var machine = graph.Machine;
            var matches = machine.Matches;
            var nodes = machine.Nodes;

            var nfaSearch = new NfaSearch(graph);
            foreach (var ch in keyword)
            {
                //Find class for character
                if (!matches.TryGet(ch, out var @class) ||
                    (nfaSearch.Search(@class) == 0))
                    return -1;
            }

            var acceptingStates = machine.AcceptingStates.Nodes;
            foreach (var nodeId in nfaSearch.States)
            {
                if (acceptingStates.TryGetValue(nodeId, out var acceptingState)
                    && !nodes.HasTransition(nodeId))
                {
                    result = acceptingState;
                    break;
                }
            }

            return result;
        }
    }
}
