using System.Collections.Generic;

namespace Opal.Nfa
{
    public class Symbols
    {
        private readonly Dictionary<string, Symbol> data;

        public Symbols() =>
            data = new Dictionary<string, Symbol>();

        public Symbol this[string name] => data[name];

        public Symbol Add(Symbol symbol)
        {
            data.Add(symbol.Name, symbol);
            return symbol;
        }


        public bool TryGetIndex(string key, out int index)
        {
            var found = data.TryGetValue(key, out var symbol);
            index = found ? symbol!.Index : 0;
            return found;
        }
    }
}
