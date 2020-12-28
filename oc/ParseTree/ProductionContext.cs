using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.ParseTree
{
    public class ProductionContext
    {
        private readonly Logger logger;
        private readonly Dictionary<string, int> symbolMap;

        public ProductionContext(Logger logger)
        {
            this.logger = logger;
            symbolMap = new Dictionary<string, int>();
            Symbols = new List<Symbol>();
            TypeTable = new Productions.TypeTable();
        }

        public List<Symbol> Symbols { get; }

        public Productions.TypeTable TypeTable { get; }

        public void AddTerminals(Nfa.Graph graph)
        {
            foreach (var symbol in graph.Machine.AcceptingStates.Symbols)
            {
                symbolMap.Add(symbol.Name, symbol.Index);
                Symbols.Add(new Symbol(symbol.Name, true, symbol.Text));
            }
        }

        public void AddDeclarations(ProductionList prods)
        {
            foreach (var production in prods)
            {
                if (!symbolMap.ContainsKey(production.Name.Value))
                {
                    symbolMap.Add(production.Name.Value, symbolMap.Count);
                    Symbols.Add(new Symbol(production.Name.Value, false));
                }
                production.AddActionType(TypeTable);
            }

            var context = new ImproptuDeclContext(symbolMap, Symbols);
            foreach (var expr in prods.Expressions)
                expr.AddImproptuDeclaration(context);
        }

        public bool TryFind(string value, out int id, out bool isTerminal)
        {
            var result = symbolMap.TryGetValue(value, out id);
            isTerminal = id >= 0;
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

        public Productions.Symbol? MissingSymbol(Identifier name)
        {
            logger.LogError($"Missing symbol '{name}'",
                name);
            return null;
        }
    }
}
