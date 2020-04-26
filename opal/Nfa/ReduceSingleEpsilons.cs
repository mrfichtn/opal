using System.Collections.Generic;
using System.Linq;

namespace Opal.Nfa
{
    public static class ReduceSingleEpsilons
    {
        public static void RemoveSingleEpsilons(this Graph graph)
        {
            var singles = graph.FindSingleEpsilons();
            var nodes = graph.Machine.Nodes;
            var count = nodes.Count;

            for (var i = 0; i < count; i++)
            {
                nodes[i].ReplaceSingleEpsilons(singles);
                //ref var node = ref nodes[i];

                //if ((node.Left != -1) && singles.TryGetValue(node.Left, out var newLeft))
                //    node.Left = newLeft;
                //if ((node.Right != -1) && singles.TryGetValue(node.Right, out var newRight))
                //    node.Right = newRight;
            }

            if (singles.TryGetValue(graph.Start, out var newStart))
                graph.Start = newStart;
        }

        private static void ReplaceSingleEpsilons(this ref NfaNode node, IDictionary<int, int> singles)
        {
            singles.ReplaceSingleEpsilon(ref node.Left);
            singles.ReplaceSingleEpsilon(ref node.Right);
        }

        private static void ReplaceSingleEpsilon(this IDictionary<int, int> singles, ref int next)
        {
            if ((next != -1) && singles.TryGetValue(next, out var newValue))
                next = newValue;
        }

        public static Dictionary<int, int> FindSingleEpsilons(this Graph graph)
        {
            var nodes = graph.Machine.Nodes;
            var nodeToAccepting = graph.Machine.AcceptingStates.Nodes;
            var singles = nodes
                .Select((node, index) => (node, index))
                .Where(x => x.node.IsSingleEpsilon && !nodeToAccepting.ContainsKey(x.index))
                .ToDictionary(x => x.index, x => x.node.Right);

            var pairs = singles.ToArray();
            foreach (var pair in pairs)
            {
                var better = pair.Value;
                while (singles.TryGetValue(better, out var next) && (better != next))
                    better = next;

                if (better != pair.Value)
                    singles[pair.Key] = better;
            }

            var count = nodes.Count;
            for (var index = 0; index < count; index++)
            {
                if (!singles.ContainsKey(index))
                {
                    ref var node = ref nodes[index];
                    if ((node.Left != -1) && singles.TryGetValue(node.Left, out int remap))
                        node.Left = remap;
                    if ((node.Right != -1) && singles.TryGetValue(node.Right, out remap))
                        node.Right = remap;
                }
            }
            return singles;
        }
    }
}
