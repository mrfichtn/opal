using Opal.Nfa;
using System.IO;

namespace Opal.ParseTree
{
    public class Language
    {
        private readonly TokenList tokens;
        private readonly OptionList? options;
        
        public Language(UsingList usings,
            Identifier? @namespace, 
            OptionList? options,
            CharacterList characters,
            TokenList tokens, 
            ProductionSection productions,
            ConflictList? conflicts)
        {
            Usings = usings;
            Namespace = @namespace ?? new Identifier("Opal");
            this.options = options;
            Characters = characters;
            this.tokens = tokens;
            Productions = productions;
            Conflicts = conflicts ?? new ConflictList();
        }

        public Identifier Namespace { get; set; }

        public UsingList Usings { get; }

        public CharacterList Characters { get; }

        public ProductionSection Productions { get; }
        public ConflictList Conflicts { get; }

        public void MergeOptions(Options options)
        {
            if (this.options != null)
                this.options.MergeTo(options);
        }

        public Graph? BuildGraph(Logger logger, Options options, string inPath)
        {
            var charMap = Characters.Build(logger);
            var graph = tokens.Build(logger, charMap);
            if (graph == null)
                return graph;
            
            var context = new DeclareTokenContext(logger, graph);
            Productions.AddStringTokens(context);

            if (options.TryGet("nfa", out var nfaPath))
            {
                if (string.IsNullOrEmpty(nfaPath))
                    nfaPath = Path.ChangeExtension(inPath, ".nfa.txt");
                File.WriteAllText(nfaPath, graph.ToString());
            }
            return graph;
        }

        public Productions.Grammar? BuildGrammar(Logger logger, Graph graph)
        {
            return Productions.Build(logger, graph);
        }
    }
}
