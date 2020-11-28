using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Dfa
{
    public static class Identifier
    {
        public static string SafeName(string name)
        {
            string result;
            if (string.IsNullOrEmpty(name))
            {
                result = "∅";
            }
            else
            {
                var builder = new StringBuilder();
                ReplaceId1Char(builder, name[0]);
                for (int i = 1; i < name.Length; i++)
                    ReplaceId2Char(builder, name[i]);
                result = builder.ToString();
            }
            return ReplaceKeyword(result);
        }

        public static void ReplaceId1Char(StringBuilder builder, char ch)
        {
            if (ch >= '0' && ch <= '9')
            {
                builder.Append('_')
                    .Append(ch);
            }
            else
            {
                ReplaceAnyChar(builder, ch);
            }
        }

        public static void ReplaceId2Char(StringBuilder builder, char ch)
        {
            if (ch == '@')
                builder.Append("AtSign");
            else
                ReplaceAnyChar(builder, ch);
        }

        public static void ReplaceAnyChar(StringBuilder builder, char ch)
        {
            switch (ch)
            {
                case '!': builder.Append("Exclamation"); break;
                case '#': builder.Append("Hash"); break;
                case '$': builder.Append("Dollar"); break;
                case '%': builder.Append("Percent"); break;
                case '^': builder.Append("Circumflex"); break;
                case '&': builder.Append("Ampersand"); break;
                case '*': builder.Append("Asterisk"); break;
                case '(': builder.Append("LeftParen"); break;
                case ')': builder.Append("RightParen"); break;
                case '-': builder.Append("Minus"); break;
                case '+': builder.Append("Plus"); break;
                case '=': builder.Append("Equal"); break;
                case '{': builder.Append("LeftCurly"); break;
                case '}': builder.Append("RightCurly"); break;
                case '[': builder.Append("LeftSquare"); break;
                case ']': builder.Append("RightSquare"); break;
                case '|': builder.Append("VerticalBar"); break;
                case ':': builder.Append("Colon"); break;
                case ';': builder.Append("Semicolon"); break;
                case '<': builder.Append("LessThan"); break;
                case '>': builder.Append("GreaterThan"); break;
                case ',': builder.Append("Comma"); break;
                case '.': builder.Append("Period"); break;
                case '?': builder.Append("QuestionMark"); break;
                case '/': builder.Append("Slash"); break;
                case '~': builder.Append("Tilde"); break;
                case '`': builder.Append("GraveAccent"); break;
                case ' ': builder.Append("Space"); break;
                case '\t': builder.Append("Tab"); break;
                case '\n': builder.Append("Newline"); break;
                case '\r': builder.Append("Return"); break;
                case '\0': builder.Append("NullTerminator"); break;
                case '\\': builder.Append("Backslash"); break;
                case '\"': builder.Append("DoubleQuote"); break;
                case '\'': builder.Append("Quote"); break;
                default: builder.Append(ch); break;
            }
        }

        public static string ReplaceKeyword(string str)
        {
            var isKeyword = keyWords.Contains(str);
            if (isKeyword)
            {
                var builder = new StringBuilder();
                builder.Append(char.ToUpper(str[0]));
                for (int i = 1; i < str.Length; i++)
                    builder.Append(str[i]);
                str = builder.ToString();
            }
            return str;
        }

        private static readonly HashSet<string> keyWords = new HashSet<string>
        {
            "abstract", "as",       "base",     "bool",     "break",    "byte",     "case",     "catch",    "char",     "checked",
            "class",    "const",    "continue", "decimal",  "default",  "delegate", "do",       "double",   "else",     "enum",
            "event",    "explicit", "extern",   "false",    "finally",  "fixed",    "float",    "for",      "foreach",  "goto",
            "if",       "implicit", "in",       "int",      "interface","internal", "is",       "lock",     "long",     "namespace",
            "new",      "null",     "object",   "operator", "out",      "override", "params",   "private",  "protected","public",
            "readonly", "ref",      "return",   "sbyte",    "sealed",   "short",    "sizeof",   "stackalloc","static",  "string",
            "struct",   "switch",   "this",     "throw",    "true",     "try",      "typeof",   "uint",     "ulong",    "unchecked",
            "unsafe",   "ushort",   "using",    "virtual",  "void",     "volatile", "while"
        };

    }
}
