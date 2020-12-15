using System;

namespace perftests
{
    public class ByteArrayTest: TestBase
    {
        private readonly byte[] data;
        public ByteArrayTest(int[] data)
            : base("Byte Array")
        {
            this.data = Array.ConvertAll(data, x => (byte)x);
        }

        protected override long Test(string source)
        {
            long checksum = 0;
            foreach (var item in source)
                checksum += GetClass(item);
            return checksum;
        }

        protected int GetClass(char ch) =>
            (ch >= 0) ? data[ch] : -1;
    }
}
