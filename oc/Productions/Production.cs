using System.Text;

namespace Opal.Productions
{
    public class Production
    {
        private readonly ParseTree.Identifier name;
        private readonly ParseTree.ActionExpr action;
        private readonly IReduction reduction;

        public Production(ParseTree.Identifier name,
            int id,
            int ruleId,
            AttributeBase attr,
            ParseTree.ActionExpr action,
            ITerminals right,
            IReduction reduction)
        {
            this.name = name;
            Id = id;
            RuleId = ruleId;
            Attribute = attr;
            this.action = action;
            Right = right;
            this.reduction = reduction;
        }

        public string Name => name.Value;
        public int RuleId { get; set;  }
        public int Id { get; }

        public AttributeBase Attribute { get; }

        public ITerminals Right { get; }

        public void Write(ProductionWriteContext context)
        {
            context.Write("case {0}:", RuleId)
                .WriteLine($" // {this}")
                .Indent();

            reduction.Write(context);
            context.WriteLine("break;")
                .UnIndent();
            
            //Right.WriteReduceProlog(context, Id);
            //action.Write(new ActionWriteContext(context, this, true));

            //context.WriteLine(");")
            //    .WriteLine("break;")
            //    .UnIndent();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Name)
                .Append(" =");

            foreach (var item in Right)
                builder.Append(' ').Append(item);
            return builder.ToString();
        }
    }
}
