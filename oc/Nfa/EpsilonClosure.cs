using System.Collections.Generic;

namespace Opal.Nfa
{
    /// <summary>
    /// An Epsilon closure algorithm that saves the found set in the Result member
    /// </summary>
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
