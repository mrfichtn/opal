using Opal.Containers;
using Opal.Nfa;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Dfa
{
    public class DfaBuilder
	{
		private readonly Matches matches;
		private readonly AcceptingStates acceptingStates;
		private readonly int edges;

		public DfaBuilder(Matches matches, AcceptingStates acceptingStates)
		{
			this.matches = matches;
			edges = matches.NextId + 1;
			this.acceptingStates = acceptingStates;
			States = new List<DfaState>();
		}

        internal List<DfaState> States { get; }

        public DfaState NewNode(IEnumerable<int> nodes)
		{
			var state = new DfaState(acceptingStates, nodes, States.Count, edges);
			States.Add(state);
			return state;
		}

		public Dfa ToDfa()
		{
			//ReduceEdges();
			var dfaList = States.Select(x => x.ToNode()).ToList();
			RemoveUnreachableStates(dfaList);
			HopcroftAlgorithm(dfaList);

			return new Dfa(matches, acceptingStates, dfaList);
		}

		
        /// Unreachable states
		/// The state p of DFA M=(Q, Σ, δ, q0, F) is unreachable if no such string 
        /// w in ∑* exists for which p=δ(q0, w). Reachable states can be obtained 
        /// with the following algorithm:
		/// 
		///let reachable_states:= {q0};
		///let new_states:= {q0};
		///do {
		///   temp := the empty set;
		///   for each q in new_states do
		///      for all c in ∑ do
		///         temp := temp ∪ {p such that p=δ(q,c)};
		///      end;
		///   end;
		///   new_states := temp \ reachable_states;
		///   reachable_states := reachable_states ∪ new_states;
		///} while(new_states ≠ the empty set);
		///unreachable_states := Q \ reachable_states;
		private static void RemoveUnreachableStates(List<DfaNode> states)
		{
            var unreachable = states.ToSet(x => x.Index);
            unreachable.Remove(0);
            var queue = new Queue<int>();
            queue.Enqueue(0);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                foreach (var next in states[item].Next)
                {
                    if (unreachable.Remove(next))
                        queue.Enqueue(next);
                }
            }

            foreach (var index in unreachable.OrderByDescending(x=>x))
                Remove(states, index);
		}

        /// <summary>
        /// The Hopcroft - Karp algorithm reduces the number of states
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
        private void HopcroftAlgorithm(List<DfaNode> states)
        {
            var groupByAccepting = states.GroupBy(x => x.AcceptingState);

            var P = new LinkedList<HashSet<int>>();
            var Q = new LinkedList<HashSet<int>>();

            foreach (var g in groupByAccepting)
            {
                var acceptingState = g.Key;
                var dfaNodes = g.ToSet(x => x.Index);
                if (acceptingState != 0)
                    Q.AddLast(dfaNodes);
                if (dfaNodes.Count > 1)
                    P.AddLast(dfaNodes);
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

                        var disjointUnion = Y.Value.Difference(intersection);
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
                            if (yNode.Value == oldY)
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

            var toRemove = new Dictionary<int, int>();
            foreach (var X2 in P)
            {
                using var e = X2.GetEnumerator();
                if (e.MoveNext())
                {
                    var first = e.Current;
                    while (e.MoveNext())
                        toRemove.Add(e.Current, first);
                }
            }

            foreach (var pair in toRemove.OrderByDescending(x => x.Key))
                Remove(states, pair.Value, pair.Key);
        }

        private static void Remove(List<DfaNode> states, int newIndex, int oldIndex)
		{
			if (newIndex > oldIndex)
				newIndex--;
			int k;

			for (k = 0; k < oldIndex; k++)
				states[k].RemoveState(oldIndex, newIndex);
			for (k++; k < states.Count; k++)
				states[k].RemoveState(oldIndex, newIndex);
			states.RemoveAt(oldIndex);
		}

        /// <summary>
        /// Renumbers and removes oldIndex from each state and then removes state
        /// </summary>
        private static void Remove(List<DfaNode> states, int stateIndex)
        {
            int k;
            for (k = 0; k < stateIndex; k++)
                states[k].RemoveState(stateIndex);
            for (k++; k < states.Count; k++)
                states[k].RemoveState(stateIndex);
            states.RemoveAt(stateIndex);
        }
    }
}
