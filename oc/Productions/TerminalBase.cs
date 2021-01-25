using Opal.Containers;
using Opal.ParseTree;
using System.Text;

namespace Opal.Productions
{
    public abstract class TerminalBase: Segment
    {
        public TerminalBase(Segment segment, string name, int id)
            : base(segment)
        {
            Name = name;
            Id = id;
        }

        public TerminalBase(Identifier name, int id)
            : this(name, name.Value, id)
        {
        }

        public string Name { get; }
        public int Id { get; }

        public abstract bool IsTerminal { get; }

        public virtual void WriteType(StringBuilder finalArgs, string? @default)
        {}

        public override string ToString() => Name;

        public IReductionExpr Reduce(ReduceContext context) =>
            context.TryFindType(Name, out var type) ?
                new CastedArgReductionExpr(0, type!) :
                new ArgReductionExpr(0);

        public virtual IReductionExpr Reduction(Grammar grammar) =>
            grammar.TryFindDefault(Name, out var type) ?
                new CastedArgReductionExpr(0, type!) :
                new ArgReductionExpr(0);
    }

    public class TerminalSymbol: TerminalBase
    {
        public TerminalSymbol(Segment segment, string name, int id)
            : base(segment, name, id)
        { 
        }

        public TerminalSymbol(Identifier name, int id)
            : base(name, id)
        { }

        public override bool IsTerminal => true;

        public override void WriteType(StringBuilder builder, string? @default)
        {
            if (!string.IsNullOrEmpty(@default))
                builder.Append('<').Append(@default).Append('>');
            else if (IsTerminal)
                builder.Append("<Token>");
        }
    }

    public class TerminalString: TerminalSymbol
    {
        private readonly string text;

        public TerminalString(StringConst text, string name, int id)
            : base(text, name, id)
        {
            this.text = text.Value;
        }

        public override bool IsTerminal => true;

        public override string ToString() => $"\"{text.ToEsc()}\"";

        //public override bool WriteArg(IGenerator generator, bool wroteArg, int index, string type)
        //{
        //    if (!Ignore)
        //    {
        //        if (wroteArg)
        //            generator.Write(", ");
        //        wroteArg = true;
        //        generator.Write("a{0}", index);
        //    }

        //    return wroteArg;
        //}

        public override void WriteType(StringBuilder builder, string? @default)
        {
            if (!string.IsNullOrEmpty(@default))
                builder.Append('<').Append(@default).Append('>');
            else
                builder.Append("<Token>");
        }

    }

    public class NonTerminalSymbol: TerminalBase
    {
        public NonTerminalSymbol(Segment segment, string name, int id)
            : base(segment, name, id)
        {
        }

        public NonTerminalSymbol(Identifier name, int id)
            : base(name, id)
        { }

        public override bool IsTerminal => false;

        public override void WriteType(StringBuilder builder, string? @default)
        {
            if (!string.IsNullOrEmpty(@default))
                builder.Append('<').Append(@default).Append('>');
            else if (IsTerminal)
                builder.Append("<Token>");
        }
    }

    public class MissingSymbolTerminal: TerminalBase
    {
        public MissingSymbolTerminal(Identifier name)
            : base(name, -1)
        {
        }

        public override bool IsTerminal => false;
    }
}
