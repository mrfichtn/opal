using System;
using System.Collections.Generic;

namespace Opal.Containers
{
    public struct BitArray
    {
        private readonly uint[] data;
        private readonly int length;
        
        internal BitArray(uint[] data, int length)
        {
            if ((length & 0x1F) != 0)
                throw new ArgumentException(
                    "Length must be divisible by 32", 
                    nameof(length));
            this.data = data;
            this.length = length;
        }
        
        public BitArray(int length)
        {
            if ((length & 0x1F) != 0)
                throw new ArgumentException(
                    "Length must be divisible by 32",
                    nameof(length));
            var words = length >> 5;
            data = new uint[words];
            this.length = length;
        }

        public BitArray(in BitArray copy)
        {
            length = copy.length;
            data = new uint[copy.data.Length];
            Array.Copy(copy.data, data, data.Length);
        }

        public int Length => length;

        public bool this[int address]
        {
            get => GetBit(address);
            set => SetBit(address, value);
        }

        public void SetBit(int address)
        {
            if (address < 0 || address >= length)
                throw new ArgumentOutOfRangeException(nameof(address));
            var index = address >> 5;
            var offset = address & 0x1F;
            data[index] |= (0x1U << offset);
        }

        public void SetBit(int address, bool value)
        {
            if (value)
                SetBit(address);
            else
                ClrBit(address);
        }

        public void ClrBit(int address)
        {
            if (address < 0 || address >= length)
                throw new ArgumentOutOfRangeException(nameof(address));

            var index = address >> 5;
            var offset = address & 0x1F;
            data[index] &= ~((0x1U << offset));
        }

        public bool GetBit(int address)
        {
            if (address < 0 || address >= length)
                throw new ArgumentOutOfRangeException(nameof(address));
            var index = address >> 5;
            var offset = address & 0x1F;
            return ((data[index] >> offset) & 0x1) == 1;
        }

        public void OrFrom(in BitArray bitArray)
        {
            var length = Math.Min(data.Length, bitArray.data.Length);
            for (var i = 0; i < length; i++)
                data[i] |= bitArray.data[i];
        }

        public void OrNotFrom(in BitArray bitArray)
        {
            var length = Math.Min(data.Length, bitArray.data.Length);
            for (var i = 0; i < length; i++)
                data[i] |= ~bitArray.data[i];
        }

        public void AndNotFrom(in BitArray bitArray)
        {
            var length = Math.Min(data.Length, bitArray.data.Length);
            for (var i = 0; i < length; i++)
                data[i] &= ~bitArray.data[i];
        }

        public void AndFrom(in BitArray bitArray)
        {
            var length = Math.Min(data.Length, bitArray.data.Length);
            for (var i = 0; i < length; i++)
                data[i] &= bitArray.data[i];
        }

        public int BitCount() => BitCount(data);

        public void Invert()
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = ~data[i];
        }

        public static BitArray operator~(BitArray array)
        {
            var data = new uint[array.data.Length];
            for (var i = 0; i < array.data.Length; i++)
                data[i] = ~array.data[i];
            
            return new BitArray(data, array.length);
        }

        public void SetAll()
        {
            for (var i = 0; i < data.Length; i++)
                data[i] = uint.MaxValue;
        }

        public IEnumerable<int> SetAddresses
        {
            get
            {
                var count = length;
                for (int i = 0; i < data.Length; i++)
                {
                    var item = data[i];
                    if (item != 0)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            if (((item >> j) & 0x1) != 0)
                                yield return ((i << 5) + j);
                            if (--count == 0)
                                yield break;
                        }
                    }
                }
            }
        }

        public IEnumerable<int> ClearedAddresses
        {
            get
            {
                var count = length;
                for (int i = 0; i < data.Length; i++)
                {
                    var item = data[i];
                    if (item != (~0U))
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            if (((item >> j) & 0x1) == 0x0U)
                                yield return ((i << 5) + j);
                            if (--count == 0)
                                yield break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of bits set to 1 (true)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int BitCount(uint[] data)
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

        public override int GetHashCode()
        {
            var hash = 0U;
            foreach (var item in data)
                hash ^= item;
            return (int) hash;
        }

        public bool Equals(in BitArray bitArray)
        {
            if (length != bitArray.length)
                return false;
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] != bitArray.data[i])
                    return false;
            }
            return true;
        }

        public bool IsInverseOf(in BitArray bitArray)
        {
            if (length != bitArray.length)
                return false;

            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] != (~bitArray.data[i]))
                    return false;
            }

            return true;
        }
    }
}
