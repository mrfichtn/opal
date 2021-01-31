using Generators;

namespace Opal.ParseTree
{
    public interface IConstant
    {
        object Value { get; }
    }

    public class Constant<T>: Segment, IConstant
    {
        public Constant(Segment segment, T value)
            : base(segment)
        {
            Value = value;
        }

        public T Value { get; }

        object IConstant.Value => Value!;
    }

    public class BoolConst: Constant<bool>
    {
        public BoolConst(Segment segment, bool value)
            : base(segment, value)
        { }

        public override string ToString() =>
            Value ? "true" : "false";
    }

    public class StringConst: Constant<string>
    {
        public StringConst(Token t)
            : base(t, t.Value!)
        {
        }

        public StringConst(Segment t, string value)
            : base(t, value)
        {
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class EscString: StringConst
    {
        public EscString(Token t)
            : this(t, t.Value)
        {
        }

        public EscString(Segment t, string value)
            : base(t, value.FromEsc())
        {
        }

        public override string ToString()
        {
            return Value.ToEsc();
        }
    }

    public class Integer: Constant<int>
    {
        public Integer(Segment segment, int value)
            : base(segment, value)
        {
        }
    }

    public class DecInteger: Integer
    {
        public DecInteger(Token t)
            : base(t, int.Parse(t.Value!))
        {
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
