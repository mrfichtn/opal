namespace Opal.Templates
{
	public class NotCondition : ICondition
	{
		private readonly ICondition cond;
		public NotCondition(ICondition cond) =>
			this.cond = cond;

		public bool Eval(FormatContext context) =>
			!cond.Eval(context);
	}
}
