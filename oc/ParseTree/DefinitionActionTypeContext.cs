using Opal.Productions;

namespace Opal.ParseTree
{
    public class DefinitionActionTypeContext
    {
        private readonly TypeTable typeTable;
        private readonly MissingReferenceTable missing;
        private readonly INoAction noAction;
        private readonly string productionName;
        private ProductionExprList? expressions;
        
        public DefinitionActionTypeContext(TypeTable typeTable,
            MissingReferenceTable missing,
            INoAction noAction,
            string productionName)
        {
            this.typeTable = typeTable;
            this.missing = missing;
            this.noAction = noAction;
            this.productionName = productionName;
        }

        public void Add(string? typeName) =>
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

        public void AddTypeFromActionEmpty()
        {
            if ((expressions == null) || (expressions.Count == 0))
                Add(null);
            else if (expressions.Count == 1)
                AddFromActionExpr(0);
            else
                noAction.AddType(this);
        }
    }
}
