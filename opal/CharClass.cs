using System;

namespace Opal
{
    public static class CharClasses
    {
		public static int[] Decompress(Reader reader)
		{
			var charToClass = new int[char.MaxValue + 1];
			using (reader)
            {
				for (var i = 0; i < charToClass.Length; i++)
					charToClass[i] = reader.Read() + 1;
			}
			return charToClass;
		}

		public static int[] Decompress8(byte[] compressedData) =>
			Decompress(new Reader8(compressedData));

		public static int[] Decompress16(byte[] compressedData) =>
			Decompress(new Reader16(compressedData));

		public static int[] Decompress24(byte[] compressedData) =>
			Decompress(new Reader24(compressedData));

		public static int[] Decompress32(byte[] compressedData) =>
			Decompress(new Reader32(compressedData));

		public static int[] ToArray((char ch, int cls)[] sparseArray)
        {
			var result = new int[char.MaxValue];
			for (var i = 0; i < char.MaxValue; i++)
				result[i] = 1;
			foreach (var item in sparseArray)
				result[item.ch] = item.cls + 1;
			return result;
        }
	}
}
