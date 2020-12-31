namespace Opal.ParseTree
{
    public class ImproptuDeclContext
    {
        private Productions.SymbolTable symbols;

        public ImproptuDeclContext(Productions.SymbolTable symbols)
        {
            this.symbols = symbols;
        }

        public bool HasSymbol(string baseName) => symbols.Contains(baseName);

        public void Add(Production production)
        {
            symbols.Add(production);
            //Todo: add production
        }
    }
}
