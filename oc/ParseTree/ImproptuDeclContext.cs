using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.ParseTree
{
    public class ImproptuDeclContext
    {
        private Dictionary<string, int> symbolMap;
        private List<Symbol> symbols;

        public ImproptuDeclContext(Dictionary<string, int> symbolMap, 
            List<Symbol> symbols)
        {
            this.symbolMap = symbolMap;
            this.symbols = symbols;
        }

        public bool HasSymbol(string baseName) =>
            symbolMap.ContainsKey(baseName);

        public void Add(Production production)
        {
            symbols.Add(new Symbol(production.Name.Value, false));
            //Todo: add production
        }
    }
}
