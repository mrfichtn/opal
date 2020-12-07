namespace Opal.Templates
{
	public class IncludeToken : IToken
	{
		private readonly string symbol;
		public IncludeToken(string symbol) =>
			this.symbol = symbol;

		public void Write(FormatContext context) =>
			context.Include(symbol);
	}
}
