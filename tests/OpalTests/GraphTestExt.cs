using Opal.Nfa;
using System.Collections.Generic;

namespace OpalTests
{
    public static class GraphTestExt
    {
        public static int[] ToArray(this Graph graph)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            var result = new List<int>();
            if (graph.Start != -1)
            {
                queue.Enqueue(graph.Start);
                visited.Add(graph.Start);
            }
            var machine = graph.Machine;
            var nodes = machine.Nodes;
            var accepting = machine.AcceptingStates.Nodes;
            while (queue.Count > 0)
            {
                var nodeIndex = queue.Dequeue();
                var node = nodes[nodeIndex];
                result.Add(nodeIndex);
                accepting.TryGetValue(nodeIndex, out var state);
                result.Add(state);
                result.Add(node.Left);
                if (node.Left != -1)
                    result.Add(node.Match);
                result.Add(node.Right);

                if ((node.Right != -1) && visited.Add(node.Right))
                    queue.Enqueue(node.Right);
                if ((node.Left != -1) && visited.Add(node.Left))
                    queue.Enqueue(node.Left);
            }
            return result.ToArray();

        }
    }
}
