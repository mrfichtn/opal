using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Productions
{
    public class SymbolTable
    {
        private readonly Dictionary<string, int> map;
        private readonly List<Symbol> symbols;

        public SymbolTable()
        {
            map = new Dictionary<string, int>();
            symbols = new List<Symbol>();
        }

        public List<Symbol> Symbols => symbols;

        public void Add(Nfa.Symbol symbol)
        {
            map.Add(symbol.Name, symbol.Index);
            symbols.Add(new Symbol(symbol.Name, true, symbol.Text));
        }

        public void Add(ParseTree.Production production)
        {
            var name = production.Name.Value;
            if (!map.ContainsKey(name))
            {
                map.Add(name, map.Count);
                symbols.Add(new Symbol(production.Name.Value, false));
            }
        }
        public bool Contains(string key) => map.ContainsKey(key);

        public bool TryGetValue(string name, 
            out int id, 
            out bool isTerminal)
        {
            var result = map.TryGetValue(name, out id);
            isTerminal = result && symbols[id].Terminal;
            return result;
        }
    }
}
