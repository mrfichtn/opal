namespace Opal.Templates
{
	public class MacroScanner : ScannerBase
	{
		public MacroScanner(string text)
			: base(text)
		{
		}

		public bool NextToken(out string symbol)
		{
			builder.Length = 0;
		StartSymbol:
			if (ch == StringBuffer.Eof)
			{
				symbol = string.Empty;
				return false;
			}
			if (ch == ',') goto Comma;
			if (char.IsWhiteSpace(ch))
			{
				NextChar();
				goto StartSymbol;
			}
		Symbol:
			PushChar();
			if ((ch == StringBuffer.Eof) || char.IsWhiteSpace(ch))
			{
				symbol = builder.ToString();
				return true;
			}
			if (ch == ',')
				goto Comma;
			goto Symbol;

		Comma:
			NextChar();
			symbol = builder.ToString();
			return true;
		}
	}
}
