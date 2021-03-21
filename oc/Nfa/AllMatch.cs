using Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Nfa
{
    public class AllMatch: Segment, IMatch
    {
        public AllMatch()
        { }

        public AllMatch(Token t)
            : base(t)
        {
        }
        
        public int Count => char.MaxValue + 1;

        public int WriteCount => 1;

        public IMatch Difference(IMatch match) => match.Invert();

        public bool Equals(IMatch? other) => 
            (other != null) && other.Count == Count;

        public IEnumerator<char> GetEnumerator()
        {
            for (var i = 0; i <= char.MaxValue; i++)
                yield return (char)i;
        }

        public IMatch? Intersect(IMatch match) => match;

        public IMatch Invert() => new EmptyMatch();

        public IMatch Invert(Token t) => new EmptyMatch(t);

        public bool IsMatch(char ch) => true;

        public IMatch Reduce() => this;

        public IMatch Union(IMatch match) => this;

        public string SwitchCondition(string varName) => "true";

        public override string ToString() => "𝕌";

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
