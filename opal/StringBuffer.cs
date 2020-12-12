using System;
using System.IO;

namespace Opal
{
    public class StringBuffer : IBuffer
	{
		private readonly string text;

		public StringBuffer(string text) => this.text = text;

		public static StringBuffer FromFile(string filePath)
        {
			var text = File.ReadAllText(filePath);
			return new StringBuffer(text);
        }

		public void Dispose() => GC.SuppressFinalize(this);

		public long Length => text.Length;
		public int Position { get; set; }

		public int Read() => (Position < text.Length) ? text[Position++] : -1;
		
		public int Peek() => (Position < text.Length) ? text[Position] : -1;

		public string PeekLine()
		{
			int i;
			for (i = Position; i < text.Length; i++)
			{
				var ch = text[i];
				if (ch == '\r' || ch == '\n')
					break;
			}
			return text.Substring(Position, i - Position + 1);
		}

		public string GetString(int start, int end) => 
			text.Substring(start, end-start);
	}
}
