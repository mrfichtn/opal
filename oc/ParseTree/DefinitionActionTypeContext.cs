using Opal.Productions;

namespace Opal.ParseTree
{
    public class DefinitionActionTypeContext
    {
        private readonly TypeTable typeTable;
        private readonly string productionName;
        private readonly MissingReferenceTable missing;
        private ProductionExprList? expressions;
        
        public DefinitionActionTypeContext(TypeTable typeTable,
            string productionName,
            MissingReferenceTable missing)
        {
            this.typeTable = typeTable;
            this.productionName = productionName;
            this.missing = missing;
        }

        public void Add(string typeName) =>
            typeTable.AddActionType(productionName, typeName);

        public void SetExpressions(ProductionExprList expressions) =>
            this.expressions = expressions;

        public bool AddFromActionExpr(int exprIndex)
        {
            bool result;
            if (exprIndex < expressions!.Count)
            {
                var name = expressions[exprIndex].Name;
                result = typeTable.TryFindNullable(name, out var type);
                if (result)
                    typeTable.AddActionType(productionName, type!);
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
