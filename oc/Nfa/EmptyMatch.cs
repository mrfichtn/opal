using Generators;
using System.Collections;
using System.Collections.Generic;

namespace Opal.Nfa
{
    public class EmptyMatch : Segment, IMatch
    {
        public readonly static EmptyMatch Instance;

        static EmptyMatch()
        {
            var token = new Token(new Position(), new Position(), 0, string.Empty);
            Instance = new EmptyMatch(token);
        }

        public EmptyMatch()
        {
        }

        public EmptyMatch(Token t)
            : base(t)
        {
        }

        #region Properties

        public int Count => 0;
        public int WriteCount => 0;

        #endregion

        public bool Equals(IMatch? other)
        {
            return other == Instance;
        }

        public IEnumerator<char> GetEnumerator()
        {
            yield break;
        }

        public IMatch? Intersect(IMatch match)
        {
            return null;
        }

        public IMatch Union(IMatch match)
        {
            return match;
        }

        public IMatch Invert(Token t) => new AllMatch(t);

        public IMatch Invert() => new AllMatch();

        public bool IsMatch(char ch) => false;

        public IMatch Reduce() => this;

        public IMatch Difference(IMatch match) => this;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Write(IGenerator generator, string varName) =>
            generator.Write("false");

        public string SwitchWriter(string varName) =>
            "false";


        public override string ToString() => "Ø";
    }

}
