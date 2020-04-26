using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.Nfa
{
    public class Matches
    {
        private readonly Dictionary<IMatch, int> _data;

        public Matches()
        {
            _data = new Dictionary<IMatch, int>();
            NextId = 1;
        }

        public int this[IMatch match]
        {
            get => _data[match];
            set => _data[match] = value;
        }
        public int NextId { get; private set; }

        public int Add(IMatch match)
        {
            var result = NextId++;
            _data.Add(match, result);
            return result;
        }

        public bool Remove(IMatch match) => _data.Remove(match);

        public void Replace(IMatch oldMatch, IMatch newMatch, int value)
        {
            _data.Remove(oldMatch);
            _data.Add(newMatch, value);
        }

        public bool TryGet(IMatch match, out int id) => _data.TryGetValue(match, out id);

        public bool TryGet(char ch, out int id) => TryGet(new SingleChar(ch), out id);

        public IMatch Find(int id)
        {
            return _data
                .Where(x => x.Value == id)
                .Select(x => x.Key)
                .FirstOrDefault();
        }

        public IMatch[] GetMatches() => _data.Select(x => x.Key).ToArray();

        public StringBuilder AppendTo(StringBuilder builder)
        {
            for (var i = 1; i <= _data.Count; i++)
            {
                var item = _data.FirstOrDefault(x => x.Value == i);
                builder.AppendFormat("{0,10}", item.Key);
            }
            return builder;
        }

        public int[] ToArray()
        {
            var result = new int[char.MaxValue + 1];
            foreach (var pair in _data)
            {
                foreach (var ch in pair.Key)
                    result[ch] = pair.Value;
            }
            return result;
        }
    }
}
