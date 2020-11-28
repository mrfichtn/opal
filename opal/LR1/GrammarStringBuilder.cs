using Opal.Containers;
using System.Text;

namespace Opal.LR1
{
	public static class GrammarStringBuilder
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
					builder.AppendFormat("[{0}] = {1}", symbol.Id, symbol.Name)
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
