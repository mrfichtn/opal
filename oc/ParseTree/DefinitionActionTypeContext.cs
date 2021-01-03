using Opal.Productions;

namespace Opal.ParseTree
{
    public class DefinitionActionTypeContext
    {
        private readonly TypeTable typeTable;
        private readonly string productionName;
        private readonly MissingReferenceTable missing;
        private ProductionExprList expressions;
        
        public DefinitionActionTypeContext(TypeTable typeTable,
            string productionName,
            MissingReferenceTable missing)
        {
            this.typeTable = typeTable;
            this.productionName = productionName;
            this.missing = missing;
        }

        public void Add(string typeName) =>
            typeTable.AddSecondary(productionName, typeName);

        public void SetExpressions(ProductionExprList expressions) =>
            this.expressions = expressions;

        public bool TryFind(int index, ActionExpr action)
        {
            bool result;
            if (index < expressions.Count)
            {
                var name = expressions[index].Name;
                result = typeTable.TryFind(name, out var type);
                if (result)
                    typeTable.AddSecondary(productionName, type!);
                else
                    missing.Add(productionName, name);
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
