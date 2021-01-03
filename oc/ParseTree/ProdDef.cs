using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProdDef
    {
        public ProdDef()
        {
            Right = new ProductionExprList();
        }

        public ProdDef(ProductionExprList right, ActionExpr? action = null)
        {
            Right = right;
            Action = action;
        }

        public ProductionExprList Right { get; }
        public ActionExpr? Action { get; }

        public void DeclareTokens(DeclareTokenContext context)
        {
            foreach (var item in Right)
                item.DeclareToken(context);
        }

        public void AddActionType(DefinitionActionTypeContext context)
        {
            if (Action != null)
            {
                context.SetExpressions(Right);
                Action.AddType(context);
            }
        }


        public IEnumerable<ProductionExpr> Expressions => Right;

        public Productions.TerminalBase[] Build(ProductionContext context)
        {
            var symbols = new Productions.TerminalBase[Right.Count];
            for (var i = 0; i < symbols.Length; i++)
                symbols[i] = Right[i].Build(context);
            
            return symbols;
        }

    }
}
