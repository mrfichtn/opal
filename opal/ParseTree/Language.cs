using Opal.Nfa;

namespace Opal.ParseTree
{
    public class Language
    {
        public Language(Identifier @namespace, 
            Graph graph, 
            ProductionList productions,
            ConflictList conflicts)
        {
            Namespace = @namespace;
            Graph = graph;
            Productions = productions;
            Conflicts = conflicts;
        }

        public Identifier Namespace { get; set; }
        public Graph Graph { get; }
        public ProductionList Productions { get; }
        public ConflictList Conflicts { get; }
    }
}
