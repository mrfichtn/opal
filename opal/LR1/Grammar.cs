using Generators;
using Opal.Containers;
using Opal.ParseTree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.LR1
{
    public class Grammar: IEnumerable<Rule>, IGeneratable
	{
        private readonly List<Rule> rules;

		public Grammar(ProductionList productions)
		{
			Symbols = new Symbols();

			foreach (var state in productions.Symbols.Skip(1))
				Symbols.Create(state, true);

			if (!Symbols.TryFind(productions.Language!.Value, out var startSym))
				startSym = Symbols[productions[0].Left.Value];

            //Create kernal rule
            var languageSymbol = new Symbol("S'", (uint)Symbols.Count, terminal:false);
            var rule = new Rule(this, 0, languageSymbol, new[] { startSym });
            rules = new List<Rule> { rule };

            foreach (var prod in productions)
			{
				var left = Symbols.Create(prod.Left.Value, false);
				var right = prod.Right
					.Select(x => Symbols[x.Id])
					.ToArray();
                prod.RuleId = rules.Count;
				rules.Add(new Rule(this, rules.Count, left, right));
			}
			TerminalSets = new TerminalSets(this);
		}

		public Grammar(params string[] symbols)
		{
			rules = new List<Rule>();
			Symbols = new Symbols();

			var startSym = Symbols.Create(symbols[0], false);
			var languageSymbol = new Symbol("S'", (uint)Symbols.Count, terminal: false);
			var rule = new Rule(this, 0, languageSymbol, new[] { startSym });
			rules.Add(rule);

			Symbol? left = null;
			var right = new List<Symbol>();
			foreach (var symValue in symbols)
			{
				var itemIsEmpty = string.IsNullOrEmpty(symValue);

				if (left == null)
				{
					if (itemIsEmpty)
						continue;
					left = Symbols.Create(symValue, false);
				}
				else if (itemIsEmpty)
				{
					var production = new Rule(this, rules.Count, left, right.ToArray());
					rules.Add(production);
					left = null;
					right.Clear();
				}
				else
				{
					right.Add(Symbols.Create(symValue, true));
				}
			}

			TerminalSets = new TerminalSets(this);
		}

        #region Properties

        public Rule this[uint index] => rules[(int)index];
        public int Count => rules.Count;
        public Symbols Symbols { get; }
		public static Symbol EndOfLine => Symbols.EndOfLine;
        public TerminalSets TerminalSets { get; }

        #endregion

        public void AppendTo(StringBuilder builder)
        {
			builder.AppendLine("Symbols:")
				.AppendTo(Symbols)
				.AppendLine();
            
			for (int i = 0; i < rules.Count; i++)
            {
                builder.Append("R")
                    .Append(i)
                    .Append(": ");
                rules[i].AppendTo(builder);
                builder.AppendLine();
            }
        }

		public override string ToString() => ToString(true);

		public string ToString(bool showSymbols)
        {
			return new StringBuilder()
				.AppendTo(this, showSymbols)
				.ToString();
        }

        public IEnumerator<Rule> GetEnumerator() => rules.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => rules.GetEnumerator();

		public void Write(Generator generator)
		{
			foreach (var prod in rules)
				prod.Write(generator);
		}
	}

	public static class GrammarExt
    {
		public static StringBuilder AppendTo(this StringBuilder builder, 
			Grammar grammar,
			bool showSymbols)
		{
			if (showSymbols)
			{
				builder.AppendLine("Symbols:");
				foreach (var symbol in grammar.Symbols)
				{
					builder.AppendFormat("[{0}] = {1}", symbol.Id, symbol.Value)
						.AppendIf(symbol.IsTerminal, "(T)")
						.AppendLine();
				}
				builder.AppendLine();
			}
			for (uint i = 0; i < grammar.Count; i++)
			{
				builder.Append("R")
					.Append(i)
					.Append(": ");
				grammar[i].AppendTo(builder);
				builder.AppendLine();
			}
			return builder;
		}
	}
}
