using Opal.Containers;
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
            var context = new ProductionContext(logger);
            context.AddTerminals(symbols);
            context.AddDeclarations(productions);

            var list = new List<Productions.Production>();
            var ruleId = 0;

            context.TypeTable.Write("types.txt");

            foreach (var prod in productions)
            {
                var attr = prod.BuildAttribute();

                context.TryFind(prod.Name.Value, out var id, out var isTerminal);
                foreach (var definition in prod.Definitions)
                {
                    var terms = definition.Build(context);
                    
                    options.TryGet("no_action", out var noAction);
                    var reduceContext = new Productions.ReduceContext(
                        context.TypeTable,
                        terms,
                        definition.Action,
                        attr,
                        GetOption(noAction),
                        id);
                    var reduction = reduceContext.Reduce();

                    var production = new Productions.Production(
                        prod.Name,
                        id,
                        ruleId++,
                        terms,
                        reduction);
                    list.Add(production);
                }
            }
            
            context.TypeTable.Write("types2.txt");

            return new Productions.Grammar(Start.Value,
                context.Symbols,
                list.ToArray(),
                context.TypeTable);
        }

        private static Productions.INoAction GetOption(string? noAction)
        {
            if (noAction.EqualsI("null"))
                return new Productions.NullNoAction();
            if (noAction.EqualsI("tuple"))
                return new Productions.TupleNoAction();
            return new Productions.FirstNoAction();
        }

    }
}
