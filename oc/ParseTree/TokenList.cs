using Opal.Nfa;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class TokenList
    {
        private readonly List<TokenDeclaration> data;
        public TokenList(TokenDeclaration token)
        {
            data = new List<TokenDeclaration> { token };
        }

        public static TokenList Add(TokenList list, TokenDeclaration token)
        {
            list.data.Add(token);
            return list;
        }

        public Graph? Build(Logger logger, 
            IDictionary<string, IMatch> matches)
        {
            var tokenContext = new TokenBuilderContext(logger, matches);
            Graph? result = null;
            foreach (var item in data)
            {
                var graph = item.Build(tokenContext);
                result = (result == null) ?
                    graph :
                    result.Union(graph);
            }
            return result;
        }
    }
}
