using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Nfa
{
    public class NfaNodes: IEnumerable<NfaNode>
    {
        private NfaNode[] data;

        public NfaNodes(int capacity = 64) => data = new NfaNode[capacity];

        #region Properties

        public ref NfaNode this[int index] => ref data[index];

        public int Count { get; private set; }

        #endregion

        /// <summary>
        /// Allocates an empty node, to be set later
        /// </summary>
        /// <returns></returns>
        public int New()
        {
            if (Count == data.Length)
                Array.Resize(ref data, Count * 2);
            return Count++;
        }

        /// <summary>
        /// Creates a new end node
        /// </summary>
        /// <returns></returns>
        public int CreateEnd() => Add(-1, -1, -1);

        public int CreateMatch(int match, int next, int epsilon=-1) =>
            Add(match, next, epsilon);

        public int Create(int right, int left = -1) =>
            Add(-1, left, right);

        public void Set(int nodeId, int match, int left, int right) =>
            data[nodeId] = new NfaNode(match, left, right);

        public void SetRight(int nodeId, int value) => data[nodeId].Right = value;

        public bool HasTransition(int index)
        {
            var hasFound = false;
            var nodes = new Stack<int>();
            nodes.Push(index);
            var visited = new HashSet<int> { index };

            while (nodes.Count > 0)
            {
                var nodeId = nodes.Pop();
                var node = data[nodeId];
                if (node.Match != -1)
                {
                    hasFound = true;
                    break;
                }
                visited.Add(nodeId);
                if (node.Right != -1)
                {
                    if (visited.Add(node.Right))
                        nodes.Push(node.Right);

                    if ((node.Left != -1) && (node.Match == -1) && visited.Add(node.Left))
                        nodes.Push(node.Left);
                }
            }

            return hasFound;
        }

        /// <summary>
        /// Looks for nodes that are transitional only, replaces them with the other side of the
        /// link, and removes them reording all nodes
        /// Replaces:
        /// { node } -- (left/right) -->  {transition-node} ---right--->  { other-node }
        /// With:
        /// { node } ---> { other-node }
        /// </summary>
        public int FullReduce(Dictionary<int, int> nodeToAccepting, int start)
        {
            var singles = FindSingleEpsilons(nodeToAccepting);

            var map = new int[Count];
            var adj = 0;
            var dest = 0;
            for (var i = 0; i < Count; i++)
            {
                if (singles.ContainsKey(i))
                {
                    adj--;
                }
                else
                {
                    data[dest++] = data[i];
                    map[i] = i + adj;
                }
            }
            
            foreach (var pair in singles)
                map[pair.Key] = map[pair.Value];
            
            Count = dest;

            for (var i = 0; i < Count; i++)
            {
                ref var node = ref data[i];
                if (node.Left != -1)  node.Left = map[node.Left];
                if (node.Right != -1) node.Right = map[node.Right];
            }

            var oldMap = new Dictionary<int, int>(nodeToAccepting);
            nodeToAccepting.Clear();
            foreach (var pair in oldMap)
                nodeToAccepting[map[pair.Key]] = pair.Value;
            
            return map[start];
        }

        /// <summary>
        /// Looks for nodes that are transitional only, replaces them with other side of the link
        /// Replaces:
        /// { node } -- (left/right) -->  {transition-node} ---right--->  { other-node }
        /// With:
        /// { node } ---> { other-node }
        /// </summary>
        public int Reduce(Dictionary<int, int> nodeToAccepting, int start)
        {
            start = RemoveSingleEpsilons(nodeToAccepting, start);

            //Search for empty states
            var emptyStates = new HashSet<int>();
            for (var i = 0; i < Count; i++)
            {
                if (data[i].IsEmpty && !nodeToAccepting.ContainsKey(i))
                    emptyStates.Add(i);
            }

            while (emptyStates.Count > 0)
            {
                var newStates = new HashSet<int>();
                for (var i = 0; i < Count; i++)
                {
                    ref var node = ref data[i];
                    if (node.Match == -1 && emptyStates.Contains(node.Left))
                        node.Left = -1;
                    
                    if (emptyStates.Contains(node.Right))
                    {
                        if (node.RemoveRight())
                        {
                            newStates.Add(i);
                            continue;
                        }
                    }
                }
                emptyStates = newStates;
            }
            
            return start;
        }

        private int RemoveSingleEpsilons(Dictionary<int, int> nodeToAccepting, int start)
        {
            var singles = FindSingleEpsilons(nodeToAccepting);

            for (var i = 0; i < Count; i++)
            {
                ref var node = ref data[i];
                if ((node.Left != -1) && singles.TryGetValue(node.Left, out var newLeft))
                    node.Left = newLeft;
                if ((node.Right != -1) && singles.TryGetValue(node.Right, out var newRight))
                    node.Right = newRight;
            }

            if (singles.TryGetValue(start, out var newStart))
                start = newStart;
            return start;
        }

        private Dictionary<int, int> FindSingleEpsilons(Dictionary<int, int> nodeToAccepting)
        {
            var singles = data
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

            for (var index = 0; index < Count; index++)
            {
                if (!singles.ContainsKey(index))
                {
                    ref var node = ref data[index];
                    if ((node.Left != -1) && singles.TryGetValue(node.Left, out int remap))
                        node.Left = remap;
                    if ((node.Right != -1) && singles.TryGetValue(node.Right, out remap))
                        node.Right = remap;
                }
            }
            return singles;
        }

        /// <summary>
        /// Adds node to storage, resizing as necessary
        /// </summary>
        /// <param name="newNode"></param>
        /// <returns></returns>
        private int Add(int match, int left, int right)
        {
            if (Count == data.Length)
                Array.Resize(ref data, Count * 2);
            data[Count] = new NfaNode(match, left, right);
            return Count++;
        }

        public IEnumerator<NfaNode> GetEnumerator() => data.Take(Count).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
