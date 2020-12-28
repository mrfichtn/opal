using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProductionExprList: List<ProductionExpr>
    {
        public ProductionExprList()
        {
        }

        public ProductionExprList(ProductionExpr expr)
        {
            Add(expr);
        }

        public static ProductionExprList Add(ProductionExprList exprs, ProductionExpr expr)
        {
            exprs.Add(expr);
            return exprs;
        }
    }
}
