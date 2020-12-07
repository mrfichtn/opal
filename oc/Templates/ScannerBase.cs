using System.Text;

namespace Opal.Templates
{
    public class ScannerBase
	{
		private readonly StringBuffer buffer;
		protected readonly StringBuilder builder;
		protected char ch;

		protected ScannerBase(string text)
		{
			buffer = new StringBuffer(text);
			builder = new StringBuilder();
			ch = buffer.NextChar();
		}

		protected char NextChar()
		{
			ch = buffer.NextChar();
			return ch;
		}

		protected char PushChar()
		{
			builder.Append(ch);
			ch = buffer.NextChar();
			return ch;
		}
	}
}
