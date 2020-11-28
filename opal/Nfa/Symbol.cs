namespace Opal.Nfa
{
    public class Symbol
    {
        public Symbol(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public Symbol(string name, int index, string text)
            : this(name, index)
        {
            Text = text;
        }

        public string Name { get; }
        public int Index { get; }

        public string? Text { get; }

        public override string ToString() => $"Nfa{Index}: {Name}";
    }
}
