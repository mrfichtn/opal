using Opal.Containers;
using System.Collections.Generic;

namespace Opal.Nfa
{
    /// <summary>
    /// Calculates nodes reachable from a starting collection of nodes
    /// </summary>
    public class EpsilonClosureAlgorithm
    {
        private readonly NfaNodes nodes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graph">NFA graph</param>
        public EpsilonClosureAlgorithm(Graph graph) =>
            nodes = graph.Machine.Nodes;

        /// <summary>
        /// Returns a node set reachable from a starting collection
        /// </summary>
        /// <param name="startSet">Starting collection of node ids</param>
        /// <param name="result">Set of reachable node ids</param>
        public void Find(IEnumerable<int> startSet, HashSet<int> result)
        {
            // Initialize result with T because each state has ε-closure to itself
            result.SetFrom(startSet);

            for (var unprocessedStack = new Stack<int>(startSet); unprocessedStack.Count > 0; )
            {
                var node = nodes[unprocessedStack.Pop()];

                // Get all epsilon transition for this state
                if (node.Right != -1)
                {
                    if (result.Add(node.Right))
                        unprocessedStack.Push(node.Right);

                    if ((node.Left != -1) && (node.Match == -1) && result.Add(node.Left))
                        unprocessedStack.Push(node.Left);
                }
            }
        }
    }
}
