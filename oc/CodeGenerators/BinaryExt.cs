using System.IO;
using System.IO.Compression;

namespace Generators
{
    public static class BinaryExt
    {
        public static void WriteCompressedArray(this Generator language, int size, int[] array)
        {
            var outStream = CompressArray(size, array);
            WriteArray(language, outStream);
        }

        public static void WriteCompressedArray(this Generator language, int[,] array)
        {
            var size = 0;
            foreach (var a in array)
            {
                if (a > size)
                    size = a;
            }
            var outStream = CompressArray(size, array);
            WriteArray(language, outStream);
        }

        public static MemoryStream CompressArray(int size, int[] array)
        {
            var outStream = new MemoryStream();

            using (var s = new GZipStream(outStream, CompressionLevel.Optimal, true))
            {
                var inStream = new MemoryStream();

                if (size <= ushort.MaxValue)
                {
                    foreach (var cls in array)
                        inStream.Write8(cls);
                }
                else if (size <= ushort.MaxValue)
                {
                    foreach (var cls in array)
                        inStream.Write16(cls);
                }
                else if (size <= 16777215)
                {
                    foreach (var cls in array)
                        inStream.Write24(cls);
                }
                else
                {
                    foreach (var cls in array)
                        inStream.Write32(cls);
                }

                inStream.Seek(0, SeekOrigin.Begin);
                inStream.CopyTo(s);
            }
            outStream.Seek(0, SeekOrigin.Begin);
            return outStream;
        }

        public static MemoryStream CompressArray(int size, int[,] array)
        {
            var outStream = new MemoryStream();
            //outStream.WriteByte((byte)size);
            //outStream.Write32(array.GetLength(0));
            //outStream.Write32(array.GetLength(1));

            using (var s = new GZipStream(outStream, CompressionLevel.Optimal, true))
            {
                var inStream = new MemoryStream();

                if (size <= ushort.MaxValue)
                {
                    foreach (var cls in array)
                        inStream.Write8(cls);
                }
                else if (size <= ushort.MaxValue)
                {
                    foreach (var cls in array)
                        inStream.Write16(cls);
                }
                else if (size <= 16777215)
                {
                    foreach (var cls in array)
                        inStream.Write24(cls);
                }
                else
                {
                    foreach (var cls in array)
                        inStream.Write32(cls);
                }

                inStream.Seek(0, SeekOrigin.Begin);
                inStream.CopyTo(s);
            }
            outStream.Seek(0, SeekOrigin.Begin);
            return outStream;
        }

        public static void WriteArray(this Generator generator, Stream outStream)
        {
            var max = 16;
            generator.StartBlock();

            var item = 1;
            for (var i = 1; true; )
            {
                var b = outStream.ReadByte();
                if (b == -1)
                    break;
                generator.Write("0x{0:X2}", b);
                if (i + 1 < max)
                    generator.Write(", ");
                if (item == 16)
                {
                    generator.WriteLine();
                    item = 1;
                }
                else
                {
                    item++;
                }
            }
            generator.WriteLine();
            generator.EndBlock(";");
        }

        private static void Write8(this Stream stream, int cls)
        {
            stream.WriteByte((byte)(cls & 0xFF));
        }

        private static void Write16(this Stream stream, int cls)
        {
            stream.WriteByte((byte)(cls & 0xFF));
            stream.WriteByte((byte)((cls >> 8) & 0xFF));
        }

        private static void Write24(this Stream stream, int cls)
        {
            stream.WriteByte((byte)(cls & 0xFF));
            stream.WriteByte((byte)((cls >> 8) & 0xFF));
            stream.WriteByte((byte)((cls >> 16) & 0xFF));
        }

        private static void Write32(this Stream stream, int cls)
        {
            stream.WriteByte((byte)(cls & 0xFF));
            stream.WriteByte((byte)((cls >> 8) & 0xFF));
            stream.WriteByte((byte)((cls >> 16) & 0xFF));
            stream.WriteByte((byte)((cls >> 24) & 0xFF));
        }

        public static bool Read8(this Stream stream, out int result)
        {
            result = stream.ReadByte();
            return result != -1;
        }

        public static bool Read16(this Stream stream, out int value)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();

            bool result;
            if (b2 != -1)
            {
                result = false;
                value = 0;
            }
            else
            {
                value = b1 | (b2 << 8);
                result = true;
            }
            return result;
        }

        public static bool Read24(this Stream stream, out int value)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var b3 = stream.ReadByte();

            bool result;
            if (b3 != -1)
            {
                result = false;
                value = 0;
            }
            else
            {
                value = b1 | (b2 << 8) | (b3 << 16);
                result = true;
            }
            return result;
        }

        public static bool Read32(this Stream stream, out int value)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var b3 = stream.ReadByte();
            var b4 = stream.ReadByte();

            bool result;
            if (b4 != -1)
            {
                result = false;
                value = 0;
            }
            else
            {
                value = b1 | (b2 << 8) | (b3 << 16) | (b4 << 24);
                result = true;
            }
            return result;
        }
    }
}
