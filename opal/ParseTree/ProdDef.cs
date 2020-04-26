namespace Opal.ParseTree
{
    public class ProdDef
    {
        public ProdDef()
        {
            Right = new ProductionExprs();
        }

        public ProdDef(ProductionExprs right, ActionExpr action = null)
        {
            Right = right;
            Action = action;
        }

        public ProductionExprs Right { get; }
        public ActionExpr Action { get; }
    }
}
