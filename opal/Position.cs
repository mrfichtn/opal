using System;

namespace Opal
{
	///<summary>Location of a character within a file</summary>
	public struct Position : IEquatable<Position>
	{
		public readonly int Ln;
		public readonly int Col;
		public readonly int Ch;

		public Position(int ln, int col, int ch)
		{
			Ln = ln;
			Col = col;
			Ch = ch;
		}

		public override int GetHashCode() => Ch.GetHashCode();
		public override bool Equals(object obj) => Equals((Position)obj);
		public bool Equals(Position other) => Ch == other.Ch;
		public override string ToString() => $"Ln {Ln}, Col {Col}, Ch {Ch}";
		public static bool operator ==(Position left, Position right) => (left.Ch == right.Ch);
		public static bool operator !=(Position left, Position right) => (left.Ch != right.Ch);
	}
}
