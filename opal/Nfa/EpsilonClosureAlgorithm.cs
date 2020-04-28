using Opal.Containers;
using System.Collections.Generic;

namespace Opal.Nfa
{
    public class EpsilonClosureAlgorithm
    {
        private readonly NfaNodes nodes;

        public EpsilonClosureAlgorithm(Graph graph) =>
            nodes = graph.Machine.Nodes;


        /// <summary>
        /// Returns a list of all states reachable from starting state nfa
        /// </summary>
        /// <param name="startSet"></param>
        /// <returns></returns>
        public void Find(IEnumerable<int> startSet, HashSet<int> result)
        {
            // Initialize result with T because each state
            // has epsilon closure to itself
            result.SetFrom(startSet);

            // Push all states onto the stack
            var unprocessedStack = new Stack<int>(startSet);

            // While the unprocessed stack is not empty
            while (unprocessedStack.Count > 0)
            {
                // Pop t, the top element from unprocessed stack
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
