using Generators;
using System.Collections;
using System.Collections.Generic;

namespace Opal.Nfa
{
    public class EmptyMatch : Segment, IMatch
    {
        public static EmptyMatch Instance;

        static EmptyMatch()
        {
            var token = new Token();
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

        public bool Equals(IMatch other)
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

        public IMatch Invert(Token t)
        {
            var result = new CharClass(this);
            return result.Invert(t);
        }

        public IMatch Invert()
        {
            var result = new CharClass(this);
            return result.Invert();
        }


        public bool IsMatch(char ch)
        {
            return false;
        }

        public IMatch Reduce()
        {
            return this;
        }

        public IMatch Difference(IMatch match)
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Write(IGenerator generator, string varName)
        {
        }


        public override string ToString()
        {
            return "Ø";
        }
    }

}
