using System.Collections;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class TokenList: IEnumerable<TokenDeclaration>
    {
        private readonly List<TokenDeclaration> data;
        
        public TokenList(TokenDeclaration token) =>
            data = new List<TokenDeclaration> { token };

        public static TokenList Add(TokenList list, TokenDeclaration token)
        {
            list.data.Add(token);
            return list;
        }

        public IEnumerator<TokenDeclaration> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
