namespace Opal.Templates
{
    public class TextToken : IToken
	{
		private readonly string text;

		public TextToken(string text) =>
			this.text = text;

		public void Write(FormatContext context) =>
			context.WriteBlock(text);
	}
}
