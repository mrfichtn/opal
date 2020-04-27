using Opal.Containers;
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

            var εClosureAlgo = new EpsilonClosure(graph);
            var εNodeIds = εClosureAlgo.Find(nfaStartSet);

            var dfaState = builder.NewNode(εNodeIds);
            var unmarkedStates = new List<DfaState> { dfaState };
            var move = new Move(graph);
            var moveResult = new List<int>();

            while (unmarkedStates.Count > 0)
            {
                // process an unprocessed state
                var processingDFAState = unmarkedStates[unmarkedStates.Count - 1];
                unmarkedStates.RemoveAt(unmarkedStates.Count - 1);

                // for each input signal a
                for (var a = 1; a <= signals; a++)
                {
                    if (move.Find(a, processingDFAState.NfaStates, moveResult) == 0)
                        continue;
                    
                    εClosureAlgo.Find(moveResult, εNodeIds);

                    // Check if the resulting set (EpsilonClosureSet) in the
                    // set of DFA states (is any DFA state already constructed
                    // from this set of NFA states) or in pseudocode:
                    // is U in D-States already (U = EpsilonClosureSet)
                    var s = builder.States
                        .FirstOrDefault(x => x.NfaStates.Compare(εNodeIds));
                    if (s == null)
                    {
                        s = builder.NewNode(εNodeIds);
                        unmarkedStates.Add(s);
                    }
                    processingDFAState.AddTransition(a, s);
                }
            }

            return builder.ToDfa();
        }


    }
}
