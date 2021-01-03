using Generators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Opal.Productions
{
    public class Grammar
    {
        private readonly TypeTable typeTable;
        
        public Grammar(string start,
            List<Symbol> symbols,
            Production[] productions,
            TypeTable typeTable)
        {
            Start = start;
            Symbols = symbols;
            Productions = productions;
            this.typeTable = typeTable;
        }

        public bool TryFindDefault(string name, out string? type) =>
            typeTable.TryFind(name, out type);

        public string Start { get; }

        public Production[] Productions { get; }
        public List<Symbol> Symbols { get; }

        public void Write(Generator generator, string noAction)
        {
            var option = GetOption(noAction);
            generator.Indent(1);
            foreach (var item in Productions)
                generator.Write(item, this, option);
            generator.UnIndent(1);
        }

        private static NoActionOption GetOption(string noAction)
        {
            NoActionOption option;
            if (string.IsNullOrEmpty(noAction))
                option = NoActionOption.First;
            else if (noAction.Equals("first", StringComparison.InvariantCultureIgnoreCase))
                option = NoActionOption.First;
            else if (noAction.Equals("tuple", StringComparison.InvariantCultureIgnoreCase))
                option = NoActionOption.Tuple;
            else
                option = NoActionOption.Null;
            return option;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var production in Productions)
                builder.Append(production).AppendLine();
            return builder.ToString();
        }
    }

    public static class GrammarExt
    {
        public static IGenerator Write(this Generator generator, 
            Grammar grammar, 
            string noAction)
        {
            grammar.Write(generator, noAction);
            return generator;
        }
    }
}
