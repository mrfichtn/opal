using System.Text;

namespace Opal.Nfa
{
    /// <summary>
    /// Non-finite automata node.  
    /// 
    ///     State           Left            Right       Match
    ///     -----------------------------------------------------
    ///     None            -1              -1          -1
    ///     Single ε        -1              ε-index     -1
    ///     One transition  trans-index     -1          class-id
    ///     Both            trans-index     ε-index     class-id
    ///     Two ε           ε-index         ε-index     -1
    /// </summary>
    public struct NfaNode
    {
        public int Left;
        public int Right;
        public int Match;

        public NfaNode(int match, int left, int right)
        {
            Match = match;
            Left = left;
            Right = right;
        }

        public bool IsSingleEpsilon => (Left == -1) && (Right != -1);

        public bool IsEmpty => (Left == -1) && (Right == -1);

        public bool RemoveRight()
        {
            var isEmpty = (Left == -1);
            if (isEmpty)
            {
                Right = -1;
            }
            else if (Match == -1)
            {
                Right = Left;
                Left = -1;
            }
            else
            {
                Right = -1;
            }
            return isEmpty;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Left != -1)
                builder.Append(Left.ToString("D3"));
            else
                builder.Append("   ");

            builder.Append(' ');

            if (Right != -1)
                builder.Append(Right.ToString("D3"));
            else
                builder.Append("   ");

            if (Match != -1)
                builder.Append(' ').Append(Match);

            return builder.ToString();
        }

        public string ToString(Machine machine, int index)
        {
            var builder = new StringBuilder();
            builder.Append(index.ToString("D3"))
                .Append(" [");

            if (machine.AcceptingStates.TryFind(index, out var acceptingState))
                builder.Append(acceptingState.ToString("000"));
            else
                builder.Append("   ");
            builder.Append("] ");

            if (Left != -1)
                builder.Append(Left.ToString("D3"));
            else
                builder.Append("   ");

            builder.Append(' ');

            if (Right != -1)
                builder.Append(Right.ToString("D3"));
            else
                builder.Append("   ");

            if (Match != -1)
                builder.Append(' ').Append(machine.Matches.Find(Match));

            return builder.ToString();
        }
    }
}
