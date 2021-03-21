using Opal.Containers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.Dfa
{
    public static class HopcraftAlgorithm
    {
        /// <summary>
        /// The Hopcroft - Karp algorithm finds equivalent states
        /// 
        /// </summary>
        ///Algorithm: 
        ///P := {{all accepting states}, {all nonaccepting states}};
        ///Q := {{all accepting states}};
        ///E := { all edges }
        ///while (Q is not empty) do
        ///     choose and remove a set A from Q
        ///     for each c in E do
        ///          let X be the set of states for which a transition on c leads to a state in A
        ///          for each set Y in P for which X ∩ Y is nonempty do
        ///               replace Y in P by the two sets X ∩ Y and Y \ X
        ///               if Y is in Q
        ///                    replace Y in Q by the same two sets
        ///               else
        ///                    add the smaller of the two sets to Q
        ///          end;
        ///     end;
        ///end;        
        public static LinkedList<HashSet<int>> FindEquivalentStates(List<DfaNode> states, 
            int edges)
        {
            var P = new LinkedList<HashSet<int>>();
            var Q = new LinkedList<HashSet<int>>();

            //Partition states by accepting states
            foreach (var g in states.GroupBy(x => x.AcceptingState))
            {
                var acceptingState = g.Key;
                var set = g.ToSet(x => x.Index);
                Q.AddLast(set);
                if (set.Count > 1)
                    P.AddLast(set.ToSet());
            }

            var X = new HashSet<int>();
            while (Q.Count > 0)
            {
                var A = Q.Last();
                Q.RemoveLast();
                for (var c = 1; c < edges; c++)
                {
                    //X is the set of states for which a transition on c leads to a state in A
                    X.CopyFrom(states.Where(x => A.Contains(x[c])).Select(x => x.Index));
                    if (X.Count == 0)
                        continue;
                    for (var Y = P.First; Y != null; Y = Y.Next)
                    {
                        var intersection = X.Intersect(Y.Value);
                        if ((intersection.Count == 0) || (intersection.Count == Y.Value.Count))
                            continue;

                        var disjointUnion = Y.Value.DisjointUnion(intersection);
                        var oldY = Y.Value;

                        //If we have sets of one, then we can simply remove them from P since there won't be any states
                        //   to remove
                        if (intersection.Count == 1)
                        {
                            if (disjointUnion.Count == 1)
                                P.Remove(Y);
                            else
                                Y.Value = disjointUnion;
                        }
                        else
                        {
                            Y.Value = intersection;
                            if (disjointUnion.Count > 1)
                                P.AddAfter(Y, disjointUnion);
                        }

                        LinkedListNode<HashSet<int>>? yNode;
                        for (yNode = Q.First; yNode != null; yNode = yNode.Next)
                        {
                            if (yNode.Value.SetEquals(oldY))
                                break;
                        }

                        if (yNode != null)
                        {
                            yNode.Value = intersection;
                            Q.AddAfter(yNode, disjointUnion);
                        }
                        else if (intersection.Count < disjointUnion.Count)
                        {
                            Q.AddLast(intersection);
                        }
                        else
                        {
                            Q.AddLast(disjointUnion);
                        }
                    }
                }
            }
            return P;
        }

        /// <summary>
        /// The Hopcroft - Karp algorithm finds equivalent states
        /// 
        /// </summary>
        ///Algorithm: 
        ///P := {{all accepting states}, {all nonaccepting states}};
        ///Q := {{all accepting states}};
        ///E := { all edges }
        ///while (Q is not empty) do
        ///     choose and remove a set A from Q
        ///     for each c in E do
        ///          let X be the set of states for which a transition on c leads to a state in A
        ///          for each set Y in P for which X ∩ Y is nonempty do
        ///               replace Y in P by the two sets X ∩ Y and Y \ X
        ///               if Y is in Q
        ///                    replace Y in Q by the same two sets
        ///               else
        ///                    add the smaller of the two sets to Q
        ///          end;
        ///     end;
        ///end;        
        public static LinkedList<HashSet<int>> Original(List<DfaNode> states,
            int edges)
        {
            var P = new LinkedList<HashSet<int>>();

            var acceptingSet = new HashSet<int>();
            var nonAcceptingSet = new HashSet<int>();
            foreach (var state in states)
            {
                if (state.IsAccepting)
                    acceptingSet.Add(state.Index);
                else
                    nonAcceptingSet.Add(state.Index);
            }
            P.AddLast(acceptingSet);
            P.AddLast(nonAcceptingSet);

            var Q = new WorkingQueue(acceptingSet.ToSet(), 
                nonAcceptingSet.ToSet());

            var X = new HashSet<int>();
            while (!Q.IsEmpty)
            {
                var A = Q.Remove();
                for (var c = 1; c < edges; c++)
                {
                    //X is the set of states for which a transition on c leads to a state in A
                    X.CopyFrom(states.Where(x => A.Contains(x[c])).Select(x => x.Index));
                    if (X.Count == 0)
                        continue;
                    for (var Y = P.First; Y != null; Y = Y.Next)
                    {
                        var intersection = X.Intersect(Y.Value);
                        if (intersection.Count == 0)
                            continue;
                        var disjointUnion = Y.Value.DisjointUnion(intersection);
                        if (disjointUnion.Count == 0)
                            continue;

                        var oldY = Y.Value;

                        if (disjointUnion.Count > 0)
                        {
                            Y.Value = intersection;
                            P.AddBefore(Y, disjointUnion);
                        }

                        if (!Q.TryReplace(oldY, intersection, disjointUnion))
                        {
                            Q.Add((intersection.Count < disjointUnion.Count) ?
                                intersection :
                                disjointUnion);
                        }
                    }
                }
            }
            
            for (var node = P.First; node != null; node = node.Next)
            {
                if (node.Value.Count <= 1)
                    P.Remove(node);
            }
            
            return P;
        }

        public class WorkingQueue
        {
            private readonly List<HashSet<int>> data;
            
            public WorkingQueue(HashSet<int> accepting, HashSet<int> nonAccepting)
            {
                data = new List<HashSet<int>>()
                {
                    nonAccepting,
                    accepting
                };
            }

            public bool TryReplace(HashSet<int> value, 
                HashSet<int> intersection,
                HashSet<int> disjointUnion)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    if (data[i].SetEquals(value))
                    {
                        data[i] = intersection;
                        Add(disjointUnion);
                        return true;
                    }
                }
                return false;
            }

            public void Add(HashSet<int> set) => 
                data.Add(set);

            public bool IsEmpty => data.Count == 0;

            public HashSet<int> Remove()
            {
                var lastIndex = data.Count - 1;
                var result = data[lastIndex];
                data.RemoveAt(lastIndex);
                return result;
            }

            public override string ToString()
            {
                var builder = new StringBuilder();
                var isFirst = true;
                foreach (var item in data)
                {
                    if (isFirst)
                        builder.Append(' ');
                    builder.Append('{')
                        .AppendJoin(", ", item)
                        .Append('}');
                }
                return builder.ToString();
            }
        }
    }
}
