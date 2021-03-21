namespace Opal.Productions
{
    public class Symbol
    {
        public Symbol(string name, bool terminal, string? text = null)
        {
            Name = name;
            Terminal = terminal;
            Text = text;
        }

        public readonly string Name;
        public readonly bool Terminal;
        public readonly string? Text;

        public override string ToString() => Name;
    }

}
