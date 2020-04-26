using Opal.Nfa;
using System.Collections.Generic;
using System.Linq;

namespace Opal.Dfa
{
    public static class GraphExt
    {
        public static Dfa ToDfa(this Machine machine, Graph graph)
        {
            graph.Reduce();

            var signals = machine.Matches.NextId;
            var builder = new DfaBuilder(machine.Matches, machine.AcceptingStates);

            var nfaStartSet = new List<int> { graph.Start };
            var dfaStartSet = new HashSet<int>();
            graph.EpsilonClosure(nfaStartSet, dfaStartSet);

            var dfaState = builder.NewNode(dfaStartSet);
            var unmarkedStates = new List<DfaState> { dfaState };
            var moveResult = new List<int>();

            while (unmarkedStates.Count > 0)
            {
                // process an unprocessed state
                var processingDFAState = unmarkedStates[unmarkedStates.Count - 1];
                unmarkedStates.RemoveAt(unmarkedStates.Count - 1);

                var EpsilonClosureRes = new HashSet<int>();
                
                // for each input signal a
                for (var a = 1; a <= signals; a++)
                {
                    graph.Move(a, processingDFAState.NfaStates, moveResult);
                    if (moveResult.Count == 0)
                        continue;

                    graph.EpsilonClosure(moveResult, EpsilonClosureRes);

                    // Check if the resulting set (EpsilonClosureSet) in the
                    // set of DFA states (is any DFA state already constructed
                    // from this set of NFA states) or in pseudocode:
                    // is U in D-States already (U = EpsilonClosureSet)
                    var s = builder.States.FirstOrDefault(x => Compare(x.NfaStates, EpsilonClosureRes));

                    if (s == null)
                    {
                        var U = builder.NewNode(EpsilonClosureRes);
                        unmarkedStates.Add(U);

                        // Add transition from processingDFAState to new state on the current character
                        processingDFAState.AddTransition(a, U);
                    }
                    else
                    {
                        // This state already exists so add transition from 
                        // processingState to already processed state
                        processingDFAState.AddTransition(a, s);
                    }
                }
            }

            return builder.ToDfa();
        }

        public static bool Compare(ICollection<int> left, ICollection<int> right)
        {
            if (left.Count != right.Count)
                return false;
            foreach (var item in left)
            {
                if (!right.Contains(item))
                    return false;
            }
            return true;
        }
    }
}
