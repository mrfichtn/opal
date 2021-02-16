using System.Collections.Generic;
using System.Linq;

namespace Opal.ParseTree
{
    public class InternalProductions
    {
        private readonly Dictionary<string, InternalProduction> productions;

        public InternalProductions()
        {
            productions = new Dictionary<string, InternalProduction>();
        }

        public void AddOption(string name, ProductionExpr expr)
        {
            if (!productions.ContainsKey(name))
                productions.Add(name, new OptionInternalProd(name, expr));
        }

        public void AddList(string name, ProductionExpr expr)
        {
            if (!productions.ContainsKey(name))
                productions.Add(name, new OptionInternalProd(name, expr));
        }


        public void Create(ProductionList productions)
        {
            foreach (var pair in this.productions)
            {
                if (!productions.Any(x=>x.Name.Value == pair.Key))
                    pair.Value.Add(productions);
            }
        }
    }
    
    public abstract class InternalProduction
    {
        protected readonly Identifier id;
        protected readonly ProductionExpr expr;
        
        public InternalProduction(string name,
            ProductionExpr expr)
        {
            id = new Identifier(name);
            this.expr = expr;
        }

        public abstract void Add(ProductionList productions);

    }

    public class OptionInternalProd: InternalProduction
    {
        public OptionInternalProd(string name, ProductionExpr expr)
            : base(name, expr)
        { }
        
        public override void Add(ProductionList list)
        {
            var definitions = new ProdDefList(
                new ProdDef(new ProductionExprList(expr)));

            var production = new Production(name: id,
                attr: null,
                definitions: definitions);
            list.Add(production);

            production = new Production(name: id,
                attr: null,
                definitions: new ProdDefList(new ProdDef()));
            list.Add(production);
        }
    }

    public class ListInternalProd: InternalProduction
    {
        public ListInternalProd(string name, ProductionExpr expr)
            : base(name, expr)
        { }

        public override void Add(ProductionList list)
        {
            //var def = new ProdDef(new ProductionExprs
            //var production = new Production(id: id,
            //    definition: new ProdDef(new ProductionExprs(expr)));
            //list.Add(production);

            //production = new Production(id: id,
            //    definition: new ProdDef());
            //list.Add(production);
        }
    }
}
