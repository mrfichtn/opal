namespace Opal.Templates
{
	public class IfThenElseToken : IToken
	{
		private readonly ICondition cond;
		private readonly IToken trueClause;
		private readonly IToken falseClause;

		public IfThenElseToken(ICondition cond,
			IToken trueClause,
			IToken falseClause)
		{
			this.cond = cond;
			this.trueClause = trueClause;
			this.falseClause = falseClause;
		}

		public void Write(FormatContext context)
		{
			if (!context.Write)
				return;
			if (cond.Eval(context))
				trueClause.Write(context);
			else
				falseClause.Write(context);
		}
	}
}
