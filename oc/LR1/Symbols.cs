using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Opal.LR1
{
    public class Symbols: IEnumerable<Symbol>
    {
        public static readonly Symbol EndOfLine;
        private readonly List<Symbol> symbols;
        private readonly Dictionary<string, Symbol> byName;

        static Symbols()
        {
            EndOfLine = new Symbol(string.Empty, 0, true);
        }

        public Symbols()
        {
            byName = new Dictionary<string, Symbol>
            {   
                { 
                    EndOfLine.Name, EndOfLine 
                }
            };
            symbols = new List<Symbol>
            {
                EndOfLine 
            };
        }

        public Symbol this[string name] => byName[name];
        public Symbol this[int id] => symbols[id];

        public int Count => symbols.Count;

        public bool TryFind(string name,
            [MaybeNullWhen(false)] out Symbol symbol) =>
            byName.TryGetValue(name, out symbol);

        public void AddSymbols(ParseTree.Symbols parseSymbols)
        {
            foreach (var parseSymbol in parseSymbols.Skip(1))
            {
                var name = parseSymbol.Name;
                if (!byName.ContainsKey(name))
                {
                    var symbol = new Symbol(name:name, 
                        id:(uint)symbols.Count, 
                        terminal: parseSymbol.Terminal,
                        text: parseSymbol.Text);
                    byName.Add(name, symbol);
                    symbols.Add(symbol);
                }
            }
        }

        public Symbol Create(string name, bool isTerminal)
        {
            if (!byName.TryGetValue(name, out var symbol))
            {
                symbol = new Symbol(name:name, 
                    id:(uint)symbols.Count, 
                    terminal: isTerminal);
                byName.Add(name, symbol);
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
}
