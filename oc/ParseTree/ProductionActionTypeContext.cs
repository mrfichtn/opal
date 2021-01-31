using Opal.Productions;

namespace Opal.ParseTree
{
    public class ProductionActionTypeContext
    {
        private readonly TypeTable typeTable;
        private readonly MissingReferenceTable missing;

        public ProductionActionTypeContext(TypeTable typeTable)
        {
            this.typeTable = typeTable;
            missing = new MissingReferenceTable();
        }

        public void Add(string productionName, ProductionAttr attr) =>
            typeTable.TypeFromAttr(productionName, attr.NullableType);

        public DefinitionActionTypeContext DefinitionContext(string productionName) =>
            new DefinitionActionTypeContext(typeTable,
                productionName,
                missing);

        public void Resolve() => missing.Resolve(typeTable);
    }
}
