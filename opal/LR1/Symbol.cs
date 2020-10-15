using System;

namespace Opal.LR1
{
	public class Symbol: IEquatable<Symbol>
	{
		public Symbol(string value, uint id, bool terminal)
		{
			Value = value;
			Id = id;
			IsTerminal = terminal;
		}

        #region Property

        public string Value { get; }
        public uint Id { get; }
        public bool IsTerminal { get; set; }

        #endregion

        public override string ToString()
		{
			return string.Format("{0}", Value, Id);
		}

		public bool Equals(Symbol other)
		{
			return (other != null) && (Id == other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}
