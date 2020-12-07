namespace Opal
{
	public class Token : Segment
	{
		public int State;
		public string Value;

		public Token(int line, int column, int ch)
			: base(new Position(line, column, ch)) 
		{
			Value = string.Empty;
		}

		public Token(Position start, Position end, int state, string value)
			: base(start, end)
        {
			State = state;
			Value = value;
        }

		public static implicit operator string(Token t) => t.Value;

		public override string ToString() =>
			string.Format("({0},{1}): '{2}', state = {3}", Start.Ln, Start.Col, Value, State);
	}
}
