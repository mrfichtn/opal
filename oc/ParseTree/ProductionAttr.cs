using System.Text;

namespace Opal.ParseTree
{
    public class ProductionAttr: Segment
    {
        public ProductionAttr(Identifier type, bool nullable)
            : base(type)
        {
            Type = type;
        }

        public Identifier Type { get; }
        public bool IsNullable { get; }

        public NullableType NullableType =>
            new NullableType(Type.Value, IsNullable);

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Type);
            if (IsNullable)
                builder.Append('?');
            return builder.ToString();
        }
    }
}
