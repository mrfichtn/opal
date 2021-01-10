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

        public virtual void DeclareToken(DeclareTokenContext context)
        {}

        public Productions.TerminalBase Build(ProductionContext context)
        {
            return Create(context);
        }

        public virtual void AddImproptuDeclaration(ImproptuDeclContext context)
        {}

        protected abstract Productions.TerminalBase Create(ProductionContext context);

        #region Properties


        #region Ignore Property
        public bool Ignore { get; protected set; }
        #endregion

        public bool IsTerminal { get; set; }

        #region CallMethod Property
        public bool CallMethod { get; set; }
        #endregion

        public string? PropName { get; set; }

        public Identifier? Type { get; set; }

        #endregion

        public override string ToString() => Name;

        public virtual bool WriteArg(IGenerator generator, bool wroteArg, int index, string type) =>
            wroteArg;

        public virtual void WriteType(StringBuilder builder, string? @default)
        {
        }
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

        protected override Productions.TerminalBase Create(ProductionContext context)
        {
            if (context.TryFind(name.Value, out var id, out var isTerminal))
            {
                return isTerminal ?
                    new Productions.TerminalSymbol(name, id) :
                    new Productions.NonTerminalSymbol(name, id);
            }
            return context.MissingSymbol(name);
        }

        //public override bool WriteSignature(IGenerator generator, bool wroteArg)
        //{
        //    if (wroteArg)
        //        generator.Write(", ");
        //    generator.Write($"{Name} {PropName}");
        //    return true;
        //}

        public override bool WriteArg(IGenerator generator, bool wroteArg, int index, string type)
        {
            if (wroteArg)
                generator.Write(", ");

            generator.Write("a{0}", index);
            return true;
        }

        public override void WriteType(StringBuilder builder, string? @default)
        {
            if (!string.IsNullOrEmpty(PropName))
                builder.Append('<').Append(PropName).Append('>');
            else if (!string.IsNullOrEmpty(@default))
                builder.Append('<').Append(@default).Append('>');
            else if (IsTerminal)
                builder.Append("<Token>");
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

        protected override Productions.TerminalBase Create(ProductionContext context) =>
            new Productions.TerminalString(text, name!, id);

        public override string ToString() => $"\"{text.Value.ToEsc()}\"";

        public override void WriteType(StringBuilder builder, string? @default)
        {
            if (!string.IsNullOrEmpty(@default))
                builder.Append('<').Append(@default).Append('>');
            else
                builder.Append("<Token>");
        }
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

        protected override Productions.TerminalBase Create(ProductionContext context) =>
            new Productions.TerminalString(text,
                name!, 
                id);

        public override string ToString() => $"\'{Opal.Containers.Strings.ToEsc(ch.Value)}\'";

        public override void WriteType(StringBuilder builder, string? @default)
        {
            if (!string.IsNullOrEmpty(@default))
                builder.Append('<').Append(@default).Append('>');
            else
                builder.Append("<Token>");
        }
    }

    public abstract class UnaryProdExpr: ProductionExpr
    {
        protected readonly ProductionExpr expr;

        public UnaryProdExpr(ProductionExpr expr, Segment segment)
            : base(new Segment(expr, segment))
        {
            this.expr = expr;
        }

        public override void DeclareToken(DeclareTokenContext context) =>
            expr.DeclareToken(context);

        protected sealed override Productions.TerminalBase Create(ProductionContext context)
        {
            var symbol = expr.Build(context);
            return Create(context, symbol);
        }

        protected abstract Productions.TerminalBase Create(ProductionContext context,
            Productions.TerminalBase symbol);
    }
    
    public class QuestionProdExpr: UnaryProdExpr
    {
        public QuestionProdExpr(ProductionExpr expr, Segment segment)
            : base(expr, segment)
        {}

        public override string Name => expr.Name + "_option";


        public override void AddImproptuDeclaration(ImproptuDeclContext context)
        {
            var baseName = Name;
            if (context.HasSymbol(baseName))
                return;

            var definitions = new ProdDefList(
                new ProdDef(new ProductionExprList(
                    new SymbolProdExpr(expr, expr.Name))));
            definitions.Add(new ProdDef());

            var production = new Production(
                new Identifier(this, baseName),
                null,
                definitions);

            context.Add(production);
        }


        protected override Productions.TerminalBase Create(ProductionContext context, 
            Productions.TerminalBase symbol)
        {
            var baseName = symbol.Name + "_option";
            string name = baseName;
            int id;
            while (true)
            {
                var found = context.TryFind(name, out id, out var isTerminal);
                if (found)
                {
                    if (!isTerminal)
                        return new Productions.NonTerminalSymbol(this,
                            name,
                            id);
                }
                else
                {
                    break;
                }
            }
            //if (!context.TryFindProd(name, out var id))
            //{
            //    internalProds.AddOption(name, expr);
            //}

            //var segment = new Segment(expr.Start, qualifier.End);
            //if (!productions.Any(x => x.Name == optionName))
            //    internalProds.AddOption(optionName, expr);
            //return new SymbolProdExpr(segment, optionName);


            return new Productions.TerminalSymbol(this, name, id);
        }

        //public static ProductionExpr CreateOption(ProductionExpr expr, Token qualifier)
        //{
        //    var optionName = expr.Name + "_option";
        //    var segment = new Segment(expr.Start, qualifier.End);
        //    if (!productions.Any(x => x.Name == optionName))
        //        internalProds.AddOption(optionName, expr);
        //    return new SymbolProdExpr(segment, optionName);
        //}
    }
}
