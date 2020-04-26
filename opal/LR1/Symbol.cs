using System;

namespace Opal.LR1
{
	public class Symbol: IEquatable<Symbol>
	{
		public Symbol(string value, uint id, bool isTerminal)
		{
			_value = value;
			_id = id;
			_isTerminal = isTerminal;
		}

		#region Property

		#region Value Property
		public string Value
		{
			get { return _value; }
		}
		private readonly string _value;
		#endregion

		#region Id Property
		public uint Id
		{
			get { return _id; }
		}
		private readonly uint _id;
		#endregion

		#region IsTerminal Property
		public bool IsTerminal
		{
			get { return _isTerminal; }
			set { _isTerminal = value; }
		}
		private bool _isTerminal;
		#endregion

		#endregion

		public override string ToString()
		{
			return string.Format("{0}({1})", _value, _id);
		}

		public bool Equals(Symbol other)
		{
			return (other != null) && (_id == other._id);
		}

		public override int GetHashCode()
		{
			return _id.GetHashCode();
		}
	}
}
