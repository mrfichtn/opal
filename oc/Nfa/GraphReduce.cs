namespace Opal.Nfa
{
    public static class GraphReduce
    {
        public static void Reduce(this Graph graph)
        {
            graph.RemoveSingleEpsilons();
        }
    }
}
