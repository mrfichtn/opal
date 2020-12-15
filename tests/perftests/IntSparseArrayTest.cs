using System;

namespace perftests
{
    public class IntSparseArrayTest: TestBase
    {
		private readonly int[] data;
		private readonly int start;
		
		public IntSparseArrayTest(int[] data)
            : base("Int sparse array")
        {
			for (var i = 0; i < data.Length; i++)
            {
				if (data[i] != 0)
                {
					start = i;
					break;
                }
            }

			for (var i = data.Length-1; i >= start; i--)
            {
				if (data[i] != 0)
                {
					this.data = new int[i - start + 1];
					Array.Copy(data, start, this.data, 0, this.data.Length);
					break;
                }
            }
			if (this.data == null)
				this.data = Array.Empty<int>();
		}

        protected override long Test(string source)
        {
			long checksum = 0;
			foreach (var item in source)
				checksum += GetClass(item);
			return checksum;
        }

		protected int GetClass(char ch)
		{
			if (ch > 0)
			{ 
				var index = ch - start;
				return (index >= 0 && index < data.Length) ?
					data[index] :
					0;
            }
			else
            {
				return -1;
            }
		}
	}
}
