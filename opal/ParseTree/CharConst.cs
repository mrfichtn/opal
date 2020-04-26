using Generators;

namespace Opal.ParseTree
{
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

        public static char FromEscCharString(string text)
        {
            char result;
            if (text.Length < 2)
            {
                result = '\0';
            }
            if (text.Length == 2)
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
                        {
                            var ch = text[i];
                            var value = FromHexDigit(ch);
                            ch = (char)((ch << 4) + value);
                        }
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
}
