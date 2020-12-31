using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProductionContext
    {
        private readonly Logger logger;
        private readonly Productions.SymbolTable symbols;

        public ProductionContext(Logger logger)
        {
            this.logger = logger;
            symbols = new Productions.SymbolTable();
            TypeTable = new Productions.TypeTable();
        }

        public Productions.TypeTable TypeTable { get; }

        public List<Productions.Symbol> Symbols => symbols.Symbols;

        public void AddTerminals(IEnumerable<Nfa.Symbol> symbols)
        {
            foreach (var symbol in symbols)
                this.symbols.Add(symbol);
        }

        public void AddDeclarations(ProductionList prods)
        {
            foreach (var production in prods)
            {
                symbols.Add(production);
                production.AddActionType(TypeTable);
            }

            var context = new ImproptuDeclContext(symbols);
            foreach (var expr in prods.Expressions)
                expr.AddImproptuDeclaration(context);
        }

        public bool TryFind(string value, out int id, out bool isTerminal)
        {
            var result = symbols.TryGetValue(value, out id, out isTerminal);
            return result;
        }

        public static string CreateName(string text)
        {
            string name;
            if ((text.Length > 0) && char.IsLetter(text[0]))
                name = "@" + text;
            else
                name = text;
            return name;
        }

        //public (string name, int id) AddKeyword(StringConst str)
        //{
        //    string name;
        //    int state;
        //    try
        //    {
        //        if (str == null)
        //            throw new ArgumentNullException(nameof(str));
        //        var text = str.Value;

        //        state = graph.FindState(text);
        //        if (state == -1)
        //        {
        //            name = CreateName(str.Value);
        //            var g = graph.Create(text);
        //            state = g.MarkEnd(name, text);
        //            graph.Union(g);
        //        }
        //        else
        //        {
        //            graph.Machine.AcceptingStates.TryGetName(state, out name);
        //        }
        //        return (name, state);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Uncaught exception: {ex.Message}",
        //            str);
        //        throw;
        //    }
        //}

        //public (string name, int id) AddKeyword(CharConst str)
        //{
        //    string name;
        //    int state;
        //    try
        //    {
        //        if (str == null)
        //            throw new ArgumentNullException(nameof(str));

        //        var text = new string(str.Value, 1);
        //        state = graph.FindState(text);
        //        if (state == -1)
        //        {
        //            name = CreateName(text);
        //            var g = graph.Create(text);
        //            state = g.MarkEnd(name, text);
        //            graph.Union(g);
        //        }
        //        else
        //        {
        //            graph.Machine.AcceptingStates.TryGetName(state, out name);
        //        }
        //        return (name, state);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Uncaught exception: {ex.Message}",
        //            str);
        //        throw;
        //    }
        //}

        public Productions.TerminalBase? MissingSymbol(Identifier name)
        {
            logger.LogError($"Missing symbol '{name}'",
                name);
            return null;
        }
    }
}
