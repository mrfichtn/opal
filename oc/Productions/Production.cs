using Generators;
using System.Text;

namespace Opal.Productions
{
    public class Production
    {
        private readonly ParseTree.Identifier name;
        private readonly ParseTree.ActionExpr action;
        private bool ignore;
        private ParseTree.Identifier? type;
        
        public Production(ParseTree.Identifier name,
            int id,
            int ruleId,
            ParseTree.ProductionAttr attr,
            ParseTree.ActionExpr action,
            TerminalBase[] right)
        {
            this.name = name;
            Id = id;
            RuleId = ruleId;
            this.action = action;
            Right = right;

            if (attr != null)
            {
                SetAttribute(attr.Option);
                CallMethod = attr.IsMethod;
            }
        }

        private void SetAttribute(ParseTree.Identifier attr)
        {
            if (attr.Value == "ignore")
                ignore = true;
            else
                type = attr;
        }

        public bool CallMethod { get; private set; }


        public string Name => name.Value;
        public int RuleId { get; set;  }
        public int Id { get; }

        public string? Type => type?.Value;

        public TerminalBase[] Right { get; }

        public void Write(Generator generator, 
            Grammar grammar,
            NoActionOption option)
        {
            var rightCount = Right.Length;

            generator.Write("case {0}:", RuleId)
                .WriteLine($" // {this}")
                .Indent();
            //.StartBlock();

            //if (rightCount != 0)
            //    generator.WriteLine("state = _stack.SetItems({0})", rightCount).Write("    .Reduce({0}, ", Id);
            //else
            //    generator.Write("state = _stack.Push({0}, ", Id);
            if (rightCount != 0)
            {
                generator.WriteLine($"items = {rightCount};");
                generator.Write($"state = Reduce({Id}, ");
            }
            else
            {
                generator.Write($"state = Push({Id}, ");
            }

            if (action != null)
                action.Write(new ActionWriteContext(generator, grammar, this, true));
            else
                WriteAttributed(generator, grammar, option);

            generator.WriteLine(");")
                .WriteLine("break;")
                .UnIndent();
            //.EndBlock();
        }

        public void WriteAttributed(IGenerator generator, 
            Grammar parent, 
            NoActionOption option)
        {
            var ignore = this.ignore || (Right.Length == 0 && type == null);
            var retType = type ?? name;
            var finalArgs = new StringBuilder();
            string? first = null;
            var argc = 0;
            for (var i = 0; i < Right.Length; i++)
            {
                var right = Right[i];
                if (right.Ignore)
                {
                    continue;
                }
                else if (right.CallMethod)
                {
                    finalArgs.Append(right.PropName)
                        .Append('(')
                        .Append("At");
                    if (right.Type != null)
                        finalArgs.Append('<').Append(right.Type.Value).Append('>');
                    finalArgs.Append('(').Append(i).Append("))");
                }
                else
                {
                    parent.TryFindDefault(right.Name, out var type);
                    finalArgs.Append("At");
                    right.WriteType(finalArgs, type);
                    finalArgs.Append('(').Append(i).Append(')');
                }
                if (argc++ == 0)
                {
                    if (right.CallMethod)
                        first = finalArgs.ToString();
                    else
                        first = $"At({i})";
                }
                finalArgs.Append(',');
            }
            if (finalArgs.Length > 0)
                finalArgs.Length--;

            if (ignore)
            {
                generator.Write("null");
            }
            else if (type == null)
            {
                if (argc == 1)
                    generator.Write("{0}", first!);
                else
                    switch (option)
                    {
                        case NoActionOption.First:
                            generator.Write("{0}", first ?? string.Empty);
                            break;
                        case NoActionOption.Null:
                            generator.Write("null");
                            break;
                        case NoActionOption.Tuple:
                            generator.Write("Tuple.Create({0})", finalArgs);
                            break;
                    }
            }
            else if (CallMethod)
            {
                generator
                    .Write(retType.Value)
                    .Write('(')
                    .Write(finalArgs.ToString())
                    .Write(")");
            }
            else if (retType.Value == "false" || retType.Value == "true")
            {
                generator.Write("{0}", retType.Value);
            }
            else
            {
                generator.Write("new {0}(", retType)
                        .Write(finalArgs.ToString())
                        .Write(")");
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Name)
                .Append(" =");

            foreach (var item in Right)
                builder.Append(' ').Append(item);
            return builder.ToString();
        }
    }

    public static class ProductionExt
    {
        public static Generator Write(this Generator generator, 
            Production production,
            Grammar grammar,
            NoActionOption noAction)
        {
            production.Write(generator, grammar, noAction);
            return generator;
        }
    }
}
