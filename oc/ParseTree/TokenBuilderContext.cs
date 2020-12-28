using Opal.Nfa;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class TokenBuilderContext
    {
        private readonly Logger logger;
        private readonly IDictionary<string, IMatch> matches;

        public TokenBuilderContext(Logger logger,
            IDictionary<string, IMatch> matches)
        {
            this.logger = logger;
            this.matches = matches;
            Graph = new Graph();
        }

        public Graph Graph { get; }
        
        public bool TryFindMatch(Identifier name, out IMatch? match)
        {
            var found = matches.TryGetValue(name.Value, out match);
            if (!found)
                logger.LogError($"Missing token system {name}",
                    name);
            return found;
        }
    }
}
