using System;
using System.Text;

namespace Opal.LR1
{
    public class LR1Item: IEquatable<LR1Item>
	{
        private int _nextState;

        public LR1Item(Rule production, uint position, Symbol lookahead)
		{
			Production = production;
			Position = position;
			Lookahead = lookahead;
            _nextState = -1;
		}

        #region Properties

        public Rule Production { get; }
        public uint Position { get; }
        public Symbol Lookahead { get; }
        public bool AtEnd => (Position >= Production.Right.Length);

        #endregion

        public LR1Item Next()
		{
			return new LR1Item(Production, Position + 1, Lookahead);
		}

        public bool IsSymbol(Symbol symbol)
        {
            return IsSymbol(symbol.Id);
        }

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

        public bool NextSymbol(out Symbol symbol)
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

        public bool Equals(LR1Item other)
        {
            return (Production.Id == other.Production.Id) && (Position == other.Position) &&
                Lookahead.Equals(other.Lookahead);
        }


        public override bool Equals(object obj)
		{
			return obj is LR1Item && Equals((LR1Item)obj);
		}

		public override int GetHashCode()
		{
			return Production.Id.GetHashCode() ^ Position.GetHashCode() ^ Lookahead.GetHashCode();
		}

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

            if (_nextState >= 0)
                builder.Append(" => ").Append(_nextState);
        }

        public override string ToString()
		{
			var builder = new StringBuilder();
            AppendTo(builder);
			return builder.ToString();
		}

        internal void SetGoto(int stateIndex)
        {
            _nextState = stateIndex;
        }
    }
}
