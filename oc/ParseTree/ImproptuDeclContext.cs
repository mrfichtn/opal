namespace Opal.ParseTree
{
    public class ImproptuDeclContext
    {
        private readonly Productions.SymbolTable symbols;
        private readonly Productions.TypeTable typeTable;

        public ImproptuDeclContext(Productions.SymbolTable symbols,
            Productions.TypeTable typeTable)
        {
            this.symbols = symbols;
            this.typeTable = typeTable;
            Productions = new ProductionList();
        }

        public ProductionList Productions { get; }

        public bool HasSymbol(string baseName) => symbols.Contains(baseName);

        public bool TryFindType(string name, out string? type) =>
            typeTable.TryFind(name, out type);

        public void Add(Production production)
        {
            symbols.AddNonTerminal(production.Name.Value);
            Productions.Add(production);
        }

        public void CopyTo(ProductionList prods)
        {
            foreach (var prod in Productions)
                prods.Add(prod);
        }
    }
}
