using Generators;
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

		public Grammar(Productions.Grammar grammar)
		{
			Symbols = new Symbols();

			Symbols.AddSymbols(grammar.Symbols);

			if (!Symbols.TryFind(grammar.Start, out var startSym))
			{
				startSym = Symbols.Create(
					name: grammar.Productions[0].Name,
					isTerminal: false);
			}

            //Create kernal rule
            var languageSymbol = new Symbol(name:"S'", 
				id:(uint)Symbols.Count, 
				terminal:false);
            var rule = new Rule(this, 0, languageSymbol, new[] { startSym });
            rules = new List<Rule> { rule };

            foreach (var prod in grammar.Productions)
			{
				var left = Symbols.Create(prod.Name, false);
				var right = prod.Right
					.Select(x => Symbols[x.Name])
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
                builder.Append('R')
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
}
