using Opal.Nfa;

namespace Opal.ParseTree
{
    public abstract class TokenExpr
    {
        public abstract Graph BuildGraph(TokenBuilderContext context);
    }

    public class SymbolTokenExpr: TokenExpr
    {
        private readonly Identifier name;
        
        public SymbolTokenExpr(Token name)
            : this(new Identifier(name))
        { }
        
        public SymbolTokenExpr(Identifier name)
        {
            this.name = name;
        }

        public override Graph BuildGraph(TokenBuilderContext context)
        {
            var found = context.TryFindMatch(name, out var match);
            return found ?
                context.Graph.Create(match!) :
                context.Graph.Create();
        }
    }

    public class CharTokenExpr: TokenExpr
    {
        private readonly CharConst ch;

        public CharTokenExpr(CharConst ch) =>
            this.ch = ch;

        public override Graph BuildGraph(TokenBuilderContext context) =>
            context.Graph.Create(new SingleChar(ch.Value));
    }

    public class CharClassTokenExpr: TokenExpr
    {
        private readonly CharClass cc;

        public CharClassTokenExpr(CharClass ch) =>
            this.cc = ch;

        public override Graph BuildGraph(TokenBuilderContext context) =>
            context.Graph.Create(cc);
    }

    public class StringLiteralTokenExpr: TokenExpr
    {
        private readonly StringConst str;

        public StringLiteralTokenExpr(StringConst str) =>
            this.str = str;

        public override Graph BuildGraph(TokenBuilderContext context) =>
            context.Graph.Create(str);
    }

    public abstract class UnaryTokenExpr: TokenExpr
    {
        private readonly TokenExpr expr;

        public UnaryTokenExpr(TokenExpr expr) =>
            this.expr = expr;

        public override sealed Graph BuildGraph(TokenBuilderContext context) =>
            BuildGraph(context, expr.BuildGraph(context));

        protected abstract Graph BuildGraph(TokenBuilderContext context, Graph graph);
    }

    public class PlusClosureExpr: UnaryTokenExpr
    {
        public PlusClosureExpr(TokenExpr expr)
            : base(expr)
        { }

        protected override Graph BuildGraph(TokenBuilderContext context, Graph graph) =>
            Graph.PlusClosure(graph);
    }

    public class StarClosureExpr: UnaryTokenExpr
    {
        public StarClosureExpr(TokenExpr expr)
            : base(expr)
        { }

        protected override Graph BuildGraph(TokenBuilderContext context, Graph graph) =>
            Graph.StarClosure(graph);
    }

    public class QuestionClosureExpr: UnaryTokenExpr
    {
        public QuestionClosureExpr(TokenExpr expr)
            : base(expr)
        { }

        protected override Graph BuildGraph(TokenBuilderContext context, Graph graph) =>
            Graph.QuestionClosure(graph);
    }

    public class QuantifierTokenExpr: UnaryTokenExpr
    {
        private readonly Integer integer;

        public QuantifierTokenExpr(TokenExpr expr, Integer integer)
            : base(expr)
        {
            this.integer = integer;
        }

        protected override Graph BuildGraph(TokenBuilderContext context, Graph graph) =>
            Graph.Quantifier(graph, integer);
    }

    public class RangeTokenExpr: UnaryTokenExpr
    {
        private readonly Integer max;
        private readonly Integer min;

        public RangeTokenExpr(TokenExpr expr, Integer min, Integer max)
            : base(expr)
        {
            this.min = min;
            this.max = max;
        }

        protected override Graph BuildGraph(TokenBuilderContext context, Graph graph) =>
            Graph.RangeQuantifier(graph, min, max);
    }

    public abstract class BinaryTokenExpr: TokenExpr
    {
        private readonly TokenExpr left;
        private readonly TokenExpr right;

        public BinaryTokenExpr(TokenExpr left, TokenExpr right)
        {
            this.left = left;
            this.right = right;
        }

        public sealed override Graph BuildGraph(TokenBuilderContext context)
        {
            var leftGraph = left.BuildGraph(context);
            var rightGraph = right.BuildGraph(context);
            return BuildGraph(leftGraph, rightGraph);
        }

        protected abstract Graph BuildGraph(Graph leftGraph, Graph rightGraph);
    }
    
    public class ConcatTokenExpr: BinaryTokenExpr
    {
        public ConcatTokenExpr(TokenExpr left, TokenExpr right)
            : base(left, right)
        { }

        protected override Graph BuildGraph(Graph leftGraph, Graph rightGraph) =>
            Graph.Concatenate(leftGraph, rightGraph);
    }

    public class UnionTokenExpr: BinaryTokenExpr
    {
        public UnionTokenExpr(TokenExpr left, TokenExpr right)
            : base(left, right)
        { }

        protected override Graph BuildGraph(Graph leftGraph, Graph rightGraph) =>
            Graph.Union(leftGraph, rightGraph);
    }
}
