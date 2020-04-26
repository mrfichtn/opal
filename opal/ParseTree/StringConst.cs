using Generators;

namespace Opal.ParseTree
{
    public class StringConst: Constant<string>
    {
        public StringConst(Token t)
            : base(t, t.Value)
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

    public class EscString : StringConst
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
}
