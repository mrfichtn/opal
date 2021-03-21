using System;

namespace Opal.ParseTree
{
    /// <summary>
    /// Identifier
    /// </summary>
    public class Identifier: Segment, IEquatable<Identifier>
    {
        public Identifier(Segment s, string value)
            : base(s)
        {
            Value = value;
        }

        protected Identifier(Identifier id, Token t)
            : base(id.Start, t.End)
        {
            Value = id.Value + "." + t.Value;
        }

        public Identifier(Token t)
            : this(t, t.Value)
        {
        }

        public Identifier(string value) =>
            Value = value;

        public static Identifier Add(Identifier id, Token t) =>
            new Identifier(id, t);

        public string Value { get; }

        public override string ToString() => Value;

        public static Identifier MakeType(Identifier id, GenericArgs args) =>
            new Identifier(id, string.Format("{0}<{1}>", id, args));

        public bool Equals(Identifier? other) =>
            string.Equals(Value, other?.Value);

        public override bool Equals(object? obj) =>
            Equals(obj as Identifier);

        public override int GetHashCode() =>
            Value.GetHashCode();
    }
}
