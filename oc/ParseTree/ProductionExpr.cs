using Generators;
using System.Text;

namespace Opal.ParseTree
{
    public abstract class ProductionExpr: Segment
    {
        public ProductionExpr(Segment segment)
            : base(segment)
        {}


        public abstract string Name { get; }


        /// <summary>
        /// If an expression is a character or string, then this method adds the string to 
        /// the token section.  This is how inline tokens (keywords) are processed.
        /// </summary>
        public virtual void DeclareToken(DeclareTokenContext context)
        {}

        public virtual void AddImproptuDeclaration(ImproptuDeclContext context)
        { }


        public abstract Productions.TerminalBase Build(Productions.GrammarBuilder builder);

        public override string ToString() => Name;
    }

    public class SymbolProdExpr : ProductionExpr
    {
        public Identifier name;

        public SymbolProdExpr(Token t)
            : base(t)
        {
            name = new Identifier(t);
        }

        public SymbolProdExpr(Segment s, string name)
            : base(s)
        {
            this.name = new Identifier(s, name);
        }

        public override string Name => name.Value;

        public override Productions.TerminalBase Build(Productions.GrammarBuilder builder)
        {
            if (builder.TryFind(name.Value, out var id, out var isTerminal))
            {
                return isTerminal ?
                    new Productions.TerminalSymbol(name, id) :
                    new Productions.NonTerminalSymbol(name, id);
            }
            return builder.MissingSymbol(name);
        }
    }

    public class StringTokenProd : ProductionExpr
    {
        private readonly StringConst text;
        private string? name;
        private int id;

        public StringTokenProd(StringConst text)
            : base(text)
        {
            this.text = text;
        }

        public override string Name => name!;


        public override void DeclareToken(DeclareTokenContext context) =>
            (name, id) = context.AddDefinition(text);


        public override Productions.TerminalBase Build(Productions.GrammarBuilder builder) =>
            new Productions.TerminalString(text, name!, id);

        public override string ToString() => $"\"{text.Value.ToEsc()}\"";
    }

    public class CharTokenProd: ProductionExpr
    {
        private readonly CharConst ch;
        private readonly StringConst text;
        private string? name;
        private int id;

        public CharTokenProd(CharConst ch)
            : base(ch)
        {
            this.ch = ch;
            text = new StringConst(ch, new string(ch.Value, 1));
        }

        public override string Name => name!;

        public override void DeclareToken(DeclareTokenContext context) =>
            (name, id) = context.AddDefinition(text);


        public override Productions.TerminalBase Build(Productions.GrammarBuilder builder) =>
            new Productions.TerminalString(text,
                name!,
                id);

        public override string ToString() => $"\'{Opal.Containers.Strings.ToEsc(ch.Value)}\'";
    }

    public abstract class UnaryProdExpr: ProductionExpr
    {
        protected readonly ProductionExpr expr;

        public UnaryProdExpr(ProductionExpr expr, Segment segment)
            : base(new Segment(expr, segment))
        {
            this.expr = expr;
        }

        public override string Name => expr.Name + NameSuffix;

        protected abstract string NameSuffix { get; }

        public override void DeclareToken(DeclareTokenContext context) =>
            expr.DeclareToken(context);

        public override Productions.TerminalBase Build(Productions.GrammarBuilder builder)
        {
            var symbol = expr.Build(builder);
            return Build(builder, symbol);
        }

        protected virtual Productions.TerminalBase Build(
            Productions.GrammarBuilder grammarBuilder,
            Productions.TerminalBase symbol)
        {
            var name = symbol.Name + NameSuffix;
            var found = grammarBuilder.TryFind(name, out var id, out var isTerminal);
            if (!found)
                return grammarBuilder.MissingSymbol(new Identifier(this, name));

            return !isTerminal ?
                new Productions.NonTerminalSymbol(this, name, id) :
                new Productions.TerminalSymbol(this, name, id);
        }
    }

    public class QuestionProdExpr: UnaryProdExpr
    {
        public QuestionProdExpr(ProductionExpr expr, Segment segment)
            : base(expr, segment)
        {}

        protected override string NameSuffix => "_option";

        public override void AddImproptuDeclaration(ImproptuDeclContext context)
        {
            var baseName = Name;
            if (context.HasSymbol(baseName))
                return;

            var definitions = new ProdDefList(
                new ProdDef(new ProductionExprList(expr)))
            {
                new ProdDef()
            };

            var production = new Production(
                new Identifier(this, baseName),
                null,
                definitions);

            context.Add(production);
        }
    }

    public class PlusProdExpr: UnaryProdExpr
    {
        public PlusProdExpr(ProductionExpr expr, Segment segment)
            : base(expr, segment)
        { }

        protected override string NameSuffix => "_list";

        public override void AddImproptuDeclaration(ImproptuDeclContext context)
        {
            var baseName = Name;
            if (context.HasSymbol(baseName))
                return;

            if (!context.TryFindType(expr.Name, out var exprType))
                exprType = "Object";

            var actionType = new Identifier(exprType + "List");

            var definitions = new ProdDefList(
                new ProdDef(
                    new ProductionExprList(expr),
                    new ActionNewExpr(actionType,
                        new ActionArgs(new ActionArg(0)))),
                new ProdDef(
                    new ProductionExprList(
                        new SymbolProdExpr(this, baseName),
                        expr),
                    new ActionFuncExpr(
                        actionType,
                        new ActionArgs(new ActionArg(0), new ActionArg(1)))));

            var production = new Production(
                new Identifier(this, baseName),
                null,
                definitions);

            context.Add(production);
        }
    }

    public class StarProdExpr: UnaryProdExpr
    {
        public StarProdExpr(ProductionExpr expr, Segment segment)
            : base(expr, segment)
        { }

        protected override string NameSuffix => "_list";

        public override void AddImproptuDeclaration(ImproptuDeclContext context)
        {
            var baseName = Name;
            if (context.HasSymbol(baseName))
                return;

            if (!context.TryFindType(expr.Name, out var exprType))
                exprType = "Object";

            var actionType = new Identifier(exprType + "List");

            var definitions = new ProdDefList(
                new ProdDef(
                    new ProductionExprList(),
                    new ActionNewExpr(actionType,
                        new ActionArgs())),
                new ProdDef(
                    new ProductionExprList(
                        new SymbolProdExpr(this, baseName),
                        expr),
                    new ActionFuncExpr(
                        actionType,
                        new ActionArgs(new ActionArg(0), new ActionArg(1)))));

            var production = new Production(
                new Identifier(this, baseName),
                null,
                definitions);

            context.Add(production);
        }
    }
}
