using System.IO;
using System.IO.Compression;

namespace Opal
{
    public static class StreamExt
    {
		public static int Read8(this Stream stream) => (sbyte)stream.ReadByte();

		public static int Read16(this Stream stream)
		{
			var b1 = stream.ReadByte();
			var b2 = stream.ReadByte();
			return (short)(b1 | (b2 << 8));
		}

		public static int Read24(this Stream stream)
		{
			var b1 = stream.ReadByte();
			var b2 = stream.ReadByte();
			var b3 = stream.ReadByte();
			return b1 | (b2 << 8) | (b3 << 16);
		}

		public static int Read32(this Stream stream)
		{
			var b1 = stream.ReadByte();
			var b2 = stream.ReadByte();
			var b3 = stream.ReadByte();
			var b4 = stream.ReadByte();
			return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24);
		}

		public static void DecompressFrom(this Stream outStream, byte[] data)
		{
            using var inStream = new MemoryStream(data);
            var s = new GZipStream(inStream, CompressionMode.Decompress, false);
            s.CopyTo(outStream);
        }
	}
}
