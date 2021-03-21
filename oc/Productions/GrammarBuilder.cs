using System.Collections.Generic;
using Opal.Containers;

namespace Opal.Productions
{
    public class GrammarBuilder
    {
        private readonly Logger logger;
        private readonly SymbolTable symbols;
        private readonly List<Production> productions;
        private readonly ParseTree.Identifier start;

        private INoAction noAction;

        public GrammarBuilder(Logger logger,
            ParseTree.Identifier start)
        {
            this.logger = logger;
            this.start = start;
            noAction = new FirstNoAction();
            symbols = new SymbolTable();
            TypeTable = new TypeTable();
            productions = new List<Production>();
        }

        public TypeTable TypeTable { get; }

        public GrammarBuilder Options(Options options)
        {
            options.TryGet("no_action", out var value);
            if (value.EqualsI("null"))
                noAction = new NullNoAction();
            else if (value.EqualsI("tuple"))
                noAction = new TupleNoAction();
            return this;
        }

        public GrammarBuilder Terminals(IEnumerable<Nfa.Symbol> symbols)
        {
            var tokenType = new NullableType("Token");
            foreach (var symbol in symbols)
            {
                this.symbols.Add(symbol);
                TypeTable.TypeFromAttr(symbol.Name, tokenType);
            }
            return this;
        }
        
        public GrammarBuilder ProductionList(ParseTree.ProductionList prods) =>
            NonTerminals(prods)
                .ActionTypes(prods)
                .Declarations(prods)
                .Productions(prods);
        
        public Grammar Build() =>
            new Grammar(start.Value,
                symbols.Symbols,
                productions.ToArray(),
                TypeTable);

        /// <summary>
        /// Adds production name to symbol table as non-terminal
        /// </summary>
        private GrammarBuilder NonTerminals(ParseTree.ProductionList prods)
        {
            foreach (var production in prods)
                symbols.AddNonTerminal(production.Name.Value);
            return this;
        }

        private GrammarBuilder Declarations(ParseTree.ProductionList prods)
        {
            var context = new ParseTree.ImproptuDeclContext(symbols,
                TypeTable);
            foreach (var expr in prods.Expressions)
                expr.AddImproptuDeclaration(context);
            ActionTypes(context.Productions);
            context.CopyTo(prods);
            return this;
        }

        private GrammarBuilder ActionTypes(ParseTree.ProductionList prods)
        {
            var typeContext = new ParseTree.ProductionActionTypeContext(TypeTable,
                noAction);
            foreach (var production in prods)
                production.AddActionType(typeContext);
            typeContext.Resolve();
            return this;
        }

        private GrammarBuilder Productions(ParseTree.ProductionList prodList)
        {
            var ruleId = 0;

            TypeTable.Write("types.txt");

            foreach (var prod in prodList)
            {
                symbols.TryGetValue(prod.Name.Value, out var id, out var isTerminal);
                
                foreach (var definition in prod.Definitions)
                {
                    var terms = definition.Build(this);
                    var reduceContext = new ReduceContext(
                        TypeTable,
                        terms,
                        definition.Action,
                        noAction,
                        id);
                    var reduction = terms.Reduction(reduceContext);
                    var production = new Production(
                        prod.Name,
                        id,
                        ruleId++,
                        terms,
                        reduction);
                    productions.Add(production);
                }
            }
            return this;
        }

        public bool TryFind(string value, out int id, out bool isTerminal) =>
            symbols.TryGetValue(value, out id, out isTerminal);

        public TerminalBase MissingSymbol(ParseTree.Identifier name)
        {
            logger.LogError($"Missing symbol '{name}'",
                name);
            return new MissingSymbolTerminal(name);
        }
    }
}
