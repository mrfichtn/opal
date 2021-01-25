namespace Opal.ParseTree
{
    public class ImproptuDeclContext
    {
        private readonly Productions.SymbolTable symbols;

        public ImproptuDeclContext(Productions.SymbolTable symbols)
        {
            this.symbols = symbols;
        }

        public bool HasSymbol(string baseName) => symbols.Contains(baseName);

        public void Add(Production production)
        {
            symbols.AddNonTerminal(production.Name.Value);
            //Todo: add production
        }
    }
}
