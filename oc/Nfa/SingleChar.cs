using Generators;
using Opal.Containers;
using Opal.ParseTree;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Opal.Nfa
{
    /// <summary>
    /// Single character
    /// </summary>
    public class SingleChar : Segment, IMatch
    {
        public SingleChar(Segment s)
            : base(s)
        {
        }

        public SingleChar(Token t)
            : base(t)
        {
            Ch = Containers.Strings.FromEscCharString(t.Value!);
        }

        public SingleChar(char ch) =>
            Ch = ch;

        public SingleChar(CharConst ch)
            : base(ch)
        {
            Ch = ch.Value;
        }

        #region Properties

        public char Ch { get; private set; }
        public int Count => 1;
        public int WriteCount => 1;

        #endregion

        public void Merge(int[] map, int state)
        {
            if (map[Ch] == 0)
                map[Ch] = state;
        }

        public override bool Equals(object? obj) =>
            (obj is SingleChar ch) && (Ch == ch.Ch);

        public bool Equals(SingleChar ch) => (Ch == ch.Ch);

        public string SwitchCondition(string varName) =>
            $"{varName}=='{Ch.ToEsc()}'";

        public override string ToString()
        {
            var builder = new StringBuilder("[");
            switch (Ch)
            {
                case '\"': builder.Append("\\\""); break;
                case '\\': builder.Append(@"\\"); break;
                case '\'': builder.Append(@"\'"); break;
                case '\0': builder.Append(@"\0"); break;
                case '\a': builder.Append(@"\a"); break;
                case '\b': builder.Append(@"\b"); break;
                case '\f': builder.Append(@"\f"); break;
                case '\n': builder.Append(@"\n"); break;
                case '\r': builder.Append(@"\r"); break;
                case '\t': builder.Append(@"\t"); break;
                case '\v': builder.Append(@"\v"); break;
                case '^': builder.Append(@"\^"); break;
                case '[': builder.Append(@"\["); break;
                case ']': builder.Append(@"\]"); break;
                default:
                    builder.Append(Ch);
                    break;
            }
            builder.Append(']');
            return builder.ToString();
        }

        public override int GetHashCode() => Ch.GetHashCode();

        public bool IsMatch(char ch) => (ch == Ch);

        public bool Equals(IMatch? other)
        {
            bool result;
            if (other is SingleChar c)
                result = Ch == c.Ch;
            else if (other is CharClass cc)
                result = cc.IsSingleChar(Ch);
            else
                result = false;
            return result;
        }

        public IMatch? Intersect(IMatch other)
        {
            IMatch? result;
            if (other is CharClass cc)
            {
                if (cc.IsMatch(Ch))
                    result = this;
                else
                    result = null;
            }
            else if (other is SingleChar c)
            {
                if (c.Ch == Ch)
                    result = this;
                else
                    result = null;
            }
            else
            {
                result = null;
            }
            return result;
        }

        public IMatch Union(IMatch other)
        {
            IMatch result;
            if (other is SingleChar c)
            {
                if (c.Ch == Ch)
                    result = this;
                else
                    result = new CharClass(this) { Ch, c.Ch };
            }
            else if (other is CharClass cc)
            {
                if (cc.IsMatch(Ch))
                    result = cc;
                else
                    result = new CharClass(cc) { Ch };
            }
            else
            {
                result = this;
            }
            return result;
        }

        public IMatch Reduce() => this;

        public IMatch Invert(Token t)
        {
            var result = new CharClass(t) { Ch };
            return result.Invert(t);
        }

        public IMatch Invert()
        {
            var result = new CharClass(this) { Ch };
            return result.Invert();
        }


        public IMatch Difference(IMatch other) =>
            other.IsMatch(Ch) ? EmptyMatch.Instance as IMatch : this;

        public IEnumerator<char> GetEnumerator()
        {
            yield return Ch;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
