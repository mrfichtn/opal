using System.Collections.Generic;
using Opal.ParseTree;

namespace Opal.Nfa
{
    public class NfaBuilder
    {
        private readonly Logger logger;
        private Dictionary<string, IMatch> charMap;
        private Graph? graph;

        public NfaBuilder(Logger logger)
        {
            this.logger = logger;
            charMap = new Dictionary<string, IMatch>();
        }

        public NfaBuilder Characters(IEnumerable<Character> characters)
        {
            charMap = new CharacterClassBuilder(logger)
                .Characters(characters)
                .Build();
            return this;
        }

        public NfaBuilder Tokens(TokenList tokens)
        {
            graph = new GraphBuilder(logger)
                .Matches(charMap)
                .Tokens(tokens)
                .Build();
            return this;
        }

        public NfaBuilder Productions(ProductionSection section)
        {
            if (graph != null)
            {
                var context = new ParseTree.DeclareTokenContext(logger, graph);
                section.AddStringTokens(context);
            }
            return this;
        }

        public Graph? Build() => graph;
    }
}
