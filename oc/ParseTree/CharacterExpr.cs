using Opal.Nfa;

namespace Opal.ParseTree
{
    public abstract class CharacterExpr: Segment
    {
        protected CharacterExpr(Segment segment)
            : base(segment)
        { }

        protected CharacterExpr(Position start, Position end)
            : base(start, end)
        { }


        public abstract bool Create(CharacterContext context, 
            out IMatch? match);

        public virtual void LogMissing(CharacterContext context)
        { }
    }

    public abstract class CharacterBinaryExpr: CharacterExpr
    {
        private readonly CharacterExpr left;
        private readonly CharacterExpr right;
        
        public CharacterBinaryExpr(CharacterExpr left, CharacterExpr right)
            : base(left.Start, right.End)
        {
            this.left = left;
            this.right = right;
        }

        public sealed override bool Create(CharacterContext context, out IMatch? match)
        {
            var result = left.Create(context, out var leftMatch);
            if (result)
            {
                result = right.Create(context, out var rightMatch);
                match = result ? Create(leftMatch!, rightMatch!) : null;
            }
            else
                match = null;
            return result;
        }

        public sealed override void LogMissing(CharacterContext context)
        {
            left.LogMissing(context);
            right.LogMissing(context);
        }

        protected abstract IMatch Create(IMatch leftMatch, IMatch rightMatch);
    }

    public class CharacterDiffExpr: CharacterBinaryExpr
    {
        public CharacterDiffExpr(CharacterExpr left, CharacterExpr right)
            : base(left, right)
        { }

        protected override IMatch Create(IMatch leftMatch, IMatch rightMatch) =>
            leftMatch.Difference(rightMatch);
    }

    public class CharacterUnionExpr: CharacterBinaryExpr
    {
        public CharacterUnionExpr(CharacterExpr left, CharacterExpr right)
            : base(left, right)
        { }

        protected override IMatch Create(IMatch leftMatch, IMatch rightMatch)
            => leftMatch.Union(rightMatch);
    }

    public class CharacterInvertExpr: CharacterExpr
    {
        private readonly CharacterExpr expr;
        public CharacterInvertExpr(Token t, CharacterExpr expr)
            : base(t.Start, expr.End)
        {
            this.expr = expr;
        }

        public override bool Create(CharacterContext context, out IMatch? match)
        {
            var result = expr.Create(context, out match);
            if (result)
                match = match!.Invert();
            return result;
        }

        public override void LogMissing(CharacterContext context) =>
            expr.LogMissing(context);
    }

    public class CharacterClass: CharacterExpr
    {
        private readonly CharClass charClass;

        public CharacterClass(CharClass charClass)
            : base(charClass)
        {
            this.charClass = charClass;
        }

        public override bool Create(CharacterContext context, out IMatch? match)
        {
            match = charClass;
            return true;
        }
    }

    public class CharacterChar: CharacterExpr
    {
        private readonly CharConst ch;

        public CharacterChar(CharConst ch)
            : base(ch)
        {
            this.ch = ch;
        }

        public override bool Create(CharacterContext context, 
            out IMatch? match)
        {
            match = new SingleChar(ch);
            return true;
        }
    }

    public class CharacterSymbol: CharacterExpr
    {
        private readonly Identifier name;

        public CharacterSymbol(Token name)
            : this(new Identifier(name))
        { }
        
        public CharacterSymbol(Identifier name)
            : base(name)
        {
            this.name = name;
        }

        public override bool Create(CharacterContext context, out IMatch? match) =>
            context.TryFind(name, out match);

        public override void LogMissing(CharacterContext context) =>
            context.LogMissing(name);
    }
}
