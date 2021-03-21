using Opal.Productions;

namespace Opal.ParseTree
{
    public class ProductionActionTypeContext
    {
        private readonly TypeTable typeTable;
        private readonly MissingReferenceTable missing;
        private readonly INoAction noAction;

        public ProductionActionTypeContext(TypeTable typeTable,
            INoAction noAction)
        {
            this.typeTable = typeTable;
            this.noAction = noAction;
            missing = new MissingReferenceTable();
        }

        public void Add(string productionName, ProductionAttr attr) =>
            typeTable.TypeFromAttr(productionName, attr.NullableType);

        public DefinitionActionTypeContext DefinitionContext(string productionName) =>
            new DefinitionActionTypeContext(typeTable,
                missing,
                noAction,
                productionName);

        public void Resolve() => missing.Resolve(typeTable);
    }
}
