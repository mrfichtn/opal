using Generators;
using System.Text;

namespace Opal.Productions
{
    public class Production
    {
        private readonly ParseTree.Identifier name;
        private readonly IReduction reduction;

        public Production(ParseTree.Identifier name,
            int id,
            int ruleId,
            ITerminals right,
            IReduction reduction)
        {
            this.name = name;
            Id = id;
            RuleId = ruleId;
            Right = right;
            this.reduction = reduction;
        }

        public string Name => name.Value;
        public int RuleId { get; set;  }
        public int Id { get; }

        public ITerminals Right { get; }

        public void Write<T>(T context) where T:Generator<T>
        {
            context.Write("case {0}:", RuleId)
                .WriteLine($" // {this}")
                .Indent();

            reduction.Write(context);
            context.WriteLine("break;")
                .UnIndent();
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
