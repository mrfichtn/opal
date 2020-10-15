using Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.Nfa
{
    public class CharClass : Segment, IMatch
    {
        private bool _invert = false;
        private uint[] _matches;
        private const int Size = 2048;

        public CharClass(Segment s)
            : base(s)
        {
            _matches = new uint[Size];
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
            Array.Copy(ch._matches, _matches, _matches.Length);
            _invert = ch._invert;
        }

        public CharClass(string value)
        {
            _matches = new uint[Size];
            Set(value);
        }

        protected CharClass(uint[] matches)
        {
            _matches = matches;
            int bits = GetCount(matches);
            if (bits > (Size >> 1))
            {
                for (int i = 0; i < _matches.Length; i++)
                    _matches[i] = ~_matches[i];
                _invert = true;
            }
        }

        #region Properties
        public int ErrorColumn { get; private set; }

        #region Count Property
        public int Count
        {
            get
            {
                var count = GetCount(_matches);
                return (!_invert) ? count : (1 << 16) - count;
            }
        }

        public int WriteCount
        {
            get
            {
                var count = _invert ? 1 : 0;
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
                _invert = !_invert
            };
            copy.CopyFrom(t);
            return copy;
        }

        public IMatch Invert()
        {
            var copy = new CharClass(this)
            {
                _invert = !_invert
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
                _invert = true;
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

        public void Add(char ch)
        {
            SetBit(ch);
        }

        public void Add(char beg, char end)
        {
            for (int ch = beg; ch <= end; ch++)
                SetBit(ch);
        }

        public void AddTo(CharClass right)
        {
            if (!_invert)
            {
                if (!right._invert)
                    OrFrom(right._matches);
                else
                    NotAndFrom(right._matches);
            }
            else if (!right._invert)
            {
                AndNotFrom(right._matches);
            }
            else
            {
                AndFrom(right._matches);
            }
        }

        public override bool Equals(object obj)
        {
            var rt = obj as CharClass;
            if (Equals(rt, null))
                return false;
            else return (rt._invert == _invert)
                && _matches.Equals(rt._matches);
        }

        public void Write(IGenerator generator, string varName)
        {
            if (_invert)
                generator.Write("!((_ch==-1) ||");

            int start = -1;
            var isFirst = true;
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
                        if (isFirst)
                            isFirst = false;
                        else
                            generator.Write(" || ");
                        generator.Write(varName)
                            .Write(" == ")
                            .WriteCharString((char)start);
                    }
                    else if (start == i - 2)
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            generator.Write(" || ");
                        generator.Write(varName)
                            .Write("==")
                            .WriteCharString((char)start);

                        generator.Write(" || ")
                            .Write(varName)
                            .Write("==")
                            .WriteCharString(i - 1);
                    }
                    else
                    {
                        if (isFirst)
                            isFirst = false;
                        else
                            generator.Write(" || ");

                        generator
                            .Write('(')
                            .Write(varName)
                            .Write(">=")
                            .WriteCharString(start);

                        generator.Write(" && ");
                        generator.Write(varName)
                            .Write("<=")
                            .WriteCharString(i - 1)
                            .Write(')');

                        //AppendTo(builder, (char)start);
                        //if (start < i + 2)
                        //    builder.Append('-');
                        //AppendTo(builder, (char)(i - 1));
                    }

                    start = -1;
                }
            }

            if (_invert)
                generator.Write(")");
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('[');
            if (_invert)
                builder.Append('^');

            int start = -1;
            for (int i = 0; i <= char.MaxValue; i++)
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
            foreach (var item in _matches)
                hash ^= item;
            if (_invert)
                hash = ~hash;
            return (int)hash;
        }

        public static char ConvertEsc(char val)
        {
            return Generators.Strings.ConvertEsc(val);
        }

        public bool IsMatch(char ch)
        {
            return GetBit(ch) ^ _invert;
        }

        #region Logic Members
        private void SetBit(int address)
        {
            var index = address >> 5;
            var offset = address & 0x1F;
            _matches[index] |= (0x1U << offset);
        }

        private void ClrBit(int address)
        {
            var index = address >> 5;
            var offset = address & 0x1F;
            _matches[index] &= ~((0x1U << offset));
        }


        private bool GetBit(int address)
        {
            var index = address >> 5;
            var offset = address & 0x1F;
            return ((_matches[index] >> offset) & 0x1) == 1;
        }


        private void OrFrom(uint[] matches)
        {
            for (var i = 0; i < _matches.Length; i++)
                _matches[i] |= matches[i];
        }
        private void NotAndFrom(uint[] matches)
        {
            for (var i = 0; i < _matches.Length; i++)
            {
                var left = _matches[i];
                var right = matches[i];
                if (left == 0)
                    _matches[i] = right;
                else
                    _matches[i] = right & (~left);
            }
            _invert = true;
        }

        private void AndNotFrom(uint[] matches)
        {
            for (var i = 0; i < _matches.Length; i++)
            {
                var left = _matches[i];
                var right = matches[i];
                if (right == 0)
                    _matches[i] = left;
                else
                    _matches[i] = left & (~right);
            }
        }

        private void AndFrom(uint[] matches)
        {
            for (var i = 0; i < _matches.Length; i++)
                _matches[i] &= matches[i];
        }


        #endregion

        public bool IsSingleChar(char ch)
        {
            var index = ch >> 5;
            var offset = ch & 0x1F;
            if (!_invert)
            {
                uint data = 1U << offset;
                if (_matches[index] != data)
                    return false;

                for (var i = 0; i < index; i++)
                {
                    if (_matches[i] != 0)
                        return false;
                }
                for (var i = index + 1; i < _matches.Length; i++)
                {
                    if (_matches[i] != 0)
                        return false;
                }
            }
            else
            {
                uint data = ~(1U << offset);
                if (_matches[index] != data)
                    return false;

                for (var i = 0; i < index; i++)
                {
                    if (_matches[i] != uint.MaxValue)
                        return false;
                }
                for (var i = index + 1; i < _matches.Length; i++)
                {
                    if (_matches[i] != uint.MaxValue)
                        return false;
                }

            }
            return true;
        }

        public bool Equals(CharClass other)
        {
            if (_invert == other._invert)
            {
                for (int i = 0; i < _matches.Length; i++)
                {
                    if (_matches[i] != other._matches[i])
                        return false;
                }
            }
            else
            {
                for (int i = 0; i < _matches.Length; i++)
                {
                    if (_matches[i] != ~other._matches[i])
                        return false;
                }
            }
            return true;
        }

        public bool Equals(IMatch other)
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
                var data = new uint[Size];
                var hasIntersection = false;
                if (!_invert)
                {
                    if (!cc._invert)
                    {
                        for (int i = 0; i < _matches.Length; i++)
                        {
                            var intersect = _matches[i] & cc._matches[i];
                            data[i] = intersect;
                            if (intersect != 0)
                                hasIntersection = true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _matches.Length; i++)
                        {
                            var intersect = _matches[i] & ~cc._matches[i];
                            data[i] = intersect;
                            if (intersect != 0)
                                hasIntersection = true;
                        }
                    }
                }
                else if (!cc._invert)
                {
                    for (int i = 0; i < _matches.Length; i++)
                    {
                        var intersect = ~_matches[i] & cc._matches[i];
                        data[i] = intersect;
                        if (intersect != 0)
                            hasIntersection = true;
                    }
                }
                else
                {
                    for (int i = 0; i < _matches.Length; i++)
                    {
                        var intersect = _matches[i] | cc._matches[i];
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

        public int GetCount()
        {
            return GetCount(_matches);
        }

        public static int GetCount(uint[] data)
        {
            var count = 0;
            for (var i = 0; i < data.Length; i++)
                count += GetCount(data[i]);
            return count;
        }

        public static int GetCount(uint item)
        {
            item = item - ((item >> 1) & 0x55555555);
            item = (item & 0x33333333) + ((item >> 2) & 0x33333333);
            item = (item + (item >> 4)) & 0x0f0f0f0f;
            item = item + (item >> 8);
            item = item + (item >> 16);
            return (int)(item & 0x3F);
        }

        public bool Subtract(IMatch other)
        {
            if (!_invert)
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
            if (count == 1 && !_invert)
                result = new SingleChar(this.FirstOrDefault());
            else if (count == 0)
                result = EmptyMatch.Instance;
            else
                result = this;
            return result;
        }

        public IMatch Difference(IMatch other)
        {
            var result = new CharClass(this);
            if (!_invert)
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
            if (!result._invert)
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
            _invert = !_invert;
            for (int i = 0; i < _matches.Length; i++)
                _matches[i] = ~_matches[i];
        }

        public IEnumerator<char> GetEnumerator()
        {
            if (!_invert)
            {
                for (int i = 0; i < _matches.Length; i++)
                {
                    var item = _matches[i];
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
                for (int i = 0; i < _matches.Length; i++)
                {
                    var item = _matches[i];
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
