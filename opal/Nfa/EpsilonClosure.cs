using System.Collections.Generic;

namespace Opal.Nfa
{
    public class EpsilonClosure: EpsilonClosureAlgorithm
    {
        public EpsilonClosure(Graph graph)
            : base(graph) 
        {
            Result = new HashSet<int>();
        }

        public HashSet<int> Result { get; }

        public HashSet<int> Find(IEnumerable<int> startSet)
        {
            Find(startSet, Result);
            return Result;
        }
    }
}
