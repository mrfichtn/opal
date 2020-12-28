using Generators;
using Opal.ParseTree;
using Opal.Templates;
using System;

namespace Opal
{
    public class ParserTemplateContext: ITemplateContext
    {
        private readonly Options options;
        private readonly Language language;
        private readonly Productions.Grammar grammar;
        private readonly LR1.LR1Parser lr1Parser;
        private readonly IGeneratable scannerWriter;
        private readonly Dfa.Dfa dfa;
        private readonly bool emitTokenStates;
        private readonly string ns;

        public ParserTemplateContext(Options options,
            string ns,
            Language language,
            Productions.Grammar grammar,
            LR1.LR1Parser lr1Parser,
            IGeneratable scannerWriter,
            Dfa.Dfa dfa)
        {
            this.options = options;
            this.ns = ns ?? language.Namespace.Value;
            this.language = language;
            this.grammar = grammar;
            this.lr1Parser = lr1Parser;
            this.scannerWriter = scannerWriter;
            this.dfa = dfa;

            emitTokenStates = (!options.TryGet("scanner", out var scannerValue) ||
                    scannerValue!.Equals("state", StringComparison.InvariantCultureIgnoreCase));
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
                case "actions": lr1Parser!.Actions.Write(generator); break;
                case "parser.symbols": lr1Parser!.WriteSymbols(generator); break;
                case "scanner": scannerWriter!.Write(generator); break;
                case "scanner.states": dfa!.WriteTokenEnum(generator, emitTokenStates); break;
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
