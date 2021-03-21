using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ProdDefList: List<ProdDef>
    {
        public ProdDefList()
        {
        }

        public ProdDefList(ProdDef prodDef)
        {
            Add(prodDef);
        }

        public ProdDefList(params ProdDef[] prodDef)
        {
            foreach (var item in prodDef)
                Add(item);
        }


        public static ProdDefList Add(ProdDefList list, ProdDef prodDef)
        {
            list.Add(prodDef);
            return list;
        }

        public static ProdDefList Add(ProdDefList list, ProductionExprList exprs) =>
            Add(list, new ProdDef(exprs));
    }
}
