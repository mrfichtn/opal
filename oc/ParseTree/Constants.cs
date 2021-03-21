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

    public class CharConst: Constant<char>
    {
        public CharConst(Segment s, char ch)
            : base(s, ch)
        {
        }
    }

    public class EscChar: CharConst
    {
        public EscChar(Token t)
            : base(t, FromEscCharString(t.Value))
        {
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static char FromEscCharString(string? text)
        {
            char result;
            if ((text == null) || (text.Length <= 2))
            {
                result = '\0';
            }
            else if (text[1] == '\\')
            {
                if (text.Length == 3)
                {
                    result = text[1];
                }
                else if (text[2] == 'x' || text[2] == 'u')
                {
                    result = '\0';
                    if (text.Length >= 5 && text.Length <= 8)
                    {
                        for (var i = 3; i < text.Length - 2; i++)
                            result = (char)((result << 4) + FromHexDigit(text[i]));
                    }
                }
                else
                {
                    result = Strings.ConvertEsc(text[1]);
                }
            }
            else
            {
                result = text[1];
            }
            return result;
        }

        public static int FromHexDigit(char ch)
        {
            int value;
            if (ch >= '0' || ch <= '9')
                value = ch;
            else if (ch >= 'a' || ch <= 'f')
                value = ch - 'a' + 0xA;
            else if (ch >= 'A' || ch <= 'F')
                value = ch - 'A' + 0xA;
            else
                value = 0;
            return value;
        }
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
