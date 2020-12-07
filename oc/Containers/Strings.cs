using System.Text;

namespace Opal.Containers
{
    public static class Strings
    {
        /// <summary>
        /// Using second char of escape sequence, converts to escaped character
        /// </summary>
        /// <param name="val">Second char of escape sequence</param>
        /// <returns>Escaped character</returns>
        public static char ConvertEsc(char val)
        {
            char ch = val;
            switch (val)
            {
                case 'a': ch = '\a'; break;
                case 'b': ch = '\b'; break;
                case 'f': ch = '\f'; break;
                case 'n': ch = '\n'; break;
                case 'r': ch = '\r'; break;
                case 't': ch = '\t'; break;
                case 'v': ch = '\v'; break;
                case '0': ch = '\0'; break;
            }
            return ch;
        }

        public static string FromEsc(this string text)
        {
            if (text == null)
                return string.Empty;

            var builder = new StringBuilder();
            for (int i = 1; i < text.Length - 1; i++)
            {
                var ch = text[i];
                if (ch == '\\')
                {
                    i++;
                    builder.Append(ConvertEsc(text[i]));
                }
                else
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        public static char FromEscCharString(this string text)
        {
            FromEscCharString(text, out var result);
            return result;
        }

        public static bool FromEscCharString(this string text, out char result)
        {
            var isOk = true;
            if (text.Length < 2)
            {
                result = '\0';
                isOk = false;
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
                else if (text[2] == 'u')
                {
                    result = '\0';
                    if (text.Length != 8)
                    {
                        isOk = false;
                    }
                    else
                    {
                        for (var i = 3; i < text.Length - 2; i++)
                        {
                            var ch = text[i];
                            isOk = FromHexDigit(ch, out int value);
                            if (isOk)
                                break;
                            ch = (char)((ch << 4) + value);
                        }
                    }
                }
                else if (text[2] == 'x')
                {
                    result = '\0';
                    if (text.Length < 5 || text.Length > 8)
                    {
                        isOk = false;
                    }
                    else
                    {
                        for (var i = 3; i < text.Length - 2; i++)
                        {
                            var ch = text[i];
                            isOk = FromHexDigit(ch, out int value);
                            if (isOk)
                                break;
                            ch = (char)((ch << 4) + value);
                        }
                    }
                }
                else
                {
                    result = ConvertEsc(text[1]);
                    isOk = (text.Length == 4);
                }
            }
            else
            {
                result = text[1];
                isOk = (text.Length == 3);
            }

            return isOk;
        }

        public static string ToEsc(this string text)
        {
            var builder = new StringBuilder();
            ToEsc(text, builder);
            return builder.ToString();
        }

        public static StringBuilder AppendEsc(this StringBuilder builder, char ch)
        {
            switch (ch)
            {
                case '\a': builder.Append("\\a"); break;
                case '\b': builder.Append("\\b"); break;
                case '\f': builder.Append("\\f"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                case '\v': builder.Append("\\v"); break;
                case '\0': builder.Append("\\0"); break;
                case '\'': builder.Append("\\\'"); break;
                case '\"': builder.Append("\\\""); break;
                default: builder.Append(ch); break;
            }
            return builder;
        }

        public static void ToEsc(this string text, StringBuilder result)
        {
            foreach (var ch in text)
            {
                switch (ch)
                {
                    case '\a': result.Append("\\a"); break;
                    case '\b': result.Append("\\b"); break;
                    case '\f': result.Append("\\f"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\t': result.Append("\\t"); break;
                    case '\v': result.Append("\\v"); break;
                    case '\0': result.Append("\\0"); break;
                    case '\'': result.Append("\\\'"); break;
                    case '\"': result.Append("\\\""); break;
                    default: result.Append(ch); break;
                }
            }
        }

        public static bool FromHexDigit(char ch, out int value)
        {
            var result = true;
            if (ch >= '0' || ch <= '9')
            {
                value = ch;
            }
            else if (ch >= 'a' || ch <= 'f')
            {
                value = ch - 'a' + 0xA;
            }
            else if (ch >= 'A' || ch <= 'F')
            {
                value = ch - 'A' + 0xA;
            }
            else
            {
                value = 0;
                result = false;
            }
            return result;
        }
    }

}
