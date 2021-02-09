using Opal.Nfa;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProductionSection
    {
        private readonly ProductionList productions;

        public ProductionSection(Token start, 
            ProductionList productions)
        {
            Start = new Identifier(start);
            this.productions = productions;
        }

        public Identifier Start { get; }

        public void AddStringTokens(DeclareTokenContext context) =>
            productions.AddStringTokens(context);

        public Productions.Grammar Build(Logger logger, 
            IEnumerable<Symbol> symbols,
            Options options)
        {
            return new Productions.GrammarBuilder(logger, Start)
                .Options(options)
                .Terminals(symbols)
                .ProductionList(productions)
                .Build();
        }
    }
}
