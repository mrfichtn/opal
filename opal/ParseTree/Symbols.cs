using Opal.Index;
using System.Collections.Generic;
using Opal.Containers;
using System.Collections;

namespace Opal.ParseTree
{
    public class Symbols: IEnumerable<Symbol>
    {
        private readonly Index<string> index;
        private readonly List<Symbol> symbols;

        public Symbols()
        {
            index = new Index<string>();
            symbols = new List<Symbol>();
        }

        public int Count => index.Count;

        public void Add(IEnumerable<Nfa.Symbol> nfaSymbols)
        {
            foreach (var nfaSymbol in nfaSymbols)
            {
                var newIndex = index.Add(nfaSymbol.Name);
                var symbol = new Symbol(name: nfaSymbol.Name,
                    terminal: true,
                    text: nfaSymbol.Text);
                symbols.Set(newIndex, symbol);
            }
        }

        public int AddOrGet(string name)
        {
            if (!index.TryGetIndex(name, out var result))
                result = Add(name);
            return result;
        }

        public bool TryGetIndex(string name, out int result) =>
            index.TryGetIndex(name, out result);

        public int Add(string name)
        {
            var id = index.Add(name);
            symbols.Add(new Symbol(name, false));
            return id;
        }

        public IEnumerator<Symbol> GetEnumerator() => symbols.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
