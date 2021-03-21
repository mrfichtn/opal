namespace Opal.ParseTree
{
    public class NullableType: Segment
    {
        private readonly Identifier id;
        private readonly bool nullable;
        
        public NullableType(Identifier id, Token t)
            : base(id.Start, t.End)
        {
            this.id = id;
            this.nullable = true;
        }

        public NullableType(Identifier id)
            : base(id)
        {
            this.id = id;
        }

        public Productions.NullableType Type =>
            new Productions.NullableType(id.Value, nullable);

        public string TypeName => id.Value;
    }
}
