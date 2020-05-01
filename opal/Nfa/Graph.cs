using Opal.Containers;
using Opal.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.Nfa
{
    public sealed class Graph
    {
        private int end;

        /// <summary>
        /// Creates graph from provided start and end nodes
        /// </summary>
        /// <param name="machine">Nfa nodes and accepting states</param>
        /// <param name="start">Starting node</param>
        /// <param name="end">Ending node</param>
        private Graph(Machine machine, int start, int end)
        {
            Machine = machine;
            Start = start;
            this.end = end;
        }

        /// <summary>
        /// Creates an empty, starting node
        /// </summary>
        /// <param name="machine"></param>
        public Graph(): this(new Machine())
        {
        }

        public Graph(Machine machine)
        {
            Machine = machine;
            Start = end = machine.Nodes.CreateEnd();
        }

        /// <summary>
        /// Creates NFA graph from a string
        /// </summary>
        public Graph(Machine machine, string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            Machine = machine;
            var nodes = machine.Nodes;
            var node = nodes.CreateEnd();
            end = node;
            for (var i = text.Length - 1; i >= 0; i--)
                node = nodes.CreateMatch(machine.GetClassId(text[i]), node);
            Start = node;
        }

        public Graph(string text)
            : this(new Machine(), text)
        {
        }

        /// <summary>
        /// Creates graph from a single match
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="match"></param>
        public Graph(Machine machine, IMatch match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            this.Machine = machine;
            end = machine.Nodes.CreateEnd();
            Start = machine.SetMatch(end, match);
        }

        #region Properties

        /// <summary>
        /// Returns starting node
        /// </summary>
        /// <returns></returns>
        public int Start { get; internal set; }

        public Machine Machine { get; }

        #endregion

        public Graph Create() => new Graph(Machine);

        public Graph Create(string value) => new Graph(Machine, value);

        public Graph Create(StringConst text) => Create(text.Value);

        public Graph Create(IMatch match) => new Graph(Machine, match);

        public int MarkEnd(string tokenName, Identifier attr = null)
        {
            var ignore = (attr?.Value == "ignore");

            if (!Machine.AcceptingStates.TryAdd(tokenName, ignore, end, out var index))
                throw new Exception(string.Format("Duplicate symbol {0}", tokenName));
            return index;
        }

        public static Graph MarkEnd(Token id, Token attr, Graph g)
        {
            var ignore = (attr?.Value == "ignore");
            if (!g.Machine.AcceptingStates.TryAdd(id.Value, ignore, g.end, out _))
                throw new Exception(string.Format("Duplicate symbol {0}", id.Value));
            return g;
        }

        #region Thompson construction

        /// <summary>
        /// Creates graph for addition of match
        /// Match   abc d
        /// New graph:                              match 
        ///                 existing graph  { end } ----->  { graph g }
        ///                                         right
        /// </summary>
        /// <param name="g"></param>
        public Graph Concatenate(Graph g)
        {
            Node(end).Right = g.Start;
            end = g.end;
            return this;
        }

        public static Graph Concatenate(Graph g, Graph g2)
            => g.Concatenate(g2);

        
        /// <summary>
        /// Adds option to this graph
        /// Match   abc d
        /// New graph:                              
        ///      {new-node} --(left) --> { existing } --(right)--> {new-end} 
        ///                 --(right)--> { g } ---------(right)--------^
        /// </summary>
        /// <param name="g"></param>
        public Graph Union(Graph g)
        {
            var nodes = Machine.Nodes;
            Start = nodes.Create(Start, g.Start);
            var newEnd = nodes.CreateEnd();

            Node(end).Right = newEnd;
            Node(g.end).Right = newEnd;
            end = newEnd;
            return this;
        }

        public static Graph Union(Graph g1, Graph g2)
        {
            g1.Union(g2);
            return g1;
        }


        /// <summary>
        /// Adds star closure (zero or more instances of graph)
        /// { new node } --> {existing graph} -> {end}
        ///              --------------------------^
        /// </summary>
        public void StarClosure()
        {
            var newEnd = Machine.Nodes.CreateEnd();
            Start = Machine.Nodes.Create(Start, newEnd);
            Machine.Nodes.Set(end, -1, Start, newEnd);
            end = newEnd;
        }

        public static Graph StarClosure(Graph g)
        {
            g.StarClosure();
            return g;
        }

        /// <summary>
        /// Adds plus closure (1 or more instance)
        /// { graph } --> { new node } -> end
        ///     ^-----------------------
        /// </summary>
        public void PlusClosure()
        {
            var end = Machine.Nodes.CreateEnd();
            Machine.Nodes.Set(this.end, -1, Start, end);
            this.end = end;
        }

        public static Graph PlusClosure(Graph g)
        {
            g.PlusClosure();
            return g;
        }

        /// <summary>
        /// Adds question closure (0 / 1 instances)
        /// </summary>
        public void QuestionClosure()
        {
            var nodes = Machine.Nodes;
            var end = nodes.CreateEnd();
            Start = nodes.Create(Start, end);
            nodes.SetRight(this.end, end);
            this.end = end;
        }

        public static Graph QuestionClosure(Graph g)
        {
            g.QuestionClosure();
            return g;
        }

        /// <summary>
        /// Adds x-number of instances
        /// </summary>
        /// <param name="n"></param>
        public Graph Quantifier(int n)
        {
            if (n == 1) return this;
            var graphs = new Graph[n - 1];
            for (var i = 1; i < n; i++)
                graphs[i - 1] = Dup();
            foreach (var g in graphs)
                Concatenate(g);
            return this;
        }

        public static Graph Quantifier(Graph g, Integer n) =>
            g.Quantifier(n.Value);

        /// <summary>
        /// Adds min to max instances
        /// </summary>
        /// <param name="min">Minimum number of occurrences</param>
        /// <param name="max">Maximum number of occurrences</param>
        public Graph RangeQuantifier(int min, int max)
        {
            if (min > max)
            {
                var swap = min;
                min = max;
                max = swap;
            }

            max--;
            var graphs = new Graph[max];
            int i;
            for (i = 0; i < max; i++)
                graphs[i] = Dup();

            i = 0;
            if (min == 0)
            {
                StarClosure();
            }
            else if (min > 1)
            {
                for (; i < min; i++)
                    Concatenate(graphs[i]);
            }

            var nodes = Machine.Nodes;
            for (; i < max; i++)
            {
                nodes.Set(end, -1, graphs[i].Start, graphs[i].end);
                end = graphs[i].end;
            }
            return this;
        }

        public static Graph RangeQuantifier(Graph g, Integer min, Integer max)
            => g.RangeQuantifier(min.Value, max.Value);

        public int FindState(string keyword)
        {
            var search = new KeywordSearch(this);
            return search.Search(keyword);
        }

        public Graph Dup()
        {
            var nodes = Machine.Nodes;
            var stack = new Stack<int>();
            stack.Push(Start);
            var start = nodes.New();
            var map = new Dictionary<int, int>() { { Start, start } };

            while (stack.Count > 0)
            {
                var nextId = stack.Pop();
                var next = nodes[nextId];
                var newNodeId = map[nextId];

                int left;
                if (next.Left == -1)
                {
                    left = -1;
                }
                else if (!map.TryGetValue(next.Left, out left))
                {
                    left = nodes.New();
                    map.Add(next.Left, left);
                    stack.Push(next.Left);
                }

                int right;
                if (next.Right == -1)
                {
                    right = -1;
                }
                else if (!map.TryGetValue(next.Right, out right))
                {
                    right = nodes.New();
                    map.Add(next.Right, right);
                    stack.Push(next.Right);
                }
                nodes.Set(newNodeId, next.Match, left, right);
            }

            return new Graph(Machine, start, map[end]);
        }

        #endregion

        public override string ToString()
        {
            var result = new StringBuilder();
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            if (Start != -1)
            {
                queue.Enqueue(Start);
                visited.Add(Start);
            }
            var nodes = Machine.Nodes;
            while (queue.Count > 0)
            {
                var nodeIndex = queue.Dequeue();
                var node = nodes[nodeIndex];
                result.AppendLine(node.ToString(Machine, nodeIndex));
                if (node.Right != -1 && visited.Add(node.Right))
                    queue.Enqueue(node.Right);
                if (node.Left != -1 && visited.Add(node.Left))
                    queue.Enqueue(node.Left);
            }
            return result.ToString();
        }

        private ref NfaNode Node(int index) =>
            ref Machine.Nodes[index];
    }
}
