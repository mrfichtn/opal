using System.Collections.Generic;
using System.Linq;

using Identifier = Opal.ParseTree.Identifier;
using Character = Opal.ParseTree.Character;

namespace Opal.Nfa
{
    /// <summary>
    /// Takes character abstract tree and builds a map between character set
    /// names and match expression
    /// </summary>
    public class CharacterClassBuilder
    {
        private readonly Logger logger;
        private readonly Dictionary<Identifier, IMatch> matches;
        private List<Character> missing;

        public CharacterClassBuilder(Logger logger)
        {
            this.logger = logger;

            matches = new Dictionary<Identifier, IMatch>(/* new Comparer() */);
            missing = new List<Character>();
        }

        public CharacterClassBuilder Characters(IEnumerable<Character> data)
        {
            foreach (var character in data)
            {
                if (Duplicate(character.name))
                    continue;
                if (!character.TryAdd(this))
                    missing.Add(character);
            }
            return this;
        }

        public Dictionary<string, IMatch> Build()
        {
            var last = 0;
            while ((missing.Count > 0) && (missing.Count != last))
            {
                var old = missing;
                last = missing.Count;
                missing = new List<Character>();
                foreach (var character in old)
                {
                    if (Duplicate(character.name))
                        continue;
                    if (!character.TryAdd(this))
                        missing.Add(character);
                }
            }

            foreach (var item in missing)
                item.expr.LogMissing(this);
            return matches.ToDictionary(x => x.Key.Value, y => y.Value);
        }

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


        private bool Duplicate(Identifier name)
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
    }
}
