using Opal.Nfa;
using System.Collections;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class CharacterList: IEnumerable<Character>
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

        public IEnumerator<Character> GetEnumerator() =>
            data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
