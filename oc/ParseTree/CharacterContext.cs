using Opal.Nfa;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Opal.ParseTree
{
    public class CharacterContext
    {
        private readonly Dictionary<Identifier, IMatch> matches;
        private readonly Logger logger;
        
        public CharacterContext(Logger logger)
        {
            matches = new Dictionary<Identifier, IMatch>(new Comparer());
            this.logger = logger;
        }

        public Dictionary<string, IMatch> Matches =>
            matches.
                ToDictionary(x => x.Key.Value, y => y.Value);

        public bool TryFind(Identifier name, out IMatch? match) =>
            matches.TryGetValue(name, out match);

        public void Add(Identifier name, IMatch match) =>
            matches.Add(name, match);

        public void LogMissing(Identifier name)
        {
            logger.LogError(
                $"Missing character class '{name.Value}'",
                name);
        }

        public bool Duplicate(Identifier name)
        {
            var found = matches.ContainsKey(name);
            if (found)
            {
                var key = matches.Keys
                    .FirstOrDefault(x => x.Value == name.Value);
                logger.LogError(
                    $"Duplicate character definition '{name.Value}' (old={key!.Start})",
                    name);
            }
            return found;
        }

        class Comparer: IEqualityComparer<Identifier>
        {
            public bool Equals(Identifier? x, Identifier? y) =>
                string.Equals(x?.Value, y?.Value);

            public int GetHashCode(Identifier obj)
                => obj.Value.GetHashCode();
        }
    }
}
