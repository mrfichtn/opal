using Generators;
using Opal.ParseTree;
using Opal.Templates;

namespace Opal
{
    public class ParserTemplateContext: ITemplateContext
    {
        private readonly Options options;
        private readonly Language language;
        private readonly Productions.Grammar grammar;
        private readonly LR1.LR1Parser lr1Parser;
        private readonly string ns;
        private readonly ScannerBuilder.ScannerWriter scanner;

        public ParserTemplateContext(Options options,
            string ns,
            Language language,
            Productions.Grammar grammar,
            LR1.LR1Parser lr1Parser,
            ScannerBuilder.ScannerWriter scanner)
        {
            this.options = options;
            this.ns = ns ?? language.Namespace.Value;
            this.language = language;
            this.grammar = grammar;
            this.lr1Parser = lr1Parser;
            this.scanner = scanner;
        }
        
        bool ITemplateContext.Condition(string varName) => options.HasOption(varName) ?? false;

        string? ITemplateContext.Include(string name) => string.Empty;

        bool ITemplateContext.WriteVariable(Generator generator, string varName)
        {
            bool result = true;
            switch (varName)
            {
                case "usings": language.Usings.Write(generator); break;
                case "namespace.start":
                    if (ns != null)
                    {
                        generator.Write("namespace ")
                            .WriteLine(ns)
                            .StartBlock();
                    }
                    break;
                case "productions":
                    generator.Indent(2);
                    options.TryGet("no_action", out var noAction);
                    if (grammar != null)
                        grammar.Write(generator, noAction!);
                    generator.UnIndent(2);
                    break;
                case "actions": lr1Parser.Actions.Write(generator); break;
                case "parser.symbols": lr1Parser.WriteSymbols(generator); break;
                case "scanner": scanner.Write(generator); break;
                case "scanner.states": scanner.WriteTokenEnum(generator); break;
                case "namespace.end":
                    if (ns != null)
                        generator.EndBlock();
                    break;
                case "namespace": generator.Write(ns); break;
                default:
                    if (options.TryGet(varName, out var value))
                        generator.Write(value!);
                    else
                        result = false;
                    break;
            }
            return result;
        }
    }
}
