using Generators;
using System.Text;

namespace Opal.ParseTree
{
    public class ProductionExpr: Segment
    {
        public ProductionExpr(Segment s)
            : base(s)
        {
            _quantifier = Quantifier.One;
        }

        #region Properties

        #region Quantifier Property
        public Quantifier Quantifier
        {
            get { return _quantifier; }
            set { _quantifier = value; }
        }
        private Quantifier _quantifier;
        #endregion

        #region Id Property
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private int _id;
        #endregion

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

        public static ProductionExpr SetAttributes(ProductionExpr expr, ProductionAttr attr)
        {
            if (attr != null)
            {
                expr.SetOption(attr.Option);
                if (attr.IsMethod)
                {
                    expr.CallMethod = true;
                    if (attr.ArgType != null)
                        expr.Type = attr.ArgType;
                }
            }
            return expr;
        }

        public override string ToString()
        {
            return _quantifier switch
            {
                Quantifier.Plus => "+",
                Quantifier.Question => "?",
                Quantifier.Star => "*",
                _ => string.Empty,
            };
        }

        public virtual bool WriteSignature(IGenerator generator, bool wroteArg) =>
            wroteArg;

        public virtual bool WriteArg(IGenerator generator, bool wroteArg, int index, string type) =>
            wroteArg;

        public virtual void WriteType(StringBuilder builder, string? @default)
        {
        }


        public bool SetOption(Identifier option)
        {
            bool isOk = true;
            switch (option.Value)
            {
                case "ignore": Ignore = true; break;
                default: PropName = option.Value; break;
            }
            return isOk;
        }
    }

    public class SymbolProd : ProductionExpr
    {
        public SymbolProd(Token t)
            : base(t)
        {
            Name = t.Value!;
        }

        public SymbolProd(Segment s, string name, int id = 0)
            : base(s)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }

        public void SetTerminal(int state)
        {
            Id = state;
            IsTerminal = true;
        }

        public override bool WriteSignature(IGenerator generator, bool wroteArg)
        {
            if (wroteArg)
                generator.Write(", ");
            generator.Write($"{Name} {PropName}");
            return true;
        }

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

        public override string ToString() => Name + base.ToString();
    }

    public class StringTokenProd : ProductionExpr
    {
        public StringTokenProd(Segment seg, string token, int id = 0)
            : base(seg)
        {
            Text = token;
            Id = id;
            IsTerminal = true;
        }

        public StringTokenProd(StringConst str, int id)
            : base(str)
        {
            Text = str.Value;
            Id = id;
            IsTerminal = true;
        }

        public string Text { get; }

        public override string ToString() => $"\"{Text.ToEsc()}\"";

        public override bool WriteSignature(IGenerator generator, bool wroteArg)
        {
            if (!Ignore)
            {
                generator.Write($"Token {PropName}");
                wroteArg = true;
            }
            return wroteArg;
        }

        public override bool WriteArg(IGenerator generator, bool wroteArg, int index, string type)
        {
            if (!Ignore)
            {
                if (wroteArg)
                    generator.Write(", ");
                wroteArg = true;
                generator.Write("a{0}", index);
            }

            return wroteArg;
        }

        public override void WriteType(StringBuilder builder, string? @default)
        {
            if (!string.IsNullOrEmpty(@default))
                builder.Append('<').Append(@default).Append('>');
            else
                builder.Append("<Token>");
        }
    }
}
