namespace Opal.Templates
{
	public class IfToken : IToken
	{
		private readonly ICondition cond;
		public IfToken(ICondition cond) =>
			this.cond = cond;

		public void Write(FormatContext context)
		{
			var write = context.Write && cond.Eval(context);
			context.Push(write);
		}
	}
}
