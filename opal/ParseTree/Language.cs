using Opal.Nfa;

namespace Opal.ParseTree
{
    public class Language
    {
        public Language(Identifier @namespace, Graph graph, ProductionList productions)
        {
            Namespace = @namespace;
            Graph = graph;
            Productions = productions;
        }

        public Identifier Namespace { get; set; }
        public Graph Graph { get; }
        public ProductionList Productions { get; }
    }
}
