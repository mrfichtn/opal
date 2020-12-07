namespace Opal.Templates
{
	public class Condition : ICondition
	{
		private readonly string symbol;
		public Condition(string symbol) =>
			this.symbol = symbol;

		public bool Eval(FormatContext context) =>
			context.TemplateContext.Condition(symbol);
	}

}
