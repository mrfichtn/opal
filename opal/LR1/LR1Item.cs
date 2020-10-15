using System;
using System.Text;

namespace Opal.LR1
{
    public class LR1Item: IEquatable<LR1Item>, IComparable<LR1Item>
	{

        public LR1Item(Rule production, uint position, Symbol lookahead)
		{
			Production = production;
			Position = position;
			Lookahead = lookahead;
            NextState = -1;
		}

        public Rule Production { get; }
        public uint Position { get; }
        public Symbol Lookahead { get; }
        public bool AtEnd => (Position >= Production.Right.Length);

        public int NextState { get; private set; }


        public LR1Item Next() => new LR1Item(Production, Position + 1, Lookahead);

        public bool IsSymbol(Symbol symbol) => IsSymbol(symbol.Id);

        /// <summary>
        /// Returns true if the expression to the right of the position marker matches
        /// symbolId
        /// </summary>
        /// <param name="position"></param>
        /// <param name="symbolId"></param>
        /// <returns></returns>
        public bool IsSymbol(uint symbolId)
        {
            bool result;
            var rule = Production;
            if (Position < rule.Right.Length)
            {
                var symbol = rule.Right[Position];
                result = (symbolId == symbol.Id);
            }
            else
            {
                result = false;
            }
            return result;
        }

        public bool NextSymbol(out uint symbol)
        {
            var rule = Production;
            var result = (Position < rule.Right.Length);
            symbol = result ? rule.Right[Position].Id : 0;
            return result;
        }

        public bool NextSymbol(out Symbol? symbol)
        {
            var rule = Production;
            var result = (Position < rule.Right.Length);
            symbol = result ? rule.Right[Position] : null;
            return result;
        }

        public bool HasNextSymbol(Symbol symbol)
        {
            var rule = Production;
            return (Position < rule.Right.Length) && (rule.Right[Position] == symbol);
        }

        public int CompareTo(LR1Item other)
        {
            var cmp = Production.Id.CompareTo(other.Production.Id);
            if (cmp != 0)
                return cmp;
            cmp = Position.CompareTo(other.Position);
            if (cmp != 0)
                return cmp;
            return Lookahead.Id.CompareTo(other.Lookahead.Id);
        }


        public bool Equals(LR1Item other) =>
            (Production.Id == other.Production.Id) && (Position == other.Position) &&
                Lookahead.Equals(other.Lookahead);

        public override bool Equals(object obj) =>
			(obj is LR1Item item) && Equals(item);

		public override int GetHashCode() =>
			Production.Id.GetHashCode() ^ Position.GetHashCode() ^ Lookahead.GetHashCode();

        public void AppendTo(StringBuilder builder)
        {
            builder.Append(Production.Left.Value);
            builder.Append(": ");
            int i = 0;
            var min = Math.Min(Position, Production.Right.Length);

            for (; i < min; i++)
            {
                builder.Append(Production.Right[i].Value);
                builder.Append(" ");
            }

            builder.Append("·");

            for (; i < Production.Right.Length; i++)
            {
                builder.Append(" ");
                builder.Append(Production.Right[i].Value);
            }

            builder.Append(", ");
            builder.Append(Lookahead.Id == 0 ? "＄" : Lookahead.Value);

            if (NextState >= 0)
                builder.Append(" => ").Append(NextState);
        }

        public override string ToString() => ToString(true);

        public string ToString(bool showTransition) =>
            new StringBuilder().Append(this, showTransition).ToString();

        internal void SetGoto(int stateIndex) => NextState = stateIndex;

    }

    public static class LR1ItemExt
    {
        public static StringBuilder Append(this StringBuilder builder,
            LR1Item item, bool showTransition)
        {
            var production = item.Production;
            
            builder.Append(production.Left.Value);
            builder.Append(": ");
            int i = 0;
            var min = Math.Min(item.Position, item.Production.Right.Length);

            for (; i < min; i++)
            {
                builder.Append(production.Right[i].Value);
                builder.Append(" ");
            }

            builder.Append("·");

            for (; i < production.Right.Length; i++)
            {
                builder.Append(" ");
                builder.Append(production.Right[i].Value);
            }

            builder.Append(", ");
            builder.Append(item.Lookahead.Id == 0 ? "＄" : item.Lookahead.Value);

            if (showTransition && (item.NextState >= 0))
                builder.Append(" => ").Append(item.NextState);

            return builder;
        }
    }
}
