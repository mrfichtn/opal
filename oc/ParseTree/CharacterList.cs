using Opal.Nfa;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class CharacterList
    {
        private readonly List<Character> data;

        public CharacterList()
        {
            data = new List<Character>();
        }
        
        public CharacterList(Character character)
        {
            data = new List<Character>()
            {
                character
            };
        }

        public static CharacterList Add(CharacterList list, 
            Character character)
        {
            list.data.Add(character);
            return list;
        }

        public Dictionary<string, IMatch> Build(Logger logger)
        {
            var context = new CharacterContext(logger);
            var missing = new List<Character>();
            var last = 0;

            foreach (var character in data)
            {
                if (context.Duplicate(character.name))
                    continue;
                if (!character.TryAdd(context))
                    missing.Add(character);
            }
            while (missing.Count > 0 && missing.Count != last)
            {
                var old = missing;
                missing = new List<Character>();
                foreach (var character in old)
                {
                    if (context.Duplicate(character.name))
                        continue;
                    if (!character.TryAdd(context))
                        missing.Add(character);
                }
            }
            foreach (var item in missing)
                item.expr.LogMissing(context);

            return context.Matches;
        }
    }
}
