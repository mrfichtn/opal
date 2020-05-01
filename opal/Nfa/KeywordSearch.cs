namespace Opal.Nfa
{
    /// <summary>
    /// Searches NFA for a keyword, returning accepting state if found
    /// </summary>
    public class KeywordSearch
    {
        private readonly Graph graph;
        
        public KeywordSearch(Graph graph) => this.graph = graph;
        
        public int Search(string keyword)
        {
            var foundStates = new StringSearch(graph).Search(keyword);
            
            var machine = graph.Machine;
            var nodes = machine.Nodes;
            var acceptingStates = machine.AcceptingStates.Nodes;
            foreach (var nodeId in foundStates)
            {
                if (acceptingStates.TryGetValue(nodeId, out var acceptingState)
                    && !nodes.HasTransition(nodeId))
                {
                    return acceptingState;
                }
            }
            return -1;
        }
    }
}
