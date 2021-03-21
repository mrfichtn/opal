using Opal.Containers;
using Opal.Logging;
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

		public Dfa ToDfa(ILogger logger)
		{
			//ReduceEdges();
			var dfaList = States
                .Select(x => x.ToNode())
                .ToList();
			RemoveUnreachableStates(logger, dfaList);
			//Reduce states, by combining states with equal transitions
            HopcroftAlgorithm(logger, dfaList);
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
		private static void RemoveUnreachableStates(ILogger logger,
            List<DfaNode> states)
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

            var removed = 0;
            foreach (var index in unreachable.OrderByDescending(x => x))
            {
                Remove(states, index);
                removed++;
            }
            if (removed > 0)
            {
                logger.LogMessage(Importance.Low, "  Removed {0} duplicate DFA states", 
                    removed);
            }
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
        private void HopcroftAlgorithm(ILogger logger, List<DfaNode> states)
        {
            //var P = HopcraftAlgorithm.FindEquivalentStates(states, edges);
            var P = HopcraftAlgorithm.FindEquivalentStates(states, edges);

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

            var removed = 0;
            foreach (var pair in toRemove.OrderByDescending(x => x.Key))
            {
                Remove(states, pair.Value, pair.Key);
                removed++;
            }
            if (removed > 0)
                logger.LogMessage(Importance.Low, 
                    "  Removed {0} duplicate DFA states",
                    removed);
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
