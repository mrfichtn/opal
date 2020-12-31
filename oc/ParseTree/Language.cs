using Opal.Nfa;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Opal.ParseTree
{
    public class Language
    {
        private readonly string? srcFile;
        private readonly TokenList tokens;
        private readonly OptionList? options;
        
        public Language(string? srcFile,
            UsingList usings,
            Identifier? @namespace, 
            OptionList? options,
            CharacterList characters,
            TokenList tokens, 
            ProductionSection productions,
            ConflictList? conflicts)
        {
            this.srcFile = srcFile;
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

        public void MergeTo(Options options)
        {
            if (this.options != null)
                this.options.MergeTo(options);
        }

        public bool BuildNfa(Logger logger, 
            INfaWriter writer,
            [NotNullWhen(true)] out Graph? graph)
        {
            var charMap = Characters.Build(logger);
            graph = tokens.Build(logger, charMap);
            if (graph == null)
                return false;
            
            var context = new DeclareTokenContext(logger, graph);
            Productions.AddStringTokens(context);

            writer.Write(graph, srcFile);
            return true;
        }

        public Productions.Grammar? BuildGrammar(Logger logger, 
            IEnumerable<Symbol> symbols)
        {
            return Productions.Build(logger, symbols);
        }
    }
}
