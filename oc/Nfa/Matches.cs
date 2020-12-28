using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.Nfa
{
    public class Matches: IEnumerable<IMatch>
    {
        private readonly Dictionary<IMatch, int> data;

        public Matches()
        {
            data = new Dictionary<IMatch, int>();
            NextId = 1;
        }

        public int this[IMatch match]
        {
            get => data[match];
            set => data[match] = value;
        }

        public int NextId { get; private set; }

        public int Add(IMatch match)
        {
            var result = NextId++;
            data.Add(match, result);
            return result;
        }

        public bool Remove(IMatch match) => data.Remove(match);

        public void Replace(IMatch oldMatch, IMatch newMatch, int value)
        {
            data.Remove(oldMatch);
            data.Add(newMatch, value);
        }

        public bool TryGet(IMatch match, out int id) => data.TryGetValue(match, out id);

        public bool TryGet(char ch, out int id) => TryGet(new SingleChar(ch), out id);

        public IMatch? Find(int id)
        {
            return data
                .Where(x => x.Value == id)
                .Select(x => x.Key)
                .FirstOrDefault();
        }

        public IMatch[] GetMatches() => data.Select(x => x.Key).ToArray();

        public StringBuilder AppendTo(StringBuilder builder)
        {
            for (var i = 1; i <= data.Count; i++)
            {
                var item = data.FirstOrDefault(x => x.Value == i);
                builder.AppendFormat("{0,10}", item.Key);
            }
            return builder;
        }

        public int[] ToArray()
        {
            var result = new int[char.MaxValue + 1];
            foreach (var pair in data)
            {
                foreach (var ch in pair.Key)
                    result[ch] = pair.Value;
            }
            return result;
        }

        public IEnumerator<IMatch> GetEnumerator() => 
            data.Select(x => x.Key).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
