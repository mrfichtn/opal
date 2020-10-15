namespace Opal
{
	public class Token : Segment
	{
		public int State;
		public string Value;

		public Token() { }

		public Token(int line, int column, int ch)
			: base(new Position(line, column, ch)) { }

		public Token Set(int state, string value, int ln, int col, int ch)
		{
			State = state;
			Value = value;
			End = new Position(ln, col, ch);
			return this;
		}

		public static implicit operator string(Token t)
		{
			return t.Value;
		}

		public override string ToString()
		{
			return string.Format("({0},{1}): '{2}', state = {3}", Start.Ln, Start.Col, Value, State);
		}
	}

}
