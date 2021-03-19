using Opal.Containers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opal.LR1
{
	public class TerminalSets
	{
        /// <summary>
        /// Calculates the set of first terminals that may occur for each symbol.
        /// (See algorithm FIRST, pg. 189):
        /// 1. If X is terminal, then FIRST(X) is X.
        /// 2. If X ⭢ ε is a production, then add ε to FIRST(X).
        /// 3. If X is nonterminal and X -> Y₁Y₂...Yₖ is a production, then place a in FIRST(X)
        ///    if for some i, a is in FIRST(Yᵢ), and ε is in all of the FIRST(Y₁),..., FIRST(Yᵢ₋₁):
        ///    that is, Y₁ ... Yᵢ₋₁ => ε.  If ε is in FIRST(Yⱼ) for all j = 1,2, ..., k, then add
        ///    ε to FIRST(X).  For example, everything in FIRST(Y₁) is surely in FIRST(X).  If Y₁
        ///    does not derive ε, then we add nothing more to FIRST(X), but if Y₁ => ε, the we
        ///    add FIRST(Y₂) and so on.
        /// </summary>
        private readonly HashSet<uint>[] _terminalSets;

		public TerminalSets(Grammar grammar)
		{
			var symbols = grammar.Symbols;
			_terminalSets = new HashSet<uint>[symbols.Count + 1];

            // Complete rule (1): if X is terminal, then FIRST(X) is X.
            foreach (var term in symbols.Where(x => x.IsTerminal))
                _terminalSets[term.Id] = new HashSet<uint> { term.Id };

            //for (var i = 0U; i < symbols.Count; i++)
            //	FindFirst(grammar, i);

            //New method
            var groups = grammar.GroupBy(x => x.Left.Id)
                .ToDictionary(x => x.Key, y => y.ToArray());
            foreach (var g in groups)
                FindFirst(grammar, groups, g.Key);
        }

        private HashSet<uint> FindFirst(Grammar grammar, 
            Dictionary<uint, Rule[]> groups, 
            uint symbolIndex)
        {
            var set = _terminalSets[symbolIndex];
            if (set == null)
            {
                set = _terminalSets[symbolIndex] = new HashSet<uint>();
                var rules = groups[symbolIndex];
                foreach (var rule in rules)
                {
                    if (rule.IsEpsilon)
                    {
                        set.Add(0);
                    }
                    else
                    {
                        foreach (var symbol in rule.Right)
                        {
                            if (symbol.Id != symbolIndex)
                            {
                                var rightSet = FindFirst(grammar, groups, symbol.Id);
                                set.UnionWith(rightSet);
                                if (!rightSet.Contains(0))
                                    break;
                            }
                        }
                    }
                }
            }
            return set;
        }

        public HashSet<uint> this[uint index] => _terminalSets[index];

        public bool HasEmpty(uint symbolId)
        {
            bool result;
            if (symbolId < _terminalSets.Length)
            {
                var set = _terminalSets[symbolId];
                result = (set != null) && set.Contains(0);
            }
            else
                result = false;
            return result;
        }

        private HashSet<uint> FindFirst(Grammar grammar, uint symbolIndex)
        {
            var set = _terminalSets[symbolIndex];
            if (set == null)
            {
                set = _terminalSets[symbolIndex] = new HashSet<uint>();
                foreach (var rule in grammar.Where(x => x.Left.Id == symbolIndex))
                {
                    if (rule.IsEpsilon)
                    {
                        set.Add(0);
                    }
                    else
                    {
                        foreach (var symbol in rule.Right)
                        {
                            if (symbol.Id != symbolIndex)
                            {
                                var rightSet = FindFirst(grammar, symbol.Id);
                                set.UnionWith(rightSet);
                                if (!rightSet.Contains(0))
                                    break;
                            }
                        }
                    }
                }
            }
            return set;
        }
    }
}
