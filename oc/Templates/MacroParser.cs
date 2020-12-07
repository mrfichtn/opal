namespace Opal.Templates
{
    public class MacroParser
	{
		private readonly MacroScanner scanner;
		private string token;

		public MacroParser(string source)
		{
			scanner = new MacroScanner(source);
			token = string.Empty;
		}

		public IToken Parse()
		{
			if (!NextToken())
				return new NullToken();

			return token switch
			{
				"if" => If(),
				"include" => Include(),
				"endif" => new EndIfToken(),
				"else" => new ElseToken(),
				_ => new LabelToken(token)
			};
		}

		private IToken If()
		{
			if (!NextToken())
				return new NullToken();

			ICondition cond;
			if (string.IsNullOrEmpty(token))
				cond = new FalseCondition();
			else if (token[0] == '!')
				cond = new NotCondition(new Condition(token[1..]));
			else
				cond = new Condition(token);

			if (!NextToken())
				return new IfToken(cond);

			var trueClause = IfClause();
			var falseClause = NextToken() ?
				IfClause() :
				new NullToken();
			return new IfThenElseToken(cond, trueClause, falseClause);
		}

		private IToken IfClause()
		{
			return token switch
			{
				"include" => Include(),
				_ => new LabelToken(token)
			};
		}

		private IToken Include()
		{
			if (!NextToken())
				return new NullToken();
			return new IncludeToken(token);
		}

		private bool NextToken() => scanner.NextToken(out token);
	}
}
