using System.Collections.Generic;
using Opal.ParseTree;

namespace Opal.Nfa
{
    public class GraphBuilder
    {
        private static readonly IDictionary<string, IMatch> emptyMatches;

        private readonly Logger logger;
        private IDictionary<string, IMatch> matches;
        private Graph graph;

        static GraphBuilder() =>
            emptyMatches = new Dictionary<string, IMatch>();

        public GraphBuilder(Logger logger)
        {
            this.logger = logger;
            matches = emptyMatches;
            graph = new Graph();
        }

        public Graph Build() => graph;

        public GraphBuilder Matches(IDictionary<string, IMatch> value)
        {
            matches = value;
            return this;
        }

        public GraphBuilder Tokens(IEnumerable<TokenDeclaration> tokens)
        {
            foreach (var token in tokens)
            {
                var newGraph = token.Build(this);
                graph = graph.Union(newGraph);
            }
            return this;
        }

        public bool TryFindMatch(Identifier name, out IMatch? match)
        {
            var found = matches.TryGetValue(name.Value, out match);
            if (!found)
                logger.LogError($"Missing token system {name}",
                    name);
            return found;
        }

        public Graph Create(IMatch match) => graph.Create(match);

        public Graph Create() => graph.Create();

        public Graph Create(StringConst str) => graph.Create(str.Value);
    }
}
