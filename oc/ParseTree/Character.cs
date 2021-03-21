namespace Opal.ParseTree
{
    public class Character: Segment
    {
        public readonly Identifier name;
        public readonly CharacterExpr expr;
        
        public Character(Token name, CharacterExpr expr)
            : this(new Identifier(name), expr)
        { }

        public Character(Identifier name, CharacterExpr expr)
            : base(name.Start, expr.End)
        {
            this.name = name;
            this.expr = expr;
        }

        public bool TryAdd(Nfa.CharacterClassBuilder builder)
        {
            var result = expr.Create(builder, out var match);
            if (result)
                builder.Add(name, match!);
            return result;
        }

        public override string ToString() => $"{name} = {expr}";
    }
}
