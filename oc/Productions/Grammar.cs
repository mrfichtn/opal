using Generators;
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

        public void Write<T>(T generator, string noAction)
            where T: Generator<T>
        {
            generator.Indent(1);
            foreach (var item in Productions)
                item.Write(generator);
            generator.UnIndent(1);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var production in Productions)
                builder.Append(production).AppendLine();
            return builder.ToString();
        }
    }
}
