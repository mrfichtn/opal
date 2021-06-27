using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Opal
{
    public class FileBuffer : IBuffer
    {
        private readonly ReaderBase reader;
        //private readonly StreamReader reader;
        private readonly StringBuilder builder;
        private readonly List<long> lineStarts;
        private int filePos;
        private int remaining;

        public FileBuffer(Stream stream, EncodingType encoding = EncodingType.Auto)
        {
            builder = new StringBuilder();
            reader = CreateReader(stream, encoding);
            lineStarts = new List<long>
            {
                0,
                reader.Position
            };
        }


        public FileBuffer(string filePath, EncodingType encoding = EncodingType.Auto)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                  encoding)
        { }

        public int Position
        {
            get => filePos - remaining;
        }

        public void Dispose()
        {
            reader.Dispose();
            GC.SuppressFinalize(this);
        }

        public string GetToken(int length)
        {
            var result = builder.ToString(0, length);
            builder.Remove(0, length);
            if (length > remaining)
            {
                filePos += length - remaining;
                remaining = 0;
            }
            else
            {
                remaining -= length;
            }
            return result;
        }

        public string Line(Position position)
        {
            if (position.Ln <= 0 || position.Ln >= lineStarts.Count)
                return string.Empty;

            var oldPosition = reader.Position;
            reader.Position = lineStarts[position.Ln];
            try
            {
                return ReadLine();
            }
            finally
            {
                reader.Position = oldPosition;
            }
        }

        public string PeekLine()
        {
            var oldPosition = reader.Position;
            try
            {
                return ReadLine();
            }
            finally
            {
                reader.Position = oldPosition;
            }
        }

        private string ReadLine()
        {
            var lineBuilder = new StringBuilder();

            while (true)
            {
                var ch = reader.Read();
                if (ch == ReaderBase.Eof || ch == '\n')
                    break;
                lineBuilder.Append((char)ch);
            }
            if ((lineBuilder.Length > 0) && (lineBuilder[lineBuilder.Length-1] == '\r'))
                lineBuilder.Length--;
            return lineBuilder.ToString();
        }

        public int Read()
        {
            int result;
            if (remaining > 0)
            {
                result = builder[builder.Length - remaining--];
            }
            else
            {
                result = reader.Read();
                if (result != ReaderBase.Eof)
                {
                    builder.Append((char)result);
                    filePos++;
                }
            }
            if (result == '\n')
                lineStarts.Add(reader.Position);
            return result;

        }

        private static ReaderBase CreateReader(Stream stream, EncodingType encoding)
        {
            ReaderBase? result;
            switch (encoding)
            {
                case EncodingType.Ansi: result = new AnsiReader(stream); break;
                case EncodingType.Utf8: result = new Utf8Reader(stream); break;
                case EncodingType.Utf16BigEndian: result = new Utf16BigEndian(stream); break;
                case EncodingType.Utf16LittleEndian: result = new Utf16LittleEndian(stream); break;
                default:
                    result = null;
                    break;
            }
            if (result != null)
            {
                result.SkipBOM();
                return result;
            }

            var length = stream.Length;

            if (length < 2)
                return new AnsiReader(stream);

            var oldPos = stream.Position;
            var b0 = stream.ReadByte();

            //UTF8                  EF BB BF
            //UTF-16 big-endian     FE FF
            //UTF-16 little-endian  FF FE
            //UTF-32 big-endian     00 00 FE FF
            //UTF-32 little-endian  FF FE 00 00
            if (b0 == 0xEF)
            {
                var b1 = stream.ReadByte();
                if (b1 == 0xBB)
                {
                    var b2 = stream.ReadByte();
                    if (b2 == 0xBF)
                        return new Utf8Reader(stream);
                }
            }
            else if (b0 == 0xFE)
            {
                var b1 = stream.ReadByte();
                if (b1 == 0xFF)
                    return new Utf16BigEndian(stream);
            }
            else if (b0 == 0xFF)
            {
                var b1 = stream.ReadByte();
                if (b1 == 0xFE)
                    return new Utf16LittleEndian(stream);
            }
            stream.Position = oldPos;
            return new AnsiReader(stream);
        }
    }

}
