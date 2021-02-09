namespace Opal.ParseTree
{
    public class ProdDef
    {
        public ProdDef()
        {
            Right = new ProductionExprList();
            Action = ActionExpr.Empty;
        }

        public ProdDef(ProductionExprList right, ActionExpr? action = null)
        {
            Right = right;
            Action = action ?? ActionExpr.Empty;
        }

        public ProductionExprList Right { get; }
        public ActionExpr Action { get; }

        public void DeclareTokens(DeclareTokenContext context)
        {
            foreach (var item in Right)
                item.DeclareToken(context);
        }

        public void AddActionType(DefinitionActionTypeContext context)
        {
            context.SetExpressions(Right);
            Action.AddType(context);
        }

        //public Productions.ITerminals Build(ProductionContext context)
        //{
        //    if (Right.Count == 0)
        //        return new Productions.EmptyTerminals();
        //    if (Right.Count == 1)
        //        return new Productions.SingleTerminal(Right[0].Build(context));
            
        //    var terms = new Productions.TerminalBase[Right.Count];
        //    for (var i = 0; i < terms.Length; i++)
        //        terms[i] = Right[i].Build(context);

        //    return new Productions.Terminals(terms);
        //}

        public Productions.ITerminals Build(Productions.GrammarBuilder builder)
        {
            if (Right.Count == 0)
                return new Productions.EmptyTerminals();
            if (Right.Count == 1)
                return new Productions.SingleTerminal(Right[0].Build(builder));

            var terms = new Productions.TerminalBase[Right.Count];
            for (var i = 0; i < terms.Length; i++)
                terms[i] = Right[i].Build(builder);

            return new Productions.Terminals(terms);
        }
    }
}
