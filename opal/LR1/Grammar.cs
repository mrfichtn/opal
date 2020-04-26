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
        private readonly List<Rule> _rules;
		private readonly Dictionary<string, Symbol> _byName;

        static Grammar()
		{
			EndOfLine = new Symbol(string.Empty, 0, true);
		}

		public Grammar(ProductionList productions)
		{
			_byName = new Dictionary<string, Symbol>()
			{
				{ EndOfLine.Value, EndOfLine }
			};
			Symbols = new List<Symbol> { EndOfLine };

			foreach (var state in productions.Symbols.Skip(1))
				GetSymbol(state, true);

            if (!_byName.TryGetValue(productions.Language.Value, out var startSym))
                startSym = _byName[productions[0].Left.Value];

            //Create kernal rule
            var languageSymbol = new Symbol("@language", (uint)Symbols.Count, false);
            var rule = new Rule(this, 0, languageSymbol, new[] { startSym });
            _rules = new List<Rule> { rule };

            foreach (var prod in productions)
			{
				var left = GetSymbol(prod.Left.Value, false);
				var right = prod.Right
					.Select(x => Symbols[x.Id])
					.ToArray();
                prod.RuleId = _rules.Count;
				_rules.Add(new Rule(this, _rules.Count, left, right));
			}
			TerminalSets = new TerminalSets(this);
		}

		public Grammar(params string[] symbols)
		{
			_rules = new List<Rule>();
			_byName = new Dictionary<string, Symbol>()
			{
				{ EndOfLine.Value, EndOfLine }
			};

			Symbols = new List<Symbol>()
			{  EndOfLine };

			Symbol left = null;
			var right = new List<Symbol>();
			foreach (var symValue in symbols)
			{
				var itemIsEmpty = string.IsNullOrEmpty(symValue);

				if (left == null)
				{
					if (itemIsEmpty)
						continue;
					left = GetSymbol(symValue, false);
				}
				else if (itemIsEmpty)
				{
					var production = new Rule(this, _rules.Count, left, right.ToArray());
					_rules.Add(production);
					left = null;
					right.Clear();
				}
				else
				{
					right.Add(GetSymbol(symValue, true));
				}
			}

			TerminalSets = new TerminalSets(this);
		}

        #region Properties

        public Rule this[uint index] => _rules[(int)index];
        public int Count => _rules.Count;
        public List<Symbol> Symbols { get; }
        public static Symbol EndOfLine { get; private set; }
        public TerminalSets TerminalSets { get; }

        #endregion

        public void Add(Rule rule)
        {
            _rules.Add(rule);
        }

        public void AppendTo(StringBuilder builder)
        {
            builder.AppendLine("Symbols:");
            foreach (var symbol in Symbols)
                builder.AppendFormat("[{0}] = {1}", symbol.Id, symbol.Value)
                    .AppendLine();
            builder.AppendLine();
            for (int i = 0; i < _rules.Count; i++)
            {
                builder.Append("R")
                    .Append(i)
                    .Append(": ");
                _rules[i].AppendTo(builder);
                builder.AppendLine();
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            AppendTo(builder);
            return builder.ToString();
        }

        public IEnumerator<Rule> GetEnumerator() => _rules.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _rules.GetEnumerator();

		private Symbol GetSymbol(string symbolValue, bool isTerminal)
		{
            if (!_byName.TryGetValue(symbolValue, out Symbol symbol))
            {
                symbol = new Symbol(symbolValue, (uint)Symbols.Count, isTerminal);
                _byName.Add(symbolValue, symbol);
                Symbols.Add(symbol);
            }
            else if (!isTerminal && symbol.IsTerminal)
            {
                symbol.IsTerminal = isTerminal;
            }
            return symbol;
		}

		public void Write(Generator generator)
		{
			foreach (var prod in _rules)
				prod.Write(generator);
		}
	}
}
