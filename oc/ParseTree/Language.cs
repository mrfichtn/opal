using Opal.Nfa;

namespace Opal.ParseTree
{
    public class Language
    {
        public Language(Identifier? @namespace, 
            Graph? graph, 
            ProductionList? productions,
            ConflictList? conflicts)
        {
            Namespace = @namespace ?? new Identifier("Opal");
            Graph = graph ?? new Graph();
            Productions = productions ?? new ProductionList();
            Conflicts = conflicts ?? new ConflictList();
        }

        public Identifier Namespace { get; set; }
        public Graph Graph { get; }
        public ProductionList Productions { get; }
        public ConflictList Conflicts { get; }
    }
}
