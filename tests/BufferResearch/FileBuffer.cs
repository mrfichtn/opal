using System;
using System.IO;
using System.Text;

namespace Opal
{
    public class FileBuffer : IDisposable
	{
		private readonly StreamReader reader;
		private readonly StringBuilder builder;
		private int filePos;
		private int remaining;

		public FileBuffer(string filePath)
			: this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
		}

		public FileBuffer(Stream stream)
		{
			Length = stream.Length;
			reader = new StreamReader(stream);
			builder = new StringBuilder();
		}

		public long Length { get; }

		public int Position
		{
			get => filePos - remaining;
			set => remaining = filePos - value;
		}

		public void Dispose()
		{
			reader.Dispose();
			GC.SuppressFinalize(this);
		}

		public string GetString(int beg, int end)
		{
			var length = end - beg;
			var start = filePos - builder.Length - beg;
			var result = builder.ToString(start, length);
			builder.Remove(start, length);
			return result;
		}

		public int Peek()
		{
			int result;
			if (remaining > 0)
				result = builder[^remaining];
			else if (filePos < Length)
				result = reader.Peek();
			else
				result = Buffer.Eof;
			return result;
		}

		public int Read()
		{
			int result;
			if (remaining > 0)
			{
				result = builder[builder.Length - remaining--];
			}
			else if (filePos < Length)
			{
				result = reader.Read();
				builder.Append((char)result);
				filePos++;
			}
			else
			{
				result = Buffer.Eof;
			}
			return result;
		}

		public string PeekLine()
		{
			var result = new StringBuilder();
			for (var i = builder.Length - remaining; i < builder.Length; i++)
			{
				var ch = builder[i];
				if (ch == '\n') return result.ToString();
				if (ch != '\r') result.Append(ch);
			}

			while (filePos < Length)
			{
				var ch = reader.Read();
				filePos++;
				builder.Append((char)ch);
				remaining++;
				if (ch == '\n') break;
				if (ch != '\r') result.Append((char)ch);
			}
			return result.ToString();
		}

		public string Line(Position position) =>
			throw new NotImplementedException();
	}
}
