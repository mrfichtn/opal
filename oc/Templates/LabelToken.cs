namespace Opal.Templates
{
	public class LabelToken : IToken
	{
		private readonly string symbol;
		public LabelToken(string symbol) =>
			this.symbol = symbol;

		public void Write(FormatContext context) =>
			context.WriteVar(symbol);
	}
}
