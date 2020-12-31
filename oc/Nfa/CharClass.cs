using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Opal.Containers;


using BitArray = Opal.Containers.BitArray;

namespace Opal.Nfa
{
    public class CharClass : Segment, IMatch
    {
        private const int MatchArraySize = 1 << 16;

        private bool invert;
        private readonly BitArray matches;

        public CharClass()
        {
            matches = new BitArray(MatchArraySize);
        }

        public CharClass(Segment s)
            : base(s)
        {
            matches = new BitArray(MatchArraySize);
        }

        public CharClass(Token t)
            : this(t as Segment)
        {
            if (!string.IsNullOrEmpty(t.Value))
                Set(t.Value);
        }

        public CharClass(CharClass ch)
            : base(ch)
        {
            matches = new BitArray(ch.matches);
            invert = ch.invert;
        }

        public CharClass(string value)
        {
            matches = new BitArray(MatchArraySize);
            Set(value);
        }

        protected CharClass(BitArray bitArray)
        {
            matches = bitArray;
            if (bitArray.BitCount() > (MatchArraySize >> 1))
                Flip();
        }

        #region Properties
        public int ErrorColumn { get; private set; }

        #region Count Property
        public int Count
        {
            get
            {
                var count = matches.BitCount();
                return (!invert) ? count : (1 << 16) - count;
            }
        }

        public int WriteCount
        {
            get
            {
                var count = invert ? 1 : 0;
                int start = -1;
                for (var i = 0; i <= char.MaxValue; i++)
                {
                    if (matches.GetBit(i))
                    {
                        if (start == -1)
                            start = i;
                    }
                    else
                    {
                        if (start == -1)
                        {
                            continue;
                        }
                        else if (start == i - 1)
                        {
                            count++;
                        }
                        else
                        {
                            count += 2;
                        }

                        start = -1;
                    }
                }
                return count;
            }
        }

        #endregion

        #endregion

        public IMatch Invert(Token t)
        {
            var copy = new CharClass(this)
            {
                invert = !invert
            };
            copy.CopyFrom(t);
            return copy;
        }

        public IMatch Invert()
        {
            var copy = new CharClass(this)
            {
                invert = !invert
            };
            return copy;
        }

        public void Set(string value)
        {
            if (value.Length < 1)
                return;
            int i = 1;
            if (value[i] == '^')
            {
                i++;
                invert = true;
            }

            var esc = false;
            var range = false;
            int lastCh = -1;

            for (; i < value.Length - 1; i++)
            {
                var ch = value[i];
                if (esc)
                {
                    esc = false;
                    switch (ch)
                    {
                        case 'a': ch = '\a'; break;
                        case 'b': ch = '\b'; break;
                        case 'd':
                            Add('0', '9');
                            ch = char.MaxValue;
                            break;
                        case 'f': ch = '\f'; break;
                        case 'n': ch = '\n'; break;
                        case 'r': ch = '\r'; break;
                        case 't': ch = '\t'; break;
                        case 'w':
                            Add(' ');
                            Add('\t');
                            Add('\n');
                            Add('\f');
                            Add('\r');
                            ch = char.MaxValue;
                            break;
                        case '0': ch = '\0'; break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (ch == '\\')
                    {
                        esc = true;
                        continue;
                    }
                    else if (ch == '-' && (lastCh != -1))
                    {
                        range = true;
                        continue;
                    }
                }
                if (range)
                {
                    Add((char)lastCh, ch);
                    lastCh = -1;
                    range = false;
                }
                else
                {
                    if (lastCh != -1)
                        Add((char)lastCh);
                    lastCh = ch;
                }
            }

            if (lastCh != -1)
            {
                if (range)
                {
                    Add((char)lastCh);
                    Add('-');
                }
                else if (esc)
                {
                    Add('\\');
                }
                else
                {
                    Add((char)lastCh);
                }
            }
        }

        public void AddEsc(char ch)
        {
            switch (ch)
            {
                case 'a': Add('\a'); break;
                case 'b': Add('\b'); break;
                case 'c': Add('\b'); break;
                case 'd': Add('0', '9'); break;
                case 'f': Add('\f'); break;
                case 'n': Add('\n'); break;
                case 'r': Add('\r'); break;
                case 't': Add('\t'); break;
                case 'w': Add(' '); Add('\t'); Add('\n'); Add('\f'); Add('\r'); break;
                case '0': Add('\0'); break;
                default:
                    Add(ch);
                    break;
            }
        }

        public void Add(char ch) => matches.SetBit(ch);

        public void Add(char beg, char end)
        {
            for (int ch = beg; ch <= end; ch++)
                matches.SetBit(ch);
        }

        public void AddTo(CharClass right)
        {
            if (!invert)
            {
                if (!right.invert)
                    matches.OrFrom(right.matches);
                else
                    matches.OrNotFrom(right.matches);
            }
            else if (!right.invert)
            {
                matches.AndNotFrom(right.matches);
            }
            else
            {
                matches.AndFrom(right.matches);
            }
        }

        public void AddTo(IMatch match)
        {
            if (match is CharClass cc)
            {
                AddTo(cc);
            }
            else if (match is SingleChar sc)
            {
                if (invert)
                    matches.ClrBit(sc.Ch);
                else
                    matches.SetBit(sc.Ch);
            }
            else if (match is AllMatch)
            {
                matches.SetAll();
            }
        }

        public string SwitchCondition(string varName)
        {
            var builder = new StringBuilder();
            if (invert)
                builder.Append($"!(");

            int start = -1;
            var isFirst = true;
            int i;
            for (i = 0; i <= char.MaxValue; i++)
            {
                if (matches.GetBit(i))
                {
                    if (start == -1)
                        start = i;
                }
                else
                {
                    if (start == -1)
                    {
                        continue;
                    }
                    else if (start == i - 1)
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            builder.Append(" || ");
                        builder.Append(varName)
                            .Append(" == ")
                            .AppendEscString((char)start);
                    }
                    else if (start == i - 2)
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            builder.Append(" || ");

                        builder .Append(varName)
                                .Append("==")
                                .AppendEscString((char)start);

                        builder.Append(" || ")
                            .Append(varName)
                            .Append("==")
                            .AppendEscString((char)(i - 1));
                    }
                    else
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            builder.Append(" || ");

                        builder
                            .Append('(')
                            .Append(varName)
                            .Append(">=")
                            .AppendEscString((char) start);

                        builder.Append(" && ");
                        builder.Append(varName)
                            .Append("<=")
                            .AppendEscString((char)(i - 1))
                            .Append(')');
                    }

                    start = -1;
                }
            }
            if (start != -1)
            {
                if (start == i - 1)
                {
                    if (!isFirst)
                        builder.Append(" || ");
                    builder.Append(varName)
                        .Append(" == ")
                        .AppendEscString((char)start);
                }
                else if (start == i - 2)
                {
                    if (!isFirst)
                        builder.Append(" || ");

                    builder.Append(varName)
                            .Append("==")
                            .AppendEscString((char)start);

                    builder.Append(" || ")
                        .Append(varName)
                        .Append("==")
                        .AppendEscString((char)(i - 1));
                }
                else
                {
                    if (!isFirst)
                        builder.Append(" || ");

                    builder
                        .Append('(')
                        .Append(varName)
                        .Append(">=")
                        .AppendEscString((char)start);

                    builder.Append(" && ");
                    builder.Append(varName)
                        .Append("<=")
                        .AppendEscString((char)(i - 1))
                        .Append(')');
                }
            }

            if (invert)
                builder.Append(')');

            return builder.ToString();
        }


        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('[');
            if (invert)
                builder.Append('^');

            int start = -1;
            int i;
            for (i = 0; i <= char.MaxValue; i++)
            {
                if (matches.GetBit(i))
                {
                    if (start == -1)
                        start = i;
                }
                else
                {
                    if (start == -1)
                    {
                        continue;
                    }
                    else if (start == i - 1)
                    {
                        AppendTo(builder, (char)start);
                    }
                    else if (start == i - 2)
                    {
                        AppendTo(builder, (char)start);
                        AppendTo(builder, (char)(i - 1));
                    }
                    else
                    {
                        AppendTo(builder, (char)start);
                        if (start < i + 2)
                            builder.Append('-');
                        AppendTo(builder, (char)(i - 1));
                    }
                    start = -1;
                }
            }
            if (start != -1)
            {
                if (start == i - 1)
                {
                    AppendTo(builder, (char)start);
                }
                else if (start == i - 2)
                {
                    AppendTo(builder, (char)start);
                    AppendTo(builder, (char)(i - 1));
                }
                else
                {
                    AppendTo(builder, (char)start);
                    if (start < i + 2)
                        builder.Append('-');
                    AppendTo(builder, (char)(i - 1));
                }
            }

            
            if (invert && builder.Length == 2)
            {
                return "Ø";
            }
            else
                builder.Append(']');
            return builder.ToString();
        }

        private static StringBuilder AppendTo(StringBuilder builder, char ch)
        {
            switch (ch)
            {
                case '\0': builder.Append("\\0"); break;
                case '\t': builder.Append("\\t"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\\': builder.Append("\\\\"); break;
                case '[': builder.Append("\\["); break;
                case ']': builder.Append("\\["); break;
                case '^': builder.Append("\\^"); break;
                case '-': builder.Append("\\-"); break;
                default:
                    if ((ch >= 1 && ch <= 31) || (ch >= 128))
                        builder.AppendFormat("\\u{0:x4}", (int)ch);
                    else
                        builder.Append(ch);
                    break;
            }

            return builder;
        }

        public override int GetHashCode()
        {
            var hash = matches.GetHashCode();
            if (invert)
                hash = ~hash;
            return (int)hash;
        }

        public static char ConvertEsc(char val) => 
            ConvertEsc(val);

        public bool IsMatch(char ch) => matches.GetBit(ch) ^ invert;

        public bool IsSingleChar(char ch)
        {
            var bitCount = matches.BitCount();
            if (invert)
                return (bitCount == MatchArraySize - 1) && !matches.GetBit(ch);
            else
                return (bitCount == 1) && matches.GetBit(ch);
        }

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                CharClass cc => Equals(cc),
                SingleChar sc => IsSingleChar(sc.Ch),
                _ => false
            };
        }

        public bool Equals(CharClass other)
        {
            return (invert == other.invert) ?
                matches.Equals(other.matches) :
                matches.IsInverseOf(other.matches);
        }

        public bool Equals(IMatch? other)
        {
            return other switch
            {
                CharClass cc => Equals(cc),
                SingleChar sc => IsSingleChar(sc.Ch),
                _ => false
            };
        }

        public IMatch? Intersect(IMatch other)
        {
            return other switch
            {
                CharClass cc => Intersect(cc),
                SingleChar sc => Intersect(sc),
                EmptyMatch => null,
                _ => this
            };
        }

        public IMatch? Intersect(SingleChar c) =>
            IsMatch(c.Ch) ? c : null;

        public IMatch? Intersect(CharClass other)
        {
            var bitArray = !invert ?
                new BitArray(matches) :
                ~matches;
            if (!other.invert)
                bitArray.AndFrom(other.matches);
            else
                bitArray.AndNotFrom(other.matches);
            var count = bitArray.BitCount();
            if (count == 0)
                return null;
            else if (count == 1)
                return new SingleChar((char)bitArray.SetAddresses.Single());
            else
                return new CharClass(bitArray);
        }

        public IMatch Union(IMatch other)
        {
            var result = new CharClass(this);
            foreach (var ch in other)
                result.Add(ch);
            return result;
        }

        public int GetCount() => matches.BitCount();

        public bool Subtract(IMatch other)
        {
            if (!invert)
            {
                foreach (var ch in other)
                    matches.ClrBit(ch);
            }
            else
            {
                foreach (var ch in other)
                    matches.SetBit(ch);
            }
            Normalize();
            return Count == 0;
        }

        public IMatch Reduce()
        {
            IMatch result;
            var count = Normalize();
            if ((count == 1) && !invert)
                result = new SingleChar(this.FirstOrDefault());
            else if (count == 0)
                result = invert ? new AllMatch() : EmptyMatch.Instance;
            else
                result = this;
            return result;
        }

        public IMatch Difference(IMatch other)
        {
            var result = new CharClass(this);
            if (!invert)
            {
                foreach (var ch in other)
                    result.matches.ClrBit(ch);
            }
            else
            {
                foreach (var ch in other)
                    result.matches.SetBit(ch);
            }

            var count = result.Normalize();
            if (!result.invert)
            {
                if (count == 0)
                    return EmptyMatch.Instance;
                else if (count == 1)
                    return new SingleChar(result.FirstOrDefault());
            }
            return result;
        }

        private int Normalize()
        {
            var count = GetCount();
            if (count > (char.MaxValue >> 1))
            {
                Flip();
                count = 1 + char.MaxValue - count;
            }
            return count;
        }

        private void Flip()
        {
            invert = !invert;
            matches.Invert();
        }

        public IEnumerator<char> GetEnumerator()
        {
            var e = (!invert) ?
                matches.SetAddresses :
                matches.ClearedAddresses;
            return e.Select(x => (char)x)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
