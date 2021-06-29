using System;
using System.IO;

namespace Opal
{
    public class StringBuffer : IBuffer
	{
		private readonly string text;
		private int position;
		private int tokenStart;

		public StringBuffer(string text) => this.text = text;

		public static StringBuffer FromFile(string filePath)
        {
			var text = File.ReadAllText(filePath);
			return new StringBuffer(text);
        }

		public void Dispose() => GC.SuppressFinalize(this);

		public long Length => text.Length;
		public int Position => position;

		public int Read() => (position < text.Length) ? text[position++] : -1;
		
		public string PeekLine()
		{
			if (position >= text.Length)
				return string.Empty;
			int i;
			for (i = position; i < text.Length; i++)
			{
				var ch = text[i];
				if (ch == '\r' || ch == '\n')
					break;
			}
			return text.Substring(position, i - position);
		}

		public string GetToken(int length)
		{
			var result = text.Substring(tokenStart, length);
			tokenStart += length;
			position = tokenStart;
			return result;
		}

		public string Line(Position position)
        {
			int i;
			for (i = position.Ch; i < text.Length; i++)
            {
				var ch = text[i];
				if (ch == '\r' || ch == '\n')
					break;
            }
			var start = position.Ch - position.Col + 1;
			var length = i - start;
			return (length > 0) ?
				text.Substring(start, length) :
				string.Empty;
        }
	}
}
