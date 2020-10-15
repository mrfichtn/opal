using Opal.Containers;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Opal.LR1
{
    public class Symbols: IEnumerable<Symbol>
    {
        public static Symbol EndOfLine;
        private readonly List<Symbol> symbols;
        private readonly Dictionary<string, Symbol> byName;

        static Symbols()
        {
            EndOfLine = new Symbol(string.Empty, 0, true);
        }

        public Symbols()
        {
            byName = new Dictionary<string, Symbol>
            {   { EndOfLine.Value, EndOfLine } };
            symbols = new List<Symbol>
            { EndOfLine };
        }

        public Symbol this[string name] => byName[name];
        public Symbol this[int id] => symbols[id];

        public int Count => symbols.Count;

        public bool TryFind(string name, out Symbol symbol) =>
            byName.TryGetValue(name, out symbol);

        public Symbol Create(string symbolValue, bool isTerminal)
        {
            if (!byName.TryGetValue(symbolValue, out Symbol symbol))
            {
                symbol = new Symbol(symbolValue, (uint)symbols.Count, isTerminal);
                byName.Add(symbolValue, symbol);
                symbols.Add(symbol);
            }
            else if (!isTerminal && symbol.IsTerminal)
            {
                symbol.IsTerminal = isTerminal;
            }
            return symbol;
        }

        public IEnumerator<Symbol> GetEnumerator() => symbols.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class SymbolsExt
    {
        public static StringBuilder AppendTo(this StringBuilder builder, 
            Symbols symbols)
        {
            foreach (var symbol in symbols)
            {
                builder.AppendFormat("[{0}] = {1}", symbol.Id, symbol.Value)
                    .AppendIf(symbol.IsTerminal, "(T)")
                    .AppendLine();
            }
            return builder;
        }
    }
}
