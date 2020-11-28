namespace Opal.Containers
{
    public class SimpleStringBuffer
    {
		private readonly string text;
		private int pos;
		public const char Eof = char.MaxValue;

		public SimpleStringBuffer(string text) =>
			this.text = text ?? string.Empty;

		public char NextChar()
		{
			return (pos < text.Length) ?
				text[pos++] :
				Eof;
		}
	}
}
