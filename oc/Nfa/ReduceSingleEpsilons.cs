using System.Collections.Generic;
using System.Linq;

namespace Opal.Nfa
{
    public static class ReduceSingleEpsilons
    {
        /// <summary>
        /// Looks for nodes that are transitional only and replaces them with other 
        /// side of the link
        /// Replaces:
        /// { node } -- (left/right) -->  {transition-node} ---right--->  { other-node }
        /// With:
        /// { node } ---> { other-node }
        /// </summary>
        public static void RemoveSingleEpsilons(this Graph graph)
        {
            var singles = graph.FindSingleEpsilons();
            var nodes = graph.Machine.Nodes;
            var count = nodes.Count;

            for (var i = 0; i < count; i++)
                singles.ReplaceSingleEpsilons(ref nodes[i]);

            graph.Start = singles.Replace(graph.Start);
        }

        private static void ReplaceSingleEpsilons(this IDictionary<int, int> singles, ref NfaNode node)
        {
            node.Left = singles.ReplaceSingleEpsilon(node.Left);
            node.Right = singles.ReplaceSingleEpsilon(node.Right);
        }

        private static int ReplaceSingleEpsilon(this IDictionary<int, int> singles, int next) =>
            ((next != -1) && singles.TryGetValue(next, out var newValue)) ? newValue : next;

        private static int Replace(this IDictionary<int, int> singles, int nodeId) =>
            singles.TryGetValue(nodeId, out var newValue) ? newValue : nodeId;

        public static Dictionary<int, int> FindSingleEpsilons(this Graph graph)
        {
            var machine = graph.Machine;
            var nodes = machine.Nodes;
            var nodeToAccepting = machine.AcceptingStates.Nodes;
            return nodes
                .Select((node, index) => (node, index))
                .Where(x => x.node.IsSingleEpsilon && !nodeToAccepting.ContainsKey(x.index))
                .ToDictionary(x => x.index, x => x.node.Right)
                .OptimizeEpsilonMap();
        }

        private static Dictionary<int, int> OptimizeEpsilonMap(this Dictionary<int, int> singles)
        {
            //Find end of chain
            var pairs = singles.ToArray();
            foreach (var pair in pairs)
            {
                var better = pair.Value;
                while (singles.TryGetValue(better, out var next) && (better != next))
                    better = next;

                if (better != pair.Value)
                    singles[pair.Key] = better;
            }
            return singles;
        }
    }
}
