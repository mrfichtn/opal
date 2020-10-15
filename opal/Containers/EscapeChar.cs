using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Containers
{
    public static class EscapeChar
    {
        public static IEnumerable<char> ToEscape(this char ch)
        {
            switch (ch)
            {
                case '\a': yield return '\\'; yield return 'a'; break;
                case '\b': yield return '\\'; yield return 'b'; break;
                case '\f': yield return '\\'; yield return 'f'; break;
                case '\n': yield return '\\'; yield return 'n'; break;
                case '\r': yield return '\\'; yield return 'r'; break;
                case '\t': yield return '\\'; yield return 't'; break;
                case '\v': yield return '\\'; yield return 'v'; break;
                case '\0': yield return '\\'; yield return '0'; break;
                case '\'': yield return '\\'; yield return '\''; break;
                case '\"': yield return '\\'; yield return '"'; break;
                case '\\': yield return '\\'; yield return '\\'; break;
                default: yield return (ch); break;
            }
        }

        public static IEnumerable<char> ToEscape(this IEnumerable<char> source) =>
            source.SelectMany(x => ToEscape(x));
    }
}
