using Opal.Nfa;

namespace Opal.ParseTree
{
    public class NamedCharClass
    {
        public NamedCharClass(Token identifier, IMatch chars)
        {
            Name = new Identifier(identifier);
            Chars = chars.Reduce();
        }

        public Identifier Name { get; set; }
        public IMatch Chars { get; set; }
    }
}
