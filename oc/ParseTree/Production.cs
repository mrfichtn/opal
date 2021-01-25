using System.Collections.Generic;
using System.Linq;

namespace Opal.ParseTree
{
    public class Production
    {
        private readonly Identifier name;
        private readonly ProductionAttr? attr;
        private readonly ProdDefList definitions;
        
        public Production(Token name, 
            ProductionAttr? attr, 
            ProdDefList definitions)
            : this(new Identifier(name), attr, definitions)
        {
        }

        public Production(Identifier name,
            ProductionAttr? attr,
            ProdDefList definitions)
        {
            this.name = name;
            this.attr = attr;
            this.definitions = definitions;
        }

        public ProductionAttr? Attribute => attr;

        public Identifier Name => name;

        public ProdDefList Definitions => definitions;

        public void DeclareTokens(DeclareTokenContext context)
        {
            foreach (var item in definitions)
                item.DeclareTokens(context);
        }

        public Productions.AttributeBase BuildAttribute()
        {
            Productions.AttributeBase result;
            if (attr == null)
                result = new Productions.NoAttribute();
            else if (attr.Option.Value == "ignore")
                result = new Productions.IgnoreAttribute();
            else if (attr.IsMethod)
                result = new Productions.MethodAttribute(attr.Option);
            else if (attr.Option.Value == "true" || attr.Option.Value == "false")
                result = new Productions.ValueAttribute(attr.Option.Value);
            else
                result = new Productions.NewAttribute(attr.Option);
            return result;
        }

        public IEnumerable<ProductionExpr> Expressions =>
            definitions.SelectMany(x => x.Right);

        public void AddActionType(ProductionActionTypeContext context)
        {
            if ((attr != null) && 
                !attr.IsMethod && 
                (attr.Option != null) &&
                (attr.Option.Value != "ignore"))
            {
                context.Add(name.Value, attr.Option.Value);
            }

            var defContext = context.DefinitionContext(name.Value);
            foreach (var def in Definitions)
                def.AddActionType(defContext);
        }
    }
}
