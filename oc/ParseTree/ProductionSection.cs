using Opal.Nfa;
using System;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProductionSection
    {
        public ProductionSection(Token start, 
            ProductionList productions)
        {
            Start = new Identifier(start);
            Productions = productions;
        }

        public Identifier Start { get; }
        public ProductionList Productions { get; }

        public void AddStringTokens(DeclareTokenContext context)
        {
            Productions.AddStringTokens(context);
        }

        public Productions.Grammar Build(Logger logger, Graph graph)
        {
            var context = new ProductionContext(logger);
            context.AddTerminals(graph);
            context.AddDeclarations(Productions);

            var list = new List<Productions.Production>();
            var ruleId = 0;

            foreach (var prod in Productions)
            {
                context.TryFind(prod.Name.Value, out var id, out var isTerminal);
                foreach (var definition in prod.Definitions)
                {
                    var symbols = definition.Build(context);
                    var production = new Productions.Production(
                        prod.Name,
                        id,
                        ruleId++,
                        prod.Attribute,
                        definition.Action,
                        symbols);
                    list.Add(production);
                }
            }
            
            foreach (var prod in list)
            {
                var type = prod.Type;
                if (!string.IsNullOrEmpty(type))
                    context.TypeTable.AddPrimary(prod.Name, type);
            }

            return new Productions.Grammar(Start.Value,
                context.Symbols,
                list.ToArray(),
                context.TypeTable);
        }
    }
}
