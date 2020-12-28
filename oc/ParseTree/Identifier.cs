using Generators;
using System;

namespace Opal.ParseTree
{
    /// <summary>
    /// Identifier
    /// </summary>
    public class Identifier: Segment, IGeneratable
    {
        public Identifier(Segment s, string value)
            : base(s)
        {
            Value = value;
        }

        public Identifier(Identifier id, Token t)
            : base(id.Start, t.End)
        {
            Value = id.Value + "." + t.Value;
        }

        public Identifier(Identifier id, Identifier id2)
            : base(id.Start, id2.End)
        {
            Value = id.Value + "." + id2.Value;
        }

        public Identifier(Token t)
            : base(t)
        {
            Value = t.Value!;
        }

        public Identifier(string value)
        {
            Value = value;
        }

        public static Identifier Add(Identifier id, Token t)
        {
            id.End = t.End;
            id.Value += "." + t.Value;
            return id;
        }

        public string Value { get; private set; }

        public override string ToString()
        {
            return Value;
        }

        public static bool Equals(Identifier id1, Identifier id2)
        {
            bool result;
            if (id1 == null)
                result = (id2 == null);
            else if (id2 == null)
                result = false;
            else
                result = id1.Value == id2.Value;

            return result;
        }

        public void Write(Generator generator)
        {
            generator.Write(Value);
        }

        public static string BuildList(string list, Identifier id2)
        {
            return string.Format("{0},{1}", list, id2);
        }

        public static Identifier MakeType(Identifier id, GenericArgs args)
        {
            return new Identifier(id, string.Format("{0}<{1}>", id, args));
        }
    }
}
