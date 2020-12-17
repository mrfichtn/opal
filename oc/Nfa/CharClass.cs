using Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Opal.Containers;

namespace Opal.Nfa
{
    public class CharClass : Segment, IMatch
    {
        private const int size = 2048;

        private bool invert;
        private readonly uint[] matches;

        public CharClass()
        {
            matches = new uint[size];
        }

        public CharClass(Segment s)
            : base(s)
        {
            matches = new uint[size];
        }

        public CharClass(Token t)
            : this(t as Segment)
        {
            if (!string.IsNullOrEmpty(t.Value))
                Set(t.Value);
        }

        public CharClass(CharClass ch)
            : this(ch as Segment)
        {
            Array.Copy(ch.matches, matches, matches.Length);
            invert = ch.invert;
        }

        public CharClass(string value)
        {
            matches = new uint[size];
            Set(value);
        }

        protected CharClass(uint[] matches)
        {
            this.matches = matches;
            int bits = GetCount(matches);
            if (bits > (size >> 1))
            {
                for (int i = 0; i < this.matches.Length; i++)
                    this.matches[i] = ~this.matches[i];
                invert = true;
            }
        }

        #region Properties
        public int ErrorColumn { get; private set; }

        #region Count Property
        public int Count
        {
            get
            {
                var count = GetCount(matches);
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
                    if (GetBit(i))
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

        public void Add(char ch) => SetBit(ch);

        public void Add(char beg, char end)
        {
            for (int ch = beg; ch <= end; ch++)
                SetBit(ch);
        }

        public void AddTo(CharClass right)
        {
            if (!invert)
            {
                if (!right.invert)
                    OrFrom(right.matches);
                else
                    NotOrFrom(right.matches);
            }
            else if (!right.invert)
            {
                AndNotFrom(right.matches);
            }
            else
            {
                AndFrom(right.matches);
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
                    ClrBit(sc.Ch);
                else
                    SetBit(sc.Ch);
            }
            else if (match is AllMatch)
            {
                for (var i = 0; i < matches.Length; i++)
                    matches[i] = uint.MaxValue;
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
                if (GetBit(i))
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
                if (GetBit(i))
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
            var hash = 0U;
            foreach (var item in matches)
                hash ^= item;
            if (invert)
                hash = ~hash;
            return (int)hash;
        }

        public static char ConvertEsc(char val) => 
            Containers.Strings.ConvertEsc(val);

        public bool IsMatch(char ch) => GetBit(ch) ^ invert;

        #region Logic Members
        private void SetBit(int address)
        {
            var index = address >> 5;
            var offset = address & 0x1F;
            matches[index] |= (0x1U << offset);
        }

        private void ClrBit(int address)
        {
            var index = address >> 5;
            var offset = address & 0x1F;
            matches[index] &= ~((0x1U << offset));
        }


        private bool GetBit(int address)
        {
            var index = address >> 5;
            var offset = address & 0x1F;
            return ((matches[index] >> offset) & 0x1) == 1;
        }


        private void OrFrom(uint[] matches)
        {
            for (var i = 0; i < this.matches.Length; i++)
                this.matches[i] |= matches[i];
        }
        private void NotOrFrom(uint[] matches)
        {
            for (var i = 0; i < this.matches.Length; i++)
                this.matches[i] |= ~matches[i];
        }

        private void AndNotFrom(uint[] matches)
        {
            for (var i = 0; i < this.matches.Length; i++)
            {
                var left = this.matches[i];
                var right = matches[i];
                if (right == 0)
                    this.matches[i] = left;
                else
                    this.matches[i] = left & (~right);
            }
        }

        private void AndFrom(uint[] matches)
        {
            for (var i = 0; i < this.matches.Length; i++)
                this.matches[i] &= matches[i];
        }


        #endregion

        public bool IsSingleChar(char ch)
        {
            var index = ch >> 5;
            var offset = ch & 0x1F;
            if (!invert)
            {
                uint data = 1U << offset;
                if (matches[index] != data)
                    return false;

                for (var i = 0; i < index; i++)
                {
                    if (matches[i] != 0)
                        return false;
                }
                for (var i = index + 1; i < matches.Length; i++)
                {
                    if (matches[i] != 0)
                        return false;
                }
            }
            else
            {
                uint data = ~(1U << offset);
                if (matches[index] != data)
                    return false;

                for (var i = 0; i < index; i++)
                {
                    if (matches[i] != uint.MaxValue)
                        return false;
                }
                for (var i = index + 1; i < matches.Length; i++)
                {
                    if (matches[i] != uint.MaxValue)
                        return false;
                }

            }
            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj is CharClass cc)
                return Equals(cc);
            if (obj is SingleChar sc)
                return IsSingleChar(sc.Ch);
            return false;
        }

        public bool Equals(CharClass other)
        {
            if (invert == other.invert)
            {
                for (int i = 0; i < matches.Length; i++)
                {
                    if (matches[i] != other.matches[i])
                        return false;
                }
            }
            else
            {
                for (int i = 0; i < matches.Length; i++)
                {
                    if (matches[i] != ~other.matches[i])
                        return false;
                }
            }
            return true;
        }

        public bool Equals(IMatch? other)
        {
            bool result;
            if (other is CharClass cc)
                result = Equals(cc);
            else if (other is SingleChar c)
                result = IsSingleChar(c.Ch);
            else
                result = false;
            return result;
        }

        public IMatch? Intersect(IMatch other)
        {
            IMatch? result = null;
            if (other is CharClass cc)
            {
                var data = new uint[size];
                var hasIntersection = false;
                if (!invert)
                {
                    if (!cc.invert)
                    {
                        for (int i = 0; i < matches.Length; i++)
                        {
                            var intersect = matches[i] & cc.matches[i];
                            data[i] = intersect;
                            if (intersect != 0)
                                hasIntersection = true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < matches.Length; i++)
                        {
                            var intersect = matches[i] & ~cc.matches[i];
                            data[i] = intersect;
                            if (intersect != 0)
                                hasIntersection = true;
                        }
                    }
                }
                else if (!cc.invert)
                {
                    for (int i = 0; i < matches.Length; i++)
                    {
                        var intersect = ~matches[i] & cc.matches[i];
                        data[i] = intersect;
                        if (intersect != 0)
                            hasIntersection = true;
                    }
                }
                else
                {
                    for (int i = 0; i < matches.Length; i++)
                    {
                        var intersect = matches[i] | cc.matches[i];
                        data[i] = ~intersect;
                        if (intersect != 0)
                            hasIntersection = true;
                    }
                }
                if (hasIntersection)
                {
                    var charClass = new CharClass(data);
                    result = charClass.Reduce();
                }
            }
            else if (other is SingleChar c)
            {
                if (IsMatch(c.Ch))
                    result = c;
                else
                    result = null;
            }
            return result;
        }

        public IMatch Union(IMatch other)
        {
            var result = new CharClass(this);
            foreach (var ch in other)
                result.Add(ch);
            return result;
        }

        public int GetCount() => GetCount(matches);

        public static int GetCount(uint[] data)
        {
            var count = 0;
            for (var i = 0; i < data.Length; i++)
                count += BitCount(data[i]);
            return count;
        }

        public static int BitCount(uint item)
        {
            item -= ((item >> 1) & 0x55555555);
            item = (item & 0x33333333) + ((item >> 2) & 0x33333333);
            item = (item + (item >> 4)) & 0x0f0f0f0f;
            item += (item >> 8);
            item += (item >> 16);
            return (int)(item & 0x3F);
        }

        public bool Subtract(IMatch other)
        {
            if (!invert)
            {
                foreach (var ch in other)
                    ClrBit(ch);
            }
            else
            {
                foreach (var ch in other)
                    SetBit(ch);
            }
            Normalize();
            return Count == 0;
        }

        public IMatch Reduce()
        {
            IMatch result;
            var count = Normalize();
            if (count == 1 && !invert)
                result = new SingleChar(this.FirstOrDefault());
            else if (count == 0)
                result = EmptyMatch.Instance;
            else if (count == char.MaxValue + 1)
                result = new AllMatch();
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
                    result.ClrBit(ch);
            }
            else
            {
                foreach (var ch in other)
                    result.SetBit(ch);
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
            for (int i = 0; i < matches.Length; i++)
                matches[i] = ~matches[i];
        }

        public IEnumerator<char> GetEnumerator()
        {
            if (!invert)
            {
                for (int i = 0; i < matches.Length; i++)
                {
                    var item = matches[i];
                    if (item != 0)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            if (((item >> j) & 0x1) != 0)
                                yield return (char)((i << 5) + j);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < matches.Length; i++)
                {
                    var item = matches[i];
                    if (item != (~0U))
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            if (((item >> j) & 0x1) == 0x0U)
                                yield return (char)((i << 5) + j);
                        }
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
