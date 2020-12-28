using System;

namespace Opal.LR1
{
	public class Symbol: IEquatable<Symbol>
	{
		public Symbol(string name, uint id, bool terminal, string? text = null)
		{
			Name = name;
			Id = id;
			IsTerminal = terminal;
			Text = text;
		}

		public Symbol(string name, uint id)
		{
			Name = name;
			Id = id;
			IsTerminal = false;
		}

		#region Property

		public string Name { get; }
		public string? Text { get; }
        public uint Id { get; }
        public bool IsTerminal { get; set; }

		#endregion

		public string ParseSymbol =>
			string.IsNullOrEmpty(Text) ? Name : Text;

		public override string ToString() => Name;

		public bool Equals(Symbol? other) =>
			(other != null) && (Id == other.Id);

		public override bool Equals(object? other) => Equals(other as Symbol);


		public override int GetHashCode() =>
			Id.GetHashCode();
	}
}
