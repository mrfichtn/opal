using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perftests
{
    public class SparseArrayTest: TestBase
    {
        private readonly Dictionary<char, byte> data;

        public SparseArrayTest(int[] data)
            : base("Sparse array (dictionary)")
        {
            this.data = new Dictionary<char, byte>();
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] > 0)
                    this.data[(char)i] = (byte)data[i];
            }
        }

        protected override long Test(string source)
        {
            long checksum = 0;
            foreach (var item in source)
                checksum += GetClass(item);
            return checksum;
        }

        public int GetClass(char ch)
        {
            if (ch >= 0)
            {
                return (data.TryGetValue(ch, out byte value)) ?
                    value : 0;
            }
            else
                return -1;
        }
    }
}
