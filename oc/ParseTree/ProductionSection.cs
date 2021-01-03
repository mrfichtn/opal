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
                context.TryFind(prod.Name.Value, out var id, out var isTerminal);
                foreach (var definition in prod.Definitions)
                {
                    var terms = definition.Build(context);
                    var production = new Productions.Production(
                        prod.Name,
                        id,
                        ruleId++,
                        prod.Attribute,
                        definition.Action,
                        terms);
                    list.Add(production);
                }
            }
            
            foreach (var prod in list)
            {
                var type = prod.Type;
                if (!string.IsNullOrEmpty(type))
                    context.TypeTable.AddPrimary(prod.Name, type);
            }

            context.TypeTable.Write("types.txt");

            return new Productions.Grammar(Start.Value,
                context.Symbols,
                list.ToArray(),
                context.TypeTable);
        }
    }
}
