using Opal.Nfa;

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

        public bool TryAdd(CharacterContext context)
        {
            var result = expr.Create(context, out var match);
            if (result)
                context.Add(name, match!);
            return result;
        }

        public override string ToString() =>
            $"{name} = {expr}";
    }
}
