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
            IEnumerable<Symbol> symbols)
        {
            var context = new ProductionContext(logger);
            context.AddTerminals(symbols);
            context.AddDeclarations(productions);

            var list = new List<Productions.Production>();
            var ruleId = 0;

            foreach (var prod in productions)
            {
                var attr = prod.BuildAttribute();

                context.TryFind(prod.Name.Value, out var id, out var isTerminal);
                foreach (var definition in prod.Definitions)
                {
                    var terms = definition.Build(context);
                    var production = new Productions.Production(
                        prod.Name,
                        id,
                        ruleId++,
                        attr,
                        definition.Action,
                        terms);
                    list.Add(production);
                }
            }
            
            context.TypeTable.Write("types.txt");

            return new Productions.Grammar(Start.Value,
                context.Symbols,
                list.ToArray(),
                context.TypeTable);
        }
    }
}
