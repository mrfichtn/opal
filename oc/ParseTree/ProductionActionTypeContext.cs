using Opal.Productions;
using System;

namespace Opal.ParseTree
{
    public class ProductionActionTypeContext
    {
        private readonly TypeTable typeTable;
        private readonly MissingReferenceTable missing;

        public ProductionActionTypeContext(TypeTable typeTable)
        {
            this.typeTable = typeTable;
            this.missing = new MissingReferenceTable();
        }

        public void Add(string productionName, string type) =>
            typeTable.AddPrimary(productionName, type);

        public DefinitionActionTypeContext DefinitionContext(string productionName) =>
            new DefinitionActionTypeContext(typeTable,
                productionName,
                missing);

        public void Resolve() => 
            missing.Resolve(typeTable);
    }
}
