using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProductionContext
    {
        private readonly Logger logger;
        private readonly Productions.SymbolTable symbols;
        private readonly Productions.INoAction noAction;

        public ProductionContext(Logger logger,
            Productions.INoAction noAction)
        {
            this.logger = logger;
            this.noAction = noAction;
            symbols = new Productions.SymbolTable();
            TypeTable = new Productions.TypeTable();
        }

        public ProductionContext(Logger logger,
            Productions.INoAction noAction,
            Productions.SymbolTable symbols,
            Productions.TypeTable typeTable)
        {
            this.logger = logger;
            this.noAction = noAction;
            this.symbols = symbols;
            TypeTable = typeTable;
        }



        public Productions.TypeTable TypeTable { get; }

        public List<Productions.Symbol> Symbols => symbols.Symbols;

        public ProductionContext AddTerminals(IEnumerable<Nfa.Symbol> symbols)
        {
            var tokenType = new Productions.NullableType("Token");
            foreach (var symbol in symbols)
            {
                this.symbols.Add(symbol);
                TypeTable.TypeFromAttr(symbol.Name, tokenType);
            }
            return this;
        }

        public ProductionContext AddNonTerminals(ProductionList prods)
        {
            foreach (var production in prods)
                symbols.AddNonTerminal(production.Name.Value);
            return this;
        }

        public void AddDeclarations(ProductionList prods)
        {
            var context = new ImproptuDeclContext(symbols);
            foreach (var expr in prods.Expressions)
                expr.AddImproptuDeclaration(context);
        }

        public ProductionContext AddActionTypes(ProductionList prods)
        {
            var typeContext = new ProductionActionTypeContext(TypeTable,
                noAction);
            foreach (var production in prods)
                production.AddActionType(typeContext);
            typeContext.Resolve();
            return this;
        }

        public bool TryFind(string value, out int id, out bool isTerminal) =>
            symbols.TryGetValue(value, out id, out isTerminal);

        public static string CreateName(string text)
        {
            string name;
            if ((text.Length > 0) && char.IsLetter(text[0]))
                name = "@" + text;
            else
                name = text;
            return name;
        }

        public Productions.TerminalBase MissingSymbol(Identifier name)
        {
            logger.LogError($"Missing symbol '{name}'",
                name);
            return new Productions.MissingSymbolTerminal(name);
        }
    }
}
