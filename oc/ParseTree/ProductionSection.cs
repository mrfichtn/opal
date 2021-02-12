using Opal.Nfa;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProductionSection
    {
        private readonly ProductionList productions;
        private readonly Identifier start;

        public ProductionSection(Token start, 
            ProductionList productions)
        {
            this.start = new Identifier(start);
            this.productions = productions;
        }


        public void AddStringTokens(DeclareTokenContext context) =>
            productions.AddStringTokens(context);

        public Productions.Grammar Build(Logger logger, 
            IEnumerable<Symbol> symbols,
            Options options) =>
            new Productions.GrammarBuilder(logger, start)
                .Options(options)
                .Terminals(symbols)
                .ProductionList(productions)
                .Build();
    }
}
