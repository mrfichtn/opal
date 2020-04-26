using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProductionExprs: List<ProductionExpr>
    {
        public ProductionExprs()
        {
        }

        public ProductionExprs(ProductionExpr expr)
        {
            Add(expr);
        }

        public static ProductionExprs Add(ProductionExprs exprs, ProductionExpr expr)
        {
            exprs.Add(expr);
            return exprs;
        }
    }
}
