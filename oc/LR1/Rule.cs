using Generators;
using System.Collections.Generic;
using System.Text;

namespace Opal.LR1
{
    /// <summary>
    /// Represents a production
    /// </summary>
    public class Rule
	{
        public Rule(Grammar grammar, int id, Symbol left, params Symbol[] right)
		{
			Grammar = grammar;
			Id = (uint)id;
			Left = left;
			Right = right;
		}

        /// <summary>
        /// Parent grammar (set of all rules)
        /// </summary>
        public Grammar Grammar { get; }

        /// <summary>
        /// Id
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Name of reduction of this rule
        /// </summary>
        public Symbol Left { get; }

        /// <summary>
        /// Input of rule
        /// </summary>
        public Symbol[] Right { get; }

        /// <summary>
        /// Returns true if rule is an ε (empty) production
        /// </summary>
        public bool IsEpsilon => (Right.Length == 0); 

        /// <summary>
        /// Find all terminals that may follow pos
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="lookahead"></param>
        /// <returns></returns>
        public HashSet<uint> FindFirst(uint pos, Symbol lookahead)
        {
            var firstSet = new HashSet<uint>();
            if (pos < Right.Length)
            {
                do
                {
                    var rightSym = Right[pos];
                    var nextTerms = Grammar.TerminalSets[rightSym.Id];
                    foreach (var term in nextTerms)
                        firstSet.Add(term);
                    if (!nextTerms.Contains(0))
                        break;
                } while (++pos < Right.Length);

                if (firstSet.Contains(0))
                    firstSet.Add(lookahead.Id);
            }
            else
            {
                firstSet.Add(lookahead.Id);
            }
            return firstSet;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            AppendTo(builder);
            return builder.ToString();
        }

        public void AppendTo(StringBuilder builder)
        {
            builder.Append(Left)
                .Append(" -> ");
            foreach (var item in Right)
                builder.Append(item).Append(" ");
            builder.Length--;
        }
    }

    public static class RuleExt
    {
        public static T Write<T>(this Generator<T> generator, Rule rule)
            where T : Generator<T> =>
            generator.WriteLine($"Prod{rule.Left}");
    }
}
