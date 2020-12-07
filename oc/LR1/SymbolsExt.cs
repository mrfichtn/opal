using Opal.Containers;
using System.Text;

namespace Opal.LR1
{
    public static class SymbolsExt
    {
        public static StringBuilder AppendTo(this StringBuilder builder,
            Symbols symbols)
        {
            foreach (var symbol in symbols)
            {
                builder.AppendFormat("[{0}] = {1}", symbol.Id, symbol.Name)
                    .AppendIf(symbol.IsTerminal, "(T)")
                    .AppendLine();
            }
            return builder;
        }
    }
}
