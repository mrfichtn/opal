using System.Collections.Generic;

namespace Opal.Templates
{
	public class TemplateScanner : ScannerBase
	{
		public TemplateScanner(string text)
			: base(text)
		{ }

		public IEnumerable<IToken> Tokens()
		{
			while (NextToken(out var token))
				yield return token;
		}
		
		public bool NextToken(out IToken token)
		{
			builder.Length = 0;
			if (ch == StringBuffer.Eof)
			{
				token = null!;
				return false;
			}

			if (ch == '$') goto Macro1;

		TextToken:
			PushChar();
			if (ch == StringBuffer.Eof || ch == '$')
			{
				token = new TextToken(builder.ToString());
				return true;
			}
			goto TextToken;

		Macro1: // $
			switch (NextChar())
			{
				case StringBuffer.Eof:
					builder.Append('(');
					token = new TextToken(builder.ToString());
					return true;
				case '(':
					goto Macro2;
				default:
					builder.Append('$');
					goto TextToken;
			}

		Macro2: // $(
			switch (NextChar())
			{
				case StringBuffer.Eof:
					builder.Append("$(");
					goto TextToken;
				case ')':
					builder.Append("$");
					ch = '(';
					goto TextToken;
			}

		Macro3: // $(...
			switch (PushChar())
			{
				case ')': goto MacroEnd;
				case StringBuffer.Eof:
					builder.Insert(0, "$(");
					goto TextToken;
				default:
					goto Macro3;
			}

		MacroEnd: // $(...)
			NextChar();
			token = new MacroParser(builder.ToString())
				.Parse();
			return true;
		}
	}
}
